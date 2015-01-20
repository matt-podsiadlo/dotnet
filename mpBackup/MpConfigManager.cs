using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpBackup
{
    public class MpConfigManger
    {
        public MpConfig config;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Instantiate the class and read default configuration info from App.config
        /// </summary>
        public MpConfigManger()
        {
            this.config = (MpConfig)System.Configuration.ConfigurationManager.GetSection("mpBackup");

        } 
        
        /// <summary>
        /// Instantiate the config manager without reading default settings from app.config.
        /// </summary>
        /// <param name="config"></param>
        public MpConfigManger(MpConfig config)
        {
            this.config = config;
        }

        /// <summary>
        /// Persist the current config state into App.config.
        /// </summary>
        /// <param name="newConfig"></param>
        public void saveConfig()
        {
            log.Info("Saving updated config values.");
            Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            MpConfig currentMpBackupConfig = (MpConfig)appConfig.Sections["mpBackup"];
            // Need to explicitly assign values for each property:
            currentMpBackupConfig.googleDriveSettings.backupFolderId = this.config.googleDriveSettings.backupFolderId;
            currentMpBackupConfig.backupDirectory = this.config.backupDirectory;
            currentMpBackupConfig.backupSchedule = this.config.backupSchedule;

            appConfig.Save(ConfigurationSaveMode.Modified);
        }

        public static MpConfig getConfig()
        {
            return (MpConfig)System.Configuration.ConfigurationManager.GetSection("mpBackup");
        }

    }
}
