using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpBackup
{
    /// <summary>
    /// Used to perform backup operations such as scanning directories, preparing files and using backup handlers to store them in the cloud.
    /// </summary>
    public class MpBackupProcess
    {
        private MpConfig config;

        private GoogleBackupProvider googleBackup;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MpBackupProcess(MpConfig config)
        {
            this.config = config;
            this.googleBackup = new GoogleBackupProvider(this.config);
            performBackup();
        }

        public async void performBackup()
        {
            List<string> filesToUpload = await compareFiles(this.config.backupDirectory.fullPath);
            if (filesToUpload.Count != 0)
            {
                await googleBackup.uploadFiles(filesToUpload);
            }
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
    }
}
