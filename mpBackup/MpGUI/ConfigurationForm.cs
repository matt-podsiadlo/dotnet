using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mpBackup.MpGUI
{
    public partial class ConfigurationForm : Form
    {
        MpSettingsManager settingsManager;
        MpMessageQueue messageQueue;

        public ConfigurationForm(MpSettingsManager settingsManager, MpMessageQueue messageQueue)
        {
            this.messageQueue = messageQueue;
            this.settingsManager = settingsManager;
            InitializeComponent();
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            backupFolderBrowseButton.Click += backupFolderBrowseButton_Click;
            configSaveButton.Click += configSaveButton_Click;
            configCancelButton.Click += configCancelButton_Click;
            this.Load += ConfigurationForm_Load;
        }

        void configCancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        void configSaveButton_Click(object sender, EventArgs e)
        {
            this.settingsManager.setBackupFolderPath(backupFolderLabel.Text);
            this.settingsManager.setBackupScheduleCron(scheduleTextBox.Text);
            this.settingsManager.settings.continuousMonitoring = continuousMonitoringcheckBox.Checked;
            if (!this.settingsManager.saveSettings(true))
            {
                errorLabel.Visible = true;
                return;
            }
            this.messageQueue.addMessageAsync(new MpMessage()
            {
                text = "Configuration values saved.",
                displayAs = MpMessage.DisplayAs.BALOON
            });
            errorLabel.Visible = false;
            this.Close();
            this.Dispose();
        }

        void ConfigurationForm_Load(object sender, EventArgs e)
        {
            errorLabel.Visible = !this.settingsManager.isValid;
            continuousMonitoringcheckBox.Checked = this.settingsManager.settings.continuousMonitoring;
            scheduleTextBox.Text = this.settingsManager.settings.backupScheduleCron;
            scheduleTextBox.Enabled = !continuousMonitoringcheckBox.Checked;
            backupFolderLabel.Text = this.settingsManager.settings.backupFolderPath;
            
        }

        void backupFolderBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult folderChoice = backupFolderBrowserDialog.ShowDialog();
            if (folderChoice == DialogResult.OK)
            {
                backupFolderLabel.Text = backupFolderBrowserDialog.SelectedPath;
            }
        }

        private void continuousMonitoringcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            scheduleTextBox.Enabled = !continuousMonitoringcheckBox.Checked;
        }
    }
}
