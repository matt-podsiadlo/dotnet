using mpBackup.MpGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace mpBackup
{
    public class MpBackup
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static string mpBackupSIGUID = "c5ec4ba2-156f-43f5-ac6a-3f6f0a8d171c";

        private CustomApplicationContext applicationContext;

        static void Main()
        {
            string mutexName = @"Global\" + mpBackupSIGUID + WindowsIdentity.GetCurrent().User.ToString();
            log.Debug("Starting the application with [" + mutexName + "] mutexs.");
            bool mutexIsNew;
            Mutex mutex = new Mutex(false, @"Global\" + mpBackupSIGUID + WindowsIdentity.GetCurrent().User.ToString(), out mutexIsNew);
            if (!mutexIsNew)
            {
                MessageBox.Show("An instance of mpBackup is already running.");
                log.Info("Attempt to start another instance of mpBackup detected. Opening the configuration window instead.");
                return;
            }
            else
            {
                MpBackup mpBackup = new MpBackup();
                try
                {
                    // Create the tray icon
                    Thread trayIconThread = new Thread(() => mpBackup.createTrayIcon(mutex));
                    trayIconThread.SetApartmentState(ApartmentState.STA);
                    trayIconThread.Start();
                }
                catch (Exception e)
                {
                    log.Error("An error was caught: ", e);
                }
            }
            GC.KeepAlive(mutex);
        }

        public void displayConfig()
        {
            this.applicationContext.openConfigurationForm(null, null);
        }

        private void createTrayIcon(Mutex mutex)
        {
            this.applicationContext = new CustomApplicationContext(mutex);
            try
            {
                Application.EnableVisualStyles();
                Application.Run(this.applicationContext);
            }
            catch (Exception e)
            {
                log.Error("An error was caught: ", e);
            }
        }
    }
}