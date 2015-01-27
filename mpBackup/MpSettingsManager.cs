using NCrontab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpBackup
{
    public class MpSettingsManager
    {
        public MpSettings settings;
        /// <summary>
        /// Are the current in-memory settings valid?
        /// </summary>
        public bool isValid { get; private set; }

        #region Events
        /// <summary>
        /// Settings were saved.
        /// </summary>
        /// <param name="interactive">Whether the settings were saved interactively (i.e. after being changed by a user)</param>
        public delegate void SettingsSavedHandler(bool interactive, bool folderChanged);
        /// <summary>
        /// Fires when settings are persisted at runtime.
        /// </summary>
        public event SettingsSavedHandler SettingsSaved;
        public delegate void SettingsInvalidatedHandler();
        /// <summary>
        /// Fires when the current settings are invalidated by a running process
        /// </summary>
        public event SettingsInvalidatedHandler SettingsInvalidated;
        #endregion

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Keep track of whether the backup folder was changed whilst saving new settings, and indicate so in the SettingsSaved event
        /// </summary>
        private bool backupFolderChanged;
        private string oldFolder;

        public MpSettingsManager()
        {
            this.settings = new MpSettings();
            validate();
        }

        /// <summary>
        /// Persist the current settings state into disk.
        /// </summary>
        public bool saveSettings(bool interactive)
        {
            if (validate())
            {
                lock (this.settings)
                {
                    this.settings.Save();
                    if (SettingsSaved != null) SettingsSaved(interactive, this.backupFolderChanged);
                    this.oldFolder = this.settings.backupFolderPath;
                    this.backupFolderChanged = false;
                }
            }
            return this.isValid;
        }

        private bool validate()
        {
            bool valid = true;
            if (this.settings.backupFolderPath == String.Empty || !Directory.Exists(this.settings.backupFolderPath))
            {
                valid = false;
            }
            else
            {
                this.oldFolder = this.settings.backupFolderPath;
            }
            if (!this.settings.continuousMonitoring)
            {
                try
                {
                    CrontabSchedule.Parse(this.settings.backupScheduleCron);
                }
                catch
                {
                    log.Error("Specified crontab expression is not valid.");
                    valid = false;
                }
            }
            if (!valid)
            {
                log.Info("The specified configuration is not valid, or has not yet been initialized.");
            }
            this.isValid = valid;
            return valid;
        }

        internal void setBackupFolderId(string folderId)
        {
            lock (this.settings)
            {
                this.settings.onlineFolderId = folderId;
            }
        }

        internal void setLastBackupTime(DateTime dateTime)
        {
            lock (this.settings)
            {
                this.settings.lastBackupTime = dateTime;
            }
        }

        internal void setNextBackupTime(DateTime dateTime)
        {
            lock (this.settings)
            {
                this.settings.nextBackupTime = dateTime;
            }
        }

        internal void setBackupFolderPath(string path)
        {
            lock (this.settings)
            {
                if (Directory.Exists(path))
                {
                    this.settings.backupFolderPath = path;
                    if (this.oldFolder != path) this.backupFolderChanged = true;
                }
            }
        }

        internal void setBackupScheduleCron(string cron)
        {
            lock (this.settings)
            {
                this.settings.backupScheduleCron = cron;
            }
        }

        /// <summary>
        /// Call this to invalidate settings currently stored in memory. An invalidation event will be fired by the settings manager.
        /// </summary>
        internal void invalidate()
        {
            this.isValid = false;
            if (SettingsInvalidated != null) SettingsInvalidated();
        }
    }
}
