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
        public bool isMonitoring = false;
        public MpSettingsManager settingsManager;

        private GoogleBackupProvider googleBackup;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Task backupTask;

        public MpMessageQueue messageQueue;
        /// <summary>
        /// Collection of currently running tasks, with appropriate cancellation tokens.
        /// CancellationTokenSource is required for each tuple, while Task is not. 
        /// If Task is null, it means it's blocking the backup process Thread.
        /// </summary>
        public List<Tuple<Task, CancellationTokenSource>> runningTasks;

        public MpBackupProcess(MpMessageQueue messageQueue, MpSettingsManager settingsManager)
        {
            this.runningTasks = new List<Tuple<Task, CancellationTokenSource>>();
            this.settingsManager = settingsManager;
            this.messageQueue = messageQueue;
            if (this.settingsManager.settings.nextBackupTime == null)
            {
                setNextBackupTime();
            }
        }

        public async void monitor()
        {
            this.isMonitoring = true;
            log.Info("Backup monitoring started.");
            displayGUIMessage("Backup monitoring started.");
            Tuple<Task, CancellationTokenSource> initCancellation = new Tuple<Task, CancellationTokenSource>(null, new CancellationTokenSource(60000));
            this.runningTasks.Add(initCancellation);
            this.googleBackup = new GoogleBackupProvider(this, initCancellation.Item2.Token);
            this.runningTasks.Remove(initCancellation);
            //MpFileUpload test = new MpFileUpload()
            //{
            //    fileName = "KeePass-2.28-Setup.zip",
            //    fileExtension = "zip",
            //    fullPath = this.settingsManager.settings.backupFolderPath + "\\KeePass-2.28-Setup.zip"
            //};
            //CancellationTokenSource djskfhdskjf = new CancellationTokenSource();
            //Task testUpload = new Task(() => this.googleBackup.uploadFile(test, djskfhdskjf.Token));
            //Tuple<Task, CancellationTokenSource> testSrc = new Tuple<Task, CancellationTokenSource>(testUpload, djskfhdskjf);
            //testUpload.Start();
            //this.runningTasks.Add(testSrc);
            
            while (!this.stopMonitoring)
            {
                if (DateTime.Compare(this.settingsManager.settings.nextBackupTime, DateTime.Now) <= 0)
                {
                    CancellationTokenSource backupCancellationToken = new CancellationTokenSource();
                    this.runningTasks.Add(new Tuple<Task, CancellationTokenSource>(null, backupCancellationToken));
                    await performBackup(backupCancellationToken.Token);
                }
                Thread.Sleep(100);
            }
            log.Info("Waiting for all running tasks to finish cleanly.");
            while (this.stopMonitoring)
            {
                if (this.runningTasks.Count(t => t.Item1 != null && !t.Item1.IsCompleted) == 0)
                {
                    break;
                }
                Thread.Sleep(50);
            }
            this.isMonitoring = false;
            log.Info("Backup monitoring stopped.");
        }

        public async Task performBackup(CancellationToken cancellationToken)
        {
            this.settingsManager.setLastBackupTime(DateTime.Now);
            List<MpFileUpload> filesToUpload = await compareFiles(this.settingsManager.settings.backupFolderPath, cancellationToken);
            if (filesToUpload.Count != 0)
            {
                foreach (MpFileUpload file in filesToUpload)
                {
                    Task fileUpload = new Task(() => this.googleBackup.uploadFile(file, file.uploadCancellation.Token));
                    file.uploadTask = fileUpload;
                    fileUpload.Start();
                    this.runningTasks.Add(new Tuple<Task, CancellationTokenSource>(fileUpload, file.uploadCancellation));
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
            this.settingsManager.setNextBackupTime(CrontabSchedule.Parse(this.settingsManager.settings.backupScheduleCron).GetNextOccurrence(DateTime.Now));
            log.Info("Next backup to occur at: " + this.settingsManager.settings.nextBackupTime.ToString());
            this.settingsManager.saveSettings();
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
