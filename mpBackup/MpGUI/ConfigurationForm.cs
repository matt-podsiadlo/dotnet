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
        public ConfigurationForm()
        {
            InitializeComponent();
            backupFolderBrowseButton.Click += backupFolderBrowseButton_Click;
        }

        void backupFolderBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult folderChoice = backupFolderBrowserDialog.ShowDialog();
            if (folderChoice == DialogResult.OK)
            {
                backupFolderLabel.Text = backupFolderBrowserDialog.SelectedPath;
            }
        }
    }
}
