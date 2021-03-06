﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCrontab;
using mpBackup.MpGUI;
using mpBackup.MpUtilities;

namespace mpBackup
{
    /// <summary>
    /// Used to perform backup operations such as scanning directories, preparing files and using backup handlers to store them in the cloud.
    /// </summary>
    public class MpBackupProcess
    {
        public BackupProcessState currentState { get; private set; }
        public MpSettingsManager settingsManager;

        private GoogleBackupProvider googleBackup;
        private bool initialized = false;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Task backupTask;

        public MpMessageQueue messageQueue;

        public event EventHandler<string> UserAuthenticationRequired;
        public event EventHandler InitializationFailed;

        /// <summary>
        /// Represents a task that is currently being waited on, with a list of its children (if any) as:
        ///     Tuple<PARENT, CHILDREN>
        /// Where:
        ///     PARENT - is the task currently blocking monitoring, with the appropriate cancellation token source
        ///     CHILDREN - (optional) list of all tasks spawned by PARENT. PARENT cannot transition to completed state until all CHILDREN complete
        ///         (appropriate CancellationTokens need to be passed into children task, to make sure all can be cancelled from within this list).
        /// </summary>
        private Tuple<Tuple<Task, CancellationTokenSource>, List<Tuple<Task,CancellationTokenSource>>> currentTask;

        private FileSystemWatcher backupDirWatcher;
        private bool directoryChangesDetected;

        public MpBackupProcess(MpMessageQueue messageQueue, MpSettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;
            this.messageQueue = messageQueue;
            if (!this.settingsManager.settings.continuousMonitoring && this.settingsManager.settings.nextBackupTime == null)
            {
                setNextBackupTime();
            }
            this.settingsManager.SettingsSaved += settingsManager_SettingsSaved;
        }

        #region Public Methods
        /// <summary>
        /// Initialize the backup process. This checks all settings are valid and initializes backup providers. MUST be called before monitor().
        /// This method will attempt to initialize backup providers which can take a long time (http requests).
        /// </summary>
        /// <returns></returns>
        public async void initialize()
        {
            this.currentState = BackupProcessState.INITIALIZING;
            if (this.settingsManager.isValid)
            {
                this.currentTask = new Tuple<Tuple<Task, CancellationTokenSource>, List<Tuple<Task, CancellationTokenSource>>>(new Tuple<Task, CancellationTokenSource>(null, new CancellationTokenSource()), null);
                bool initResult = await initializeBackupProviders(this.currentTask.Item1.Item2.Token);
                 this.currentTask = null;
                if (initResult)
                {
                    initializeFSMonitor();
                    this.initialized = true;
                    this.currentState = BackupProcessState.READY;
                    return;
                }
                else
                {
                    this.currentState = BackupProcessState.FAILED;
                    if (InitializationFailed != null) InitializationFailed(this, EventArgs.Empty);
                    return;
                }
            }
            this.currentState = BackupProcessState.FAILED;
            throw new Exception("Attempted to initialize the backup provider with invalid settings.");
        }

        public async void monitor()
        {
            if (!this.initialized) throw new Exception("The backup process has not been initialized.");
            log.Info("Backup monitoring started.");
            displayGUIMessage("Backup monitoring started.");
            this.currentState = BackupProcessState.MONITORING;
            while (currentState != BackupProcessState.STOP_REQUESTED)
            {
                if (currentState == BackupProcessState.MONITORING)
                {
                    if (this.directoryChangesDetected && (this.settingsManager.settings.continuousMonitoring 
                        || (DateTime.Compare(this.settingsManager.settings.nextBackupTime, DateTime.Now) <= 0)))
                    {
                        this.currentTask = new Tuple<Tuple<Task, CancellationTokenSource>, List<Tuple<Task, CancellationTokenSource>>>
                            (new Tuple<Task, CancellationTokenSource>(null, new CancellationTokenSource()), new List<Tuple<Task,CancellationTokenSource>>());
                        await performBackup(this.currentTask.Item1.Item2.Token);
                        this.currentTask = null;
                    }
                }
                Thread.Sleep(100);
            }
            this.currentState = BackupProcessState.STOPPED;
            log.Info("Backup monitoring stopped.");
        }

