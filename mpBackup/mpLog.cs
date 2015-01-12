using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpBackup
{
    /// <summary>
    /// Use this class to log messages.
    /// </summary>
    class mpLog
    {
        public int verbosity;
        public string cwd;
        public string fileName;
        public string fullPath;

        /// <summary>
        /// Instantiate the logger. Logs are kept in the current working directory of the application.
        /// </summary>
        /// <param name="verbosity">Incoming messages with verbosity greater than this value WILL NOT get logged.</param>
        public mpLog(int verbosity)
        {
            this.fileName = "mpBackupLog-" + DateTime.Now.ToString("dd_MMM_yyyy_HH-mm-ss") + ".txt";
            this.fullPath = Directory.GetCurrentDirectory() + "\\" + this.fileName;
            logMessage("Logging started.");
        }

        /// <summary>
        /// Log a message. If verbosity is not provided, it will get logged by default.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="verbosity"></param>
        public void logMessage(string message, int verbosity = 0)
        {
            if (!File.Exists(this.fullPath))
            {
                File.Create(this.fullPath).Close();
            }
            StreamWriter tw = new StreamWriter(this.fullPath, true);
            tw.WriteLine("[" + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss") + "]" + message);
            tw.Close();
        }

    }
}
