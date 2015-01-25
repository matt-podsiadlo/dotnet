using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mpBackup
{
    /// <summary>
    /// Represents a local file that needs to be uploaded to the cloud.
    /// </summary>
    public class MpFileUpload
    {
        /// <summary>
        /// "[name].[extension]"
        /// </summary>
        public string fileName { get; set; }
        /// <summary>
        /// Full path on the local storage
        /// </summary>
        public string fullPath { get; set; }
        /// <summary>
        /// File extension, without the preceding dot
        /// </summary>
        public string fileExtension { get; set; }
        /// <summary>
        /// An upload progress that should be incremented by relevant upload providers.
        /// </summary>
        public int uploadPercentComplete { get; set; }
        /// <summary>
        /// Used to instruct upload providers to cancel the current file upload.
        /// </summary>
        public CancellationTokenSource uploadCancellation { get; private set; }
        /// <summary>
        /// If the file is being uploaded, this represents the upload task.
        /// </summary>
        public Task uploadTask { get; set; }

        public MpFileUpload()
        {
            this.uploadCancellation = new CancellationTokenSource();
        }
    }
}