        /// <summary>
        /// Cancel all running tasks, if any. This method will block until all tasks transition to a complete state.
        /// </summary>
        public void cancelRunningTasks()
        {
            log.Debug("Cancelling all running tasks.");
            if (this.currentTask != null)
            {
                lock (this.currentTask)
                {
                    if (this.currentTask.Item2 != null)
                    {
                        log.Debug("Waiting for [" + currentTask.Item2.Count + "] children tasks to finish cleanly.");
                        foreach (var childTask in currentTask.Item2)
                        {
                            childTask.Item2.Cancel();
                        }
                        while (currentTask.Item2.Count(t => !t.Item1.IsCompleted) != 0)
                        {
                            Thread.Sleep(50);
                        }
                    }
                    this.currentTask.Item1.Item2.Cancel();
                }
                this.currentTask = null;
            }
        }

        public void stop()
        {
            BackupProcessState tempState = this.currentState;
            this.currentState = BackupProcessState.STOP_REQUESTED;
            if (tempState.IsIn(new List<BackupProcessState>() { BackupProcessState.MONITORING, BackupProcessState.AWAIT_AUTH, BackupProcessState.AWAIT_SETTINGS }))
            {
                // The backup process is currently inside monitor()
                cancelRunningTasks();
            }
            else
            {
                this.currentState = BackupProcessState.STOPPED;
            }
                
        }
        #endregion
        #region Private Methods
        /// <summary>
        /// Initialize the file system monitor to monitor the directory currently saved in user settings.
        /// </summary>
        private void initializeFSMonitor()
        {
            log.Debug("Initializing a new File System Monitor.");
            DirectoryInfo backupDir = new DirectoryInfo(this.settingsManager.settings.backupFolderPath);
            this.backupDirWatcher = new FileSystemWatcher(backupDir.FullName);
            this.backupDirWatcher.Error += watcher_Error;
            this.backupDirWatcher.Changed += watcher_Changed;
            this.backupDirWatcher.Created += watcher_Changed;
            this.backupDirWatcher.Renamed += watcher_Changed;
            this.backupDirWatcher.EnableRaisingEvents = true;
            // Whenever a new directory is added to the watch list, a backup is forced:
            this.directoryChangesDetected = true;
        }
        /// <summary>
        /// Attempt to authenticate the google backup provider when the application is executed, so we can ensure valid credentials for later.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> initializeBackupProviders(CancellationToken cancellationToken)
        {
            bool result;
            this.googleBackup = new GoogleBackupProvider(this);
            this.googleBackup.AuthenticationRequired += googleBackup_AuthenticationRequired;
            try
            {
                result = await this.googleBackup.initialize(cancellationToken);
            }
            catch (Exception e)
            {
                log.Error("An error was caught: ", e);
                result = false;
            }
            return result;
        }

        private async Task performBackup(CancellationToken cancellationToken)
        {
            // TODO consider passing cancellation token to children tasks as a global cancel
            this.settingsManager.setLastBackupTime(DateTime.Now);
            List<MpFileUpload> filesToUpload = await compareFiles(this.settingsManager.settings.backupFolderPath, cancellationToken);
            if (filesToUpload.Count != 0)
            {
                // Lock the list of children tasks until we're finished spawning them
                lock (this.currentTask.Item2)
                {
                    foreach (MpFileUpload file in filesToUpload)
                    {
                        Task fileUpload = new Task(() => this.googleBackup.uploadFile(file, file.uploadCancellation.Token));
                        file.uploadTask = fileUpload;
                        fileUpload.Start();
                        this.currentTask.Item2.Add(new Tuple<Task, CancellationTokenSource>(fileUpload, file.uploadCancellation));
                    }
                }
                log.Info("Waiting for [" + filesToUpload.Count + "] uploads to finish.");
                while (filesToUpload.Count(f => !f.uploadTask.IsCompleted) > 0)
                {
                    Thread.Sleep(100);
                }
            }
            log.Info("Backup task finished.");
            setNextBackupTime();
        }

