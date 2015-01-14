using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace mpBackup
{
    public partial class MpBackupService : ServiceBase
    {
        private MpConfigManger config;
        private MpBackupProcess backupProcess;
        private Thread backupProcessThread;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MpBackupService(bool isInteractive)
        {
            log.Info("Starting the service, UserInteractive=" + isInteractive.ToString());
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
#if DEBUG
            // This piece of code will only get executed in the Debug configuration. It stops the service from doing anything until a VS debugger is attached.
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(1000);
            }
#endif
            serviceStart();
        }

        protected override void OnStop()
        {
            log.Info("Stopping the service.");
            this.backupProcess.stopMonitoring = true;
        }

        public void serviceStart()
        {
            try
            {
                this.config = new MpConfigManger();
                this.backupProcess = new MpBackupProcess(this.config.config);
                this.backupProcessThread = new Thread(backupProcess.monitor);
                this.backupProcessThread.Start();
            }
            catch (Exception e)
            {
                log.Error("An error was caught: ", e);
            }

        }
    }
}