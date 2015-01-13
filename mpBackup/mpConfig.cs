using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpBackup
{
    public class MpConfig : ConfigurationSection
    {
        [ConfigurationProperty("backupDirectory", IsRequired = true)]
        public BackupDirectory backupDirectory 
        {
            get { return (BackupDirectory)this["backupDirectory"]; }
            set { this["backupDirectory"] = value; }
        }
    }

    public class BackupDirectory : ConfigurationElement
    {
        [ConfigurationProperty("fullPath", DefaultValue = "", IsRequired = true)]
        public string fullPath 
        {
            get { return (string)this["fullPath"]; }
            set { this["fullPath"] = value; }
        }
    }
}
