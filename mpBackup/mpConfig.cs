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

        [ConfigurationProperty("googleDriveSettings", IsRequired = true)]
        public GoogleDriveSettings googleDriveSettings
        {
            get { return (GoogleDriveSettings)this["googleDriveSettings"]; }
            set { this["googleDriveSettings"] = value; }
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

    public class GoogleDriveSettings : ConfigurationElement
    {
        [ConfigurationProperty("backupFolderId", DefaultValue = "", IsRequired = false)]
        public string backupFolderId
        {
            get { return (string)this["backupFolderId"]; }
            set { this["backupFolderId"] = value; }
        }

        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
