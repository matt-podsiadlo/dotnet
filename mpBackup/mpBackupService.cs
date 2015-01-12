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

namespace mpBackup
{
    public partial class mpBackupService : ServiceBase
    {
        public mpConfig config;
        public mpLog log;

        public mpBackupService()
        {
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
            this.log = new mpLog(0);
            this.config = new mpConfig();
        }

        protected override void OnStop()
        {
            this.log.logMessage("Service is stopping.", 0);
        }
    }
}
