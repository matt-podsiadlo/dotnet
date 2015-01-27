using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using System.Threading;
using System.Threading.Tasks;

namespace mpBackup.MpGoogle
{
    /// <summary>
    /// A wrapper class for InsertMediaUpload that handles resumable uploads better (at least for mpBackup purposes).
    /// </summary>
    public class MpInsertMediaUpload : Google.Apis.Drive.v2.FilesResource.InsertMediaUpload
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MpFileUpload fileUpload;
        
        public MpInsertMediaUpload(IClientService service, File body, System.IO.Stream stream, string contentType, MpFileUpload localFile)
            : base (service, body, stream, contentType) 
        {
            this.fileUpload = localFile;
            log.Debug("Adding upload progress event listener.");
            base.ProgressChanged += MpInsertMediaUpload_ProgressChanged;
        }

        void MpInsertMediaUpload_ProgressChanged(IUploadProgress obj)
        {
            if (obj.Status == UploadStatus.Uploading)
            {
                int percentComplete = (int)(0.5f + ((100f * obj.BytesSent) / ContentStream.Length));
                fileUpload.uploadPercentComplete = percentComplete;
                log.Info("[" + fileUpload.fileName + "] upload progress: " + percentComplete);
            }
            if (obj.Status == UploadStatus.Completed)
            {
                log.Info("Finished uploading [" + fileUpload.fileName + "]");
            }
        }

        /// <summary>
        /// Start uploading the file.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void uploadFileCustom(CancellationToken cancellationToken)
        {

        }
    }
}