        /// <summary>
        /// Compare offline files with ones stored online and return a list of files that need to be uploaded.
        /// </summary>
        /// <param name="fullPath"></param>
        private async Task<List<MpFileUpload>> compareFiles(string fullPath, CancellationToken cancellationToken)
        {
            List<string> onlineFiles = new List<string>();
            onlineFiles = await this.googleBackup.getUploadedFileNames(cancellationToken);
            DirectoryInfo dir = new DirectoryInfo(fullPath);
            List<string> offlineFiles = dir.GetFiles().Select(f => f.Name).ToList();
            // We've obtained a list of offline files, mark the directory as unchanged
            this.directoryChangesDetected = false;
            List<MpFileUpload> filesToUpload = new List<MpFileUpload>();
            foreach (string fileName in offlineFiles.Except(onlineFiles))
            {
                filesToUpload.Add(new MpFileUpload()
                    {
                        fileName = fileName,
                        fullPath = fullPath + "\\" + fileName,
                        fileExtension = fileName.Substring(fileName.IndexOf('.') + 1)
                    });
            }
            log.Info("Found [" + filesToUpload.Count + "] files to upload.");
            return filesToUpload;
        }

        private void setNextBackupTime()
        {
            if (!this.settingsManager.settings.continuousMonitoring)
            {
                this.settingsManager.setNextBackupTime(CrontabSchedule.Parse(this.settingsManager.settings.backupScheduleCron).GetNextOccurrence(DateTime.Now));
                log.Info("Next backup to occur at: " + this.settingsManager.settings.nextBackupTime.ToString());
                this.settingsManager.saveSettings(false);
            }
        }

        /// <summary>
        /// Display a baloon text message from the tray icon.
        /// </summary>
        /// <param name="msg"></param>
        private void displayGUIMessage(string msg)
        {
            messageQueue.addMessageAsync(new MpMessage()
            {
                text = msg,
                displayAs = MpMessage.DisplayAs.BALOON
            });
        }
        #endregion
        #region Event Handlers
        void googleBackup_AuthenticationRequired(object sender, string authenticationUrl)
        {
            if (UserAuthenticationRequired != null) UserAuthenticationRequired(sender, authenticationUrl);
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.directoryChangesDetected = true;
        }

        private void watcher_Error(object sender, ErrorEventArgs e)
        {
            log.Error("The FileSystemWatcher raised an error: ", e.GetException());
            if (!Directory.Exists(this.settingsManager.settings.backupFolderPath))
            {
                this.currentState = BackupProcessState.AWAIT_SETTINGS;
                this.backupDirWatcher.Dispose();
                this.backupDirWatcher = null;
                // Invalidate the settings
                this.settingsManager.invalidate();
            }
        }
        private void settingsManager_SettingsSaved(bool interactive, bool backupFolderChanged)
        {
            if (interactive && backupFolderChanged)
            {
                log.Debug("Folder change detected.");
                cancelRunningTasks();
                initializeFSMonitor();
                if (currentState == BackupProcessState.AWAIT_SETTINGS)
                {
                    this.currentState = BackupProcessState.MONITORING;
                }
            }
        }
        #endregion
        /// <summary>
        /// Indicates the current state of MpBackupProcess
        /// </summary>
        public enum BackupProcessState
        {
            INITIALIZING,
            /// <summary>
            /// Initialization failed, the MpBackupProcess object is safe to be disposed.
            /// </summary>
            FAILED,
            /// <summary>
            /// The backup process is ready to start monitoring.
            /// </summary>
            READY,
            /// <summary>
            /// Backup process is waiting for valid settings
            /// </summary>
            AWAIT_SETTINGS,
            /// <summary>
            /// One or more backup providers requires authentication.
            /// The backup process will block until user specifies valid credentials
            /// </summary>
            AWAIT_AUTH,
            /// <summary>
            /// The file system is being monitored for changes
            /// </summary>
            MONITORING,
            /// <summary>
            /// A backup is currently being performed
            /// </summary>
            BACKUP_RUNNING,
            /// <summary>
            /// An external thread requested the backup process to stop
            /// </summary>
            STOP_REQUESTED,
            /// <summary>
            /// The backup proess is stopped.
            /// </summary>
            STOPPED
        }
    }
}
