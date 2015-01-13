using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace mpBackup
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            MpBackupService backupService = new MpBackupService(Environment.UserInteractive);
            ServicesToRun = new ServiceBase[] 
            { 
                backupService
            };
            if (!Environment.UserInteractive)
            {
                // Startup as service.
                ServiceBase.Run(ServicesToRun);
            }
            else
            {
                // Startup as an application.
                backupService.serviceStart();
                Console.WriteLine("Press any key to exit the application.");
                Console.ReadKey();
            }
        }
    }
}