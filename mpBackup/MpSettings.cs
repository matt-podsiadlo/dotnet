using System;
using System.Configuration;

namespace mpBackup
{
    public class MpSettings : ApplicationSettingsBase
    {
        [UserScopedSetting]
        [DefaultSettingValue("")]
        [SettingsManageability(SettingsManageability.Roaming)]
        public string backupFolderPath 
        { 
            get
            {
                return (string)this["backupFolderPath"];
            }
            set
            {
                this["backupFolderPath"] = (string)value;
            }
        }
        /// <summary>
        /// ID of the mpBackup folder on Google Drive
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("")]
        [SettingsManageability(SettingsManageability.Roaming)]
        public string onlineFolderId
        {
            get
            {
                return (string)this["onlineFolderId"];
            }
            set
            {
                this["onlineFolderId"] = (string)value;
            }
        }
        /// <summary>
        /// Backup schedule in a cron format
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("")]
        [SettingsManageability(SettingsManageability.Roaming)]
        public string backupScheduleCron
        {
            get
            {
                return (string)this["backupScheduleCron"];
            }
            set
            {
                this["backupScheduleCron"] = (string)value;
            }
        }
        [UserScopedSetting]
        [DefaultSettingValue("")]
        [SettingsManageability(SettingsManageability.Roaming)]
        public DateTime lastBackupTime
        {
            get
            {
                return (DateTime)this["lastBackupTime"];
            }
            set
            {
                this["lastBackupTime"] = value;
            }
        }
        [UserScopedSetting()]
        [SettingsSerializeAs(System.Configuration.SettingsSerializeAs.Xml)]
        [DefaultSettingValue("")]
        [SettingsManageability(SettingsManageability.Roaming)]
        public DateTime nextBackupTime
        {
            get
            {
                return (DateTime)this["nextBackupTime"];
            }
            set
            {
                this["nextBackupTime"] = value;
            }
        }
        /// <summary>
        /// If set to true, the backup process continuously monitors for file system changes, rather than looking at <see cref="MpSettings.backupScheduleCron"/>
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValueAttribute("true")]
        [SettingsManageability(SettingsManageability.Roaming)]
        public bool continuousMonitoring
        {
            get
            {
                return (bool)this["continuousMonitoring"];
            }
            set
            {
                this["continuousMonitoring"] = value;
            }
        }
        /// <summary>
        /// Start the application with Windows?
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValueAttribute("true")]
        [SettingsManageability(SettingsManageability.Roaming)]
        public bool autostart
        {
            get
            {
                return (bool)this["autostart"];
            }
            set
            {
                this["autostart"] = value;
            }
        }
    }
}
