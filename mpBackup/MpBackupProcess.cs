using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCrontab;
using mpBackup.MpGUI;

namespace mpBackup
{
    /// <summary>
    /// Used to perform backup operations such as scanning directories, preparing files and using backup handlers to store them in the cloud.
    /// </summary>
    public class MpBackupProcess
    {
        public bool stopMonitoring = false;
        public bool isGUIReady = false;
        public CustomApplicationContext mpGUIContext;
        public MpConfig config;

        private GoogleBackupProvider googleBackup;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Task backupTask;
        public bool backupRunning = false;

        public MpMessageQueue messageQueue;
        /// <summary>
        /// Collection of tokens for all currently running tasks (can be used for cancellation).
        /// </summary>
        public List<CancellationTokenSource> runningTasks;

        public MpBackupProcess(MpMessageQueue messageQueue)
        {
            this.runningTasks = new List<CancellationTokenSource>();
            this.config = MpConfigManger.getConfig();
            this.messageQueue = messageQueue;
            if (this.config.backupSchedule.nextBackup == null)
            {
                setNextBackupTime();
            }
        }

        public void monitor()
        {
            log.Info("Waiting for the GUI to initialize.");
            while (!this.isGUIReady)
            {
                Thread.Sleep(100);
            }
            log.Info("Backup monitoring started.");
            displayGUIMessage("Backup monitoring started.");
            CancellationTokenSource initToken = new CancellationTokenSource(60000);
            this.runningTasks.Add(initToken);
            this.googleBackup = new GoogleBackupProvider(this, initToken.Token);
            while (!this.stopMonitoring)
            {
                if (DateTime.Compare(this.config.backupSchedule.nextBackup, DateTime.Now) <= 0)
                {
                    if (this.backupTask == null || this.backupTask.IsCompleted)
                    {
                        this.backupTask = performBackup();
                    }
                    else
                    {
                        log.Info("Skipping backup because one is running already.");
                    }
                }
                Thread.Sleep(100);
            }
            log.Info("Backup monitoring stopped.");
            
        }

        public async Task performBackup()
        {
            this.config.backupSchedule.lastBackup = DateTime.Now;
            this.backupRunning = true;
            List<string> filesToUpload = await compareFiles(this.config.backupDirectory.fullPath);
            if (filesToUpload.Count != 0)
            {
                await googleBackup.uploadFiles(filesToUpload);
            }
            setNextBackupTime();
            this.backupRunning = false;
        }

        /// <summary>
        /// Compare offline files with ones stored online and return a list of files that need to be uploaded.
        /// </summary>
        /// <param name="fullPath"></param>
        private async Task<List<string>> compareFiles(string fullPath)
        {
            List<string> onlineFiles = new List<string>();
            onlineFiles = await this.googleBackup.getUploadedFileNames();
            DirectoryInfo dir = new DirectoryInfo(fullPath);
            List<string> offlineFiles = dir.GetFiles().Select(f => f.Name).ToList();
            List<string> filesToUpload = offlineFiles.Except(onlineFiles).ToList();
            log.Info("Found [" + filesToUpload.Count + "] files to upload.");
            return filesToUpload;
        }

        private void setNextBackupTime()
        {
            this.config.backupSchedule.nextBackup = CrontabSchedule.Parse(this.config.backupSchedule.ncron).GetNextOccurrence(DateTime.Now);
            log.Info("Next backup to occur at: " + this.config.backupSchedule.nextBackup.ToString());
            MpConfigManger configManager = new MpConfigManger(this.config);
            configManager.saveConfig();
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
    }
}
