using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mpBackup.MpGUI
{
    /// <summary>
    /// Initializes the application and creates the tray icon. All subsequent threads spawn from here.
    /// </summary>
    public class CustomApplicationContext : ApplicationContext
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Mutex mutex;

        public Container components;
        public NotifyIcon mpTray;

        MpMessageQueue messageQueue;
        MpSettingsManager settingsManager;
        MpBackupProcess backupProcess;
        Thread backupProcessThread;

        /// <summary>
        /// Monitors the message queue and displays messages.
        /// </summary>
        BackgroundWorker messageWorker;

        ConfigurationForm configForm;
        Thread configFormThread;

        public CustomApplicationContext(Mutex mutex)
        {
            this.mutex = mutex;
            this.messageQueue = new MpMessageQueue();
            this.settingsManager = new MpSettingsManager();
            // Create the tray icon
            initializeContext();
            // Create a worker thread for displaying messages
            messageWorker = new BackgroundWorker();
            //messageWorker.WorkerReportsProgress = true;
            messageWorker.WorkerSupportsCancellation = true;
            messageWorker.DoWork += messageWorker_DoWork;
            messageWorker.RunWorkerAsync();
            // Check the initial user configuration
            BackgroundWorker configurationCheck = new BackgroundWorker();
            configurationCheck.DoWork += (obj, e) => configurationCheck_DoWork(this.settingsManager, this.messageQueue);
            configurationCheck.RunWorkerCompleted += configurationCheck_RunWorkerCompleted;
            configurationCheck.RunWorkerAsync();
        }

        /// <summary>
        /// Start the backup process and any remaining worker threads if the initial user configuration is valid.
        /// </summary>
        /// <param name="sender">The config check backround worker</param>
        /// <param name="e"></param>
        void configurationCheck_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!this.settingsManager.isValid)
            {
                exit(null, null);
            }
            else
            {
                // Bind to relevant events
                this.settingsManager.SettingsInvalidated += settingsManager_SettingsInvalidated;
                // Start the backup process
                try
                {
                    this.backupProcess = new MpBackupProcess(this.messageQueue, this.settingsManager);
                    this.backupProcess.InitializationFailed += backupProcess_InitializationFailed;
                    this.backupProcess.UserAuthenticationRequired += backupProcess_UserAuthenticationRequired;
                    // Starting the backup process in a separate thread.
                    this.backupProcessThread = new Thread(() => startBackupProcess(this.backupProcess));
                    backupProcessThread.Start();
                }
                catch (Exception ex)
                {
                    log.Error("An error was caught: ", ex);
                }
            }
        }

        void backupProcess_InitializationFailed(object sender, EventArgs e)
        {
            log.Error("Backup process initialization failed.");
            exit(null, null);
        }

        private void startBackupProcess(MpBackupProcess backupProcess)
        {
            this.backupProcess.initialize();
            if (this.backupProcess.currentState == MpBackupProcess.BackupProcessState.FAILED)
            {
                // Here we can handle what happens when the backup process fails to initialize, at the moment exiting the application is fine.
            }
            else if (this.backupProcess.currentState == MpBackupProcess.BackupProcessState.READY)
            {
                this.backupProcess.monitor();
            }
        }

        private void backupProcess_UserAuthenticationRequired(object sender, string authenticationUrl)
        {
            Thread authFormThread = new Thread(() => showAuthenticationForm(authenticationUrl, (MpBackupProvider)sender));
            authFormThread.SetApartmentState(ApartmentState.STA);
            authFormThread.Start();
        }

        /// <summary>
        /// Open a dialog for the user to provide valid settings if they somehow became invalidated at runtime.
        /// </summary>
        void settingsManager_SettingsInvalidated()
        {
            ConfigurationForm configForm = new ConfigurationForm(this.settingsManager, this.messageQueue);
            Thread configFormThread = new Thread(() => showConfigForm(configForm));
            configFormThread.SetApartmentState(ApartmentState.STA);
            configFormThread.Start();
            while (!settingsManager.isValid)
            {
                if (configForm.IsDisposed)
                {
                    // User closed the config form, gotta exit!
                    log.Error("User canceled out of the config dialog, exiting.");
                    break;
                }
                Thread.Sleep(100);
            }
            if (!this.settingsManager.isValid) exit(null, null);
        }

        /// <summary>
        /// Check the current user configuration. Display a form if the configuration is invalid.
        /// </summary>
        /// <param name="sender">The CustomApplicationContext</param>
        /// <param name="e"></param>
        void configurationCheck_DoWork(MpSettingsManager settingsManager, MpMessageQueue messageQueue)
        {
            if (!settingsManager.isValid)
            {
                log.Info("Waiting for the user to specify a valid configuration.");
                ConfigurationForm configForm = new ConfigurationForm(settingsManager, messageQueue);
                Thread configFormThread = new Thread(() => showConfigForm(configForm));
                configFormThread.SetApartmentState(ApartmentState.STA);
                configFormThread.Start();
                while (!settingsManager.isValid)
                {
                    if (configForm.IsDisposed)
                    {
                        // User closed the config form, gotta exit!
                        log.Error("Initial configuration failed. Exiting.");
                        break;
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private void initializeContext()
        {
            // Create the tray icon
            components = new Container();
            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            Stream iconStream = myAssembly.GetManifestResourceStream("mpBackup.Images.mp-icon-blue.ico");
            ContextMenuStrip menu = new ContextMenuStrip();
            mpTray = new NotifyIcon(components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = new Icon(iconStream),
                Text = "mpBackup",
                Visible = true
            };
            mpTray.ContextMenuStrip.Items.Add("Settings", null, openConfigurationForm);
            mpTray.ContextMenuStrip.Items.Add("Exit", null, exit);
        }

        public void openConfigurationForm(object sender, EventArgs e)
        {
            if (this.configFormThread == null || !this.configFormThread.IsAlive)
            {
                this.configForm = new ConfigurationForm(this.settingsManager, this.messageQueue);
                this.configFormThread = new Thread(() => showConfigForm(this.configForm));
                this.configFormThread.SetApartmentState(ApartmentState.STA);
                this.configFormThread.Start();
            }
            else
            {
                this.configForm.BeginInvoke(new MethodInvoker(this.configForm.BringToFront));
            }
        }

        private void showConfigForm(ConfigurationForm form = null)
        {
            Application.Run(form);
        }

        private void showAuthenticationForm(string authenticationUrl, MpBackupProvider backupProvider)
        {
            AuthenticationForm authForm = new AuthenticationForm(authenticationUrl, backupProvider);
            Application.Run(authForm);
        }

        /// <summary>
        /// Exit the application. Dispose of all reasources here.
        /// </summary>
        public void exit(object sender, EventArgs e)
        {
            log.Info("Exitting the application.");
            // Stop the backup process.
            if (backupProcess != null)
            {
                log.Info("Waiting for the backup process to finish.");
                this.backupProcess.stop();
                while (this.backupProcess.currentState != MpBackupProcess.BackupProcessState.STOPPED)
                {
                    Thread.Sleep(50);
                }
            }
            // Destroy any existing forms
            if (this.configForm != null && !this.configForm.IsDisposed)
            {
                this.configForm.BeginInvoke(new MethodInvoker(this.configForm.Close));
                this.configForm.BeginInvoke(new MethodInvoker(this.configForm.Dispose));
            }
            // Stop the message queue
            if (this.messageWorker != null)
            {
                this.messageWorker.RunWorkerCompleted += messageWorker_RunWorkerCompleted;
                this.messageWorker.CancelAsync();
            }
        }

        void messageWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            log.Debug("Finished stopping all background workers.");
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) { components.Dispose(); }
        }

        protected override void ExitThreadCore()
        {
            if (this.mpTray != null)
            {
                this.mpTray.Visible = false; // should remove lingering tray icon!
            }
            base.ExitThreadCore();
        }

        private void messageWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            log.Info("Message queue monitoring started.");
            BackgroundWorker worker = sender as BackgroundWorker;
            while (!worker.CancellationPending)
            {
                foreach (MpMessage message in messageQueue.getMessages(MpMessage.DisplayAs.BALOON))
                {
                    mpTray.ShowBalloonTip(3000, "mpBackup", message.text, ToolTipIcon.Info);
                }
                Thread.Sleep(100);
            }
            e.Cancel = true;
            log.Info("Message queue monitoring stopped.");
        }
    }
}
