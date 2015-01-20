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

namespace mpBackup.mpGUI
{
    public partial class AuthenticationForm : Form
    {
        public string authUrl;

        public AuthenticationForm(string authUrl)
        {
            this.authUrl = authUrl;
            this.Load += new EventHandler(load);
            InitializeComponent();
            
        }

        private void load(object sender, EventArgs e)
        {
            this.MinimumSize = new Size(500, 500);
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            //webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);
            webBrowser.Navigate(this.authUrl);
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.Size = webBrowser.Size;
        }
    }
}
