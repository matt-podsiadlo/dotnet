using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpBackup
{
    public class MpConfigReader
    {
        /// <summary>
        /// Directory where backups are stored.
        /// </summary>
        public string BACKUP_DIR { get; set; }

        public MpConfig config;

        /// <summary>
        /// Instantiate the class and read default configuration info from App.config
        /// </summary>
        public MpConfigReader()
        {
            this.config = (MpConfig)System.Configuration.ConfigurationManager.GetSection("mpBackup/settings");
        }

    }
}
