using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCrontab;

namespace mpBackup
{
    /// <summary>
    /// Used to perform backup operations such as scanning directories, preparing files and using backup handlers to store them in the cloud.
    /// </summary>
    public class MpBackupProcess
    {
        public bool stopMonitoring = false;

        private MpConfig config;
        private GoogleBackupProvider googleBackup;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MpBackupProcess(MpConfig config)
        {
            this.config = config;
            if (this.config.backupSchedule.nextBackup == null)
            {
                setNextBackupTime();
            }
            this.googleBackup = new GoogleBackupProvider(this.config);
        }

        public void monitor()
        {
            log.Info("Backup monitoring started.");
            while (!this.stopMonitoring)
            {
                if (DateTime.Compare(this.config.backupSchedule.nextBackup, DateTime.Now) <= 0)
                {
                    performBackup();
                }
                Thread.Sleep(60000);
            }
            
        }

        public async void performBackup()
        {
            List<string> filesToUpload = await compareFiles(this.config.backupDirectory.fullPath);
            if (filesToUpload.Count != 0)
            {
                await googleBackup.uploadFiles(filesToUpload);
            }
            this.config.backupSchedule.lastBackup = DateTime.Now;
            setNextBackupTime();
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
    }
}
