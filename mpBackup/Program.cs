using mpBackup.MpGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace mpBackup
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            MpMessageQueue messageQueue = new MpMessageQueue();
            // Start the backup process
            MpBackupProcess backupProcess;
            Thread backupProcessThread;
            try
            {
                backupProcess = new MpBackupProcess(messageQueue);
                backupProcessThread = new Thread(backupProcess.monitor);
                backupProcessThread.Start();

                // Create the tray icon
                Thread trayIconThread = new Thread(() => createTrayIcon(backupProcess, messageQueue));
                trayIconThread.SetApartmentState(ApartmentState.STA);
                trayIconThread.Start();
            }
            catch (Exception e)
            {
                log.Error("An error was caught: ", e);
            }
        }

        private static void createTrayIcon(MpBackupProcess backupProcess, MpMessageQueue messageQueue)
        {
            ApplicationContext applicationContext = new CustomApplicationContext(backupProcess, messageQueue);
            Application.Run(applicationContext);
        }
    }
}