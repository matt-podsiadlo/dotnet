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

        public MpBackupProcess(MpConfig config)
        {
            this.config = config;
            this.googleBackup = new GoogleBackupProvider();
            compareFiles(this.config.backupDirectory.fullPath);
        }

        private void backupDirectory(string fullPath)
        {
            DirectoryInfo dir = new DirectoryInfo(fullPath);
            List<FileInfo> files = dir.GetFiles().ToList();
        }

        private async void compareFiles(string fullPath)
        {
            List<string> onlineFiles = new List<string>();
            onlineFiles = await this.googleBackup.getUploadedFileNames();
            DirectoryInfo dir = new DirectoryInfo(fullPath);
            List<FileInfo> offlineFiles = new List<FileInfo>();
            offlineFiles = dir.GetFiles().ToList();
        }
    }
}
