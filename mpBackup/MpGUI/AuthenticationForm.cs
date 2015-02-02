using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mpBackup.MpGUI
{
    public partial class AuthenticationForm : Form
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string authUrl;
        /// <summary>
        /// Backup provider requiring authentication.
        /// </summary>
        private MpBackupProvider backupProvider;

        /// <summary>
        /// Create an authentication form.
        /// </summary>
        /// <param name="authUrl">URL to navigate to on load.</param>
        /// <param name="backupProvider">Backup provider that requires authentication.</param>
        public AuthenticationForm(string authUrl, MpBackupProvider backupProvider)
        {
            this.authUrl = authUrl;
            this.backupProvider = backupProvider;
            // Handle the form being closed before successful authentication:
            this.Disposed += AuthenticationForm_Disposed;
            // Close the form on successful authentication:
            backupProvider.AuthenticationSuccessful += backupProvider_AuthenticationSuccessful;
            // Navigate to the authentication URL on load:
            this.Load += new EventHandler(load);
            InitializeComponent();
        }

        private void load(object sender, EventArgs e)
        {
            this.MinimumSize = new Size(500, 500);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            webBrowser.Navigate(this.authUrl);
        }

        void backupProvider_AuthenticationSuccessful(object sender, EventArgs e)
        {
            this.Invoke(new MethodInvoker(this.Close));
        }

        void AuthenticationForm_Disposed(object sender, EventArgs e)
        {
            if (this.backupProvider.cancelAuthentication != null)
            {
                log.Debug("User closed the authentication form prematurely, informing the backup provider.");
                this.backupProvider.cancelAuthentication.Cancel();
            }
        }
    }
}
