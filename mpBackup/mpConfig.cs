using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpBackup
{
    class mpConfig
    {
        /// <summary>
        /// Directory where backups are stored.
        /// </summary>
        public string BACKUP_DIR { get; set; }

        /// <summary>
        /// Instantiate the class and read default configuration info from App.config
        /// </summary>
        public mpConfig()
        {
            BACKUP_DIR = ConfigurationManager.AppSettings["BackupDirectory"];
        }

    }
}
