using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Logging;
using Google.Apis.Services;
using System.Threading;
using System.IO;
using Google.Apis.Http;
using Google.Apis.Util.Store;
using mpBackup.MpGoogle;

namespace mpBackup
{
    class GoogleBackupProvider
    {
        /// <summary>The Drive API scopes.</summary>
        private static readonly string[] Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive };

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string backupFolderId;
        private MpBackupProcess backupProcess;

        /// <summary>
        /// Location of the secrets file.
        /// </summary>
        private string clientSecrets = System.AppDomain.CurrentDomain.BaseDirectory + "client_secrets.json";

        public GoogleBackupProvider(MpBackupProcess backupProcess, CancellationToken initToken)
        {
            log.Info("Creating the Google Drive service.");
            this.backupProcess = backupProcess;
            if (this.backupProcess.settingsManager.settings.onlineFolderId == null || this.backupProcess.settingsManager.settings.onlineFolderId == "")
            {
                initialize(initToken);
            }
            else
            {
                this.backupFolderId = this.backupProcess.settingsManager.settings.onlineFolderId;
            }
        }

        /// <summary>
        /// Get the names (with .extension) of all files currently in the mpBackup folder on Drive.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> getUploadedFileNames(CancellationToken cancellationToken)
        {
            log.Info("Getting the list of uploaded files.");
            DriveService service;
            try
            {
                service = await authenticateAsync(cancellationToken);
            }
            catch (Exception e)
            {
                log.Error("Authentication failed: ", e);
                return null;
            }
            FilesResource.ListRequest request = service.Files.List();
            request.Q = "'" + this.backupFolderId + "' in parents"; // List only files in the mpBackup directory
            FileList files;
            files = request.Execute();
            return files.Items.Select(t => t.Title).ToList();
        }

        /// <summary>
        /// Upload files to drive, based on the provided list of file names.
        /// </summary>
        /// <param name="filesToUpload">Names of the files to upload, with .extension</param>
        public async Task uploadFiles(List<string> filesToUpload, CancellationToken cancellationToken)
        {
            DriveService service;
            try
            {
                service = await authenticateAsync(cancellationToken);
            }
            catch (Exception e)
            {
                log.Error("Authentication failed: ", e);
                return;
            }
            foreach (string fileName in filesToUpload)
            {
                log.Info("Started uploading [" + fileName + "]");
                string fullFilePath = this.backupProcess.settingsManager.settings.backupFolderPath + "\\" + fileName;
                string extension = fileName.Substring(fileName.IndexOf('.') + 1);
                string contentType = ContentTypes.getMimetypeForExtension(extension);
                FileStream uploadStream = new System.IO.FileStream(fullFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                FilesResource.InsertMediaUpload insert = service.Files.Insert(new Google.Apis.Drive.v2.Data.File 
                { 
                    Title = fileName,
                    // Ensuring the file ends up inside the backup folder:
                    Parents = new List<ParentReference>() { new ParentReference() {Id = this.backupFolderId} }
                }, uploadStream, contentType);

                insert.ChunkSize = FilesResource.InsertMediaUpload.MinimumChunkSize * 2;
                Task task = insert.UploadAsync();
                task.ContinueWith(t =>
                {
                    // NotOnRanToCompletion - this code will be called if the upload fails
                    log.Error("Upload of [" + fileName + "] failed.", t.Exception);
                }, TaskContinuationOptions.NotOnRanToCompletion);
                task.ContinueWith(t =>
                {
                    uploadStream.Dispose();
                    log.Info("File [" + fileName + "] uploaded succesfully.");
                });
                
            }
        }

        /// <summary>
        /// Return an authenticated DriveService, should be an entry point for all Drive operations.
        /// </summary>
        /// <returns></returns>
        private async Task<DriveService> authenticateAsync(CancellationToken cancellationToken)
        {
            UserCredential credential;
            CancellationTokenSource token = new CancellationTokenSource(20000);
            if (!System.IO.File.Exists(this.clientSecrets))
            {
                throw new Exception("The client secrets for Google API does not exist!");
            }
            using (FileStream stream = new System.IO.FileStream(this.clientSecrets, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                log.Info("Attempting to authenticate with Google.");
                MpGoogleAuthenticator authenticator = new MpGoogleAuthenticator(stream, Scopes, this.backupProcess.messageQueue);
                
                credential = await authenticator.authorizeAsync("user", cancellationToken);
            }
            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "mpBackup"
            });
        }

        private DriveService authenticate(CancellationToken cancellationToken)
        {
            UserCredential credential;
            if (!System.IO.File.Exists(this.clientSecrets))
            {
                throw new Exception("The client secrets for Google API does not exist!");
            }
            using (FileStream stream = new System.IO.FileStream(this.clientSecrets, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                log.Info("Attempting to authenticate with Google.");
                MpGoogleAuthenticator authenticator = new MpGoogleAuthenticator(stream, Scopes, this.backupProcess.messageQueue);
                credential = authenticator.authorize("user", cancellationToken);
            }
            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "mpBackup"
            });
        }

        /// <summary>
        /// This should only run once (synchronously), when the application is initially launched. Initializes the backup folder and GUID for files.
        /// </summary>
        private void initialize(CancellationToken initToken)
        {
            log.Info("Initializing Google Drive structure.");
            DriveService service;
            try
            {
                service = authenticate(initToken);
            }
            catch (Exception e)
            {
                log.Error("Authentication failed: ", e);
                return;
            }
            FilesResource.ListRequest request = service.Files.List();
            request.Q = "title='mpBackup' and mimeType='application/vnd.google-apps.folder' and trashed=false";
            FileList folder = request.Execute();
            if (folder.Items.Count != 1)
            {
                log.Info("Creating the mpBackup folder on Drive.");
                FilesResource.InsertRequest createFolder = service.Files.Insert(new Google.Apis.Drive.v2.Data.File()
                    {
                        MimeType = ContentTypes.GOOGLE_DRIVE_FOLDER,
                        Title = "mpBackup",
                        Description = "Files archived with mpBackup are kept here."
                    });
                createFolder.Execute();
            }
            else
            {
                this.backupProcess.settingsManager.setBackupFolderId(folder.Items[0].Id);
                this.backupProcess.settingsManager.saveSettings();
                this.backupFolderId = folder.Items[0].Id;
            }
        }
    }
}