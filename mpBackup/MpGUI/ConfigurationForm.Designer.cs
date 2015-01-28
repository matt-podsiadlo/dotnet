namespace mpBackup.MpGUI
{
    partial class ConfigurationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationForm));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.errBackupFolderPath = new System.Windows.Forms.Label();
            this.backupFolderBrowseButton = new System.Windows.Forms.Button();
            this.backupFolderLabel = new System.Windows.Forms.Label();
            this.continuousMonitoringcheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.errSchedule = new System.Windows.Forms.Label();
            this.scheduleTextBox = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.errorLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.configCancelButton = new System.Windows.Forms.Button();
            this.configSaveButton = new System.Windows.Forms.Button();
            this.backupFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.panel4 = new System.Windows.Forms.Panel();
            this.autoStartCheckBox = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.groupBox1);
            this.flowLayoutPanel1.Controls.Add(this.groupBox2);
            this.flowLayoutPanel1.Controls.Add(this.panel4);
            this.flowLayoutPanel1.Controls.Add(this.panel2);
            this.flowLayoutPanel1.Controls.Add(this.panel1);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.flowLayoutPanel1.MinimumSize = new System.Drawing.Size(488, 238);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(497, 296);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.errBackupFolderPath);
            this.groupBox1.Controls.Add(this.backupFolderBrowseButton);
            this.groupBox1.Controls.Add(this.backupFolderLabel);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.MinimumSize = new System.Drawing.Size(491, 74);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(491, 74);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Backup Folder";
            // 
            // errBackupFolderPath
            // 
            this.errBackupFolderPath.AutoSize = true;
            this.errBackupFolderPath.ForeColor = System.Drawing.Color.Red;
            this.errBackupFolderPath.Location = new System.Drawing.Point(6, 58);
            this.errBackupFolderPath.Name = "errBackupFolderPath";
            this.errBackupFolderPath.Size = new System.Drawing.Size(171, 13);
            this.errBackupFolderPath.TabIndex = 2;
            this.errBackupFolderPath.Text = "The specified folder does not exist.";
            this.errBackupFolderPath.Visible = false;
            // 
            // backupFolderBrowseButton
            // 
            this.backupFolderBrowseButton.Location = new System.Drawing.Point(6, 32);
            this.backupFolderBrowseButton.Name = "backupFolderBrowseButton";
            this.backupFolderBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.backupFolderBrowseButton.TabIndex = 1;
            this.backupFolderBrowseButton.Text = "Select";
            this.backupFolderBrowseButton.UseVisualStyleBackColor = true;
            // 
            // backupFolderLabel
            // 
            this.backupFolderLabel.AutoSize = true;
            this.backupFolderLabel.Location = new System.Drawing.Point(6, 16);
            this.backupFolderLabel.Name = "backupFolderLabel";
            this.backupFolderLabel.Size = new System.Drawing.Size(33, 13);
            this.backupFolderLabel.TabIndex = 0;
            this.backupFolderLabel.Text = "None";
            // 
            // continuousMonitoringcheckBox
            // 
            this.continuousMonitoringcheckBox.AutoSize = true;
            this.continuousMonitoringcheckBox.Location = new System.Drawing.Point(9, 19);
            this.continuousMonitoringcheckBox.Name = "continuousMonitoringcheckBox";
            this.continuousMonitoringcheckBox.Size = new System.Drawing.Size(131, 17);
            this.continuousMonitoringcheckBox.TabIndex = 0;
            this.continuousMonitoringcheckBox.Text = "Continuous Monitoring";
            this.continuousMonitoringcheckBox.UseVisualStyleBackColor = true;
            this.continuousMonitoringcheckBox.CheckedChanged += new System.EventHandler(this.continuousMonitoringcheckBox_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.continuousMonitoringcheckBox);
            this.groupBox2.Controls.Add(this.errSchedule);
            this.groupBox2.Controls.Add(this.scheduleTextBox);
            this.groupBox2.Location = new System.Drawing.Point(3, 83);
            this.groupBox2.MinimumSize = new System.Drawing.Size(491, 74);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(491, 93);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Backup Schedule (cron format)";
            // 
            // errSchedule
            // 
            this.errSchedule.AutoSize = true;
            this.errSchedule.ForeColor = System.Drawing.Color.Red;
            this.errSchedule.Location = new System.Drawing.Point(8, 69);
            this.errSchedule.Name = "errSchedule";
            this.errSchedule.Size = new System.Drawing.Size(205, 13);
            this.errSchedule.TabIndex = 1;
            this.errSchedule.Text = "The specified schedule format is not valid.";
            this.errSchedule.Visible = false;
            // 
            // scheduleTextBox
            // 
            this.scheduleTextBox.Location = new System.Drawing.Point(6, 42);
            this.scheduleTextBox.Name = "scheduleTextBox";
            this.scheduleTextBox.Size = new System.Drawing.Size(478, 20);
            this.scheduleTextBox.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.errorLabel);
            this.panel2.Location = new System.Drawing.Point(3, 217);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(491, 39);
            this.panel2.TabIndex = 3;
            // 
            // errorLabel
            // 
            this.errorLabel.AutoSize = true;
            this.errorLabel.ForeColor = System.Drawing.Color.Red;
            this.errorLabel.Location = new System.Drawing.Point(136, 13);
            this.errorLabel.Name = "errorLabel";
            this.errorLabel.Size = new System.Drawing.Size(229, 13);
            this.errorLabel.TabIndex = 0;
            this.errorLabel.Text = "Please provide valid settings before continuing.";
            this.errorLabel.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.configCancelButton);
            this.panel1.Controls.Add(this.configSaveButton);
            this.panel1.Location = new System.Drawing.Point(3, 262);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(491, 23);
            this.panel1.TabIndex = 2;
            // 
            // configCancelButton
            // 
            this.configCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.configCancelButton.Location = new System.Drawing.Point(413, 0);
            this.configCancelButton.Name = "configCancelButton";
            this.configCancelButton.Size = new System.Drawing.Size(75, 23);
            this.configCancelButton.TabIndex = 1;
            this.configCancelButton.Text = "Cancel";
            this.configCancelButton.UseVisualStyleBackColor = true;
            // 
            // configSaveButton
            // 
            this.configSaveButton.Location = new System.Drawing.Point(332, 0);
            this.configSaveButton.Name = "configSaveButton";
            this.configSaveButton.Size = new System.Drawing.Size(75, 23);
            this.configSaveButton.TabIndex = 0;
            this.configSaveButton.Text = "Save";
            this.configSaveButton.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.autoStartCheckBox);
            this.panel4.Location = new System.Drawing.Point(3, 182);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(491, 29);
            this.panel4.TabIndex = 5;
            // 
            // autoStartCheckBox
            // 
            this.autoStartCheckBox.AutoSize = true;
            this.autoStartCheckBox.Location = new System.Drawing.Point(9, 4);
            this.autoStartCheckBox.Name = "autoStartCheckBox";
            this.autoStartCheckBox.Size = new System.Drawing.Size(194, 17);
            this.autoStartCheckBox.TabIndex = 0;
            this.autoStartCheckBox.Text = "Auto start mpBackup with Windows";
            this.autoStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // ConfigurationForm
            // 
            this.AcceptButton = this.configSaveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.configCancelButton;
            this.ClientSize = new System.Drawing.Size(512, 310);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConfigurationForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "mpBackup Configuration";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button backupFolderBrowseButton;
        private System.Windows.Forms.Label backupFolderLabel;
        private System.Windows.Forms.FolderBrowserDialog backupFolderBrowserDialog;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox scheduleTextBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button configCancelButton;
        private System.Windows.Forms.Button configSaveButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label errorLabel;
        private System.Windows.Forms.Label errBackupFolderPath;
        private System.Windows.Forms.Label errSchedule;
        private System.Windows.Forms.CheckBox continuousMonitoringcheckBox;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.CheckBox autoStartCheckBox;
    }
}