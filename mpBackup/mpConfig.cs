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

        [ConfigurationProperty("backupSchedule", IsRequired = true)]
        public BackupSchedule backupSchedule
        {
            get { return (BackupSchedule)this["backupSchedule"]; }
            set { this["backupSchedule"] = value; }
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

    public class BackupSchedule : ConfigurationElement
    {
        [ConfigurationProperty("ncron", DefaultValue = "", IsRequired = false)]
        public string ncron
        {
            get { return (string)this["ncron"]; }
            set { this["ncron"] = value; }
        }

        [ConfigurationProperty("lastBackup", DefaultValue = "", IsRequired = false)]
        public DateTime lastBackup
        {
            get { return (DateTime)this["lastBackup"]; }
            set { this["lastBackup"] = value; }
        }

        [ConfigurationProperty("nextBackup", DefaultValue = "", IsRequired = false)]
        public DateTime nextBackup
        {
            get { return (DateTime)this["nextBackup"]; }
            set { this["nextBackup"] = value; }
        }

        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
