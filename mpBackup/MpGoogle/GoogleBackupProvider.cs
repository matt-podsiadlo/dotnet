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
        private MpConfig config;
        /// <summary>
        /// Location of the secrets file. Using AppDomain since the executable will be residing in system32.
        /// </summary>
        private string clientSecrets = System.AppDomain.CurrentDomain.BaseDirectory + "client_secrets.json";

        public GoogleBackupProvider(MpConfig config)
        {
            log.Info("Creating the Google Drive service.");
            this.config = config;
            if (config.googleDriveSettings.backupFolderId == null || config.googleDriveSettings.backupFolderId == "")
            {
                initialize();
            }
            else
            {
                this.backupFolderId = config.googleDriveSettings.backupFolderId;
            }
            
        }

        /// <summary>
        /// Get the names (with .extension) of all files currently in the mpBackup folder on Drive.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> getUploadedFileNames()
        {
            log.Info("Getting the list of uploaded files.");
            DriveService service;
            try
            {
                service = await authenticate();
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
        public async Task uploadFiles(List<string> filesToUpload)
        {
            DriveService service;
            try
            {
                service = await authenticate();
            }
            catch (Exception e)
            {
                log.Error("Authentication failed: ", e);
                return;
            }
            foreach (string fileName in filesToUpload)
            {
                log.Info("Started uploading [" + fileName + "]");
                string fullFilePath = this.config.backupDirectory.fullPath + "\\" + fileName;
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
        private async Task<DriveService> authenticate()
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
                MpGoogleAuthenticator authenticator = new MpGoogleAuthenticator(stream, Scopes);
                credential = await authenticator.authorizeAsync("user", CancellationToken.None);
                //credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, Scopes, "user",
                //    CancellationToken.None);
            }
            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "mpBackup"
            });
        }

        /// <summary>
        /// This should only run once, when the application is initially launched. Initializes the backup folder and GUID for files.
        /// </summary>
        private async void initialize()
        {
            log.Info("Initializing Google Drive structure.");
            DriveService service;
            try
            {
                service = await authenticate();
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
                this.config.googleDriveSettings.backupFolderId = folder.Items[0].Id;
                MpConfigManger configManager = new MpConfigManger(this.config);
                configManager.saveConfig();
                this.backupFolderId = folder.Items[0].Id;
            }
        }
    }
}