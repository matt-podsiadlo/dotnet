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

namespace mpBackup
{
    class GoogleBackupProvider
    {
        private DriveService service;
        /// <summary>The Drive API scopes.</summary>
        private static readonly string[] Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive };

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string backupFolderId;

        public GoogleBackupProvider()
        {
            log.Info("Creating the Google Drive service.");
            initialize();
        }

        private async Task uploadFileAsync(DriveService service)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> getUploadedFileNames()
        {
            DriveService service = await authenticate();
            FilesResource.ListRequest request = service.Files.List();
            request.Q = "'" + this.backupFolderId + "' in parents"; // List only files in the mpBackup directory
            FileList files;
            files = request.Execute();
            return files.Items.Select(t => t.Title).ToList();
        }

        /// <summary>
        /// Return an authenticated DriveService, should be an entry point for all Drive operations.
        /// </summary>
        /// <returns></returns>
        private async Task<DriveService> authenticate()
        {

            UserCredential credential;
            using (FileStream stream = new System.IO.FileStream("client_secrets.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                log.Info("Attempting to authenticate with Google.");
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, Scopes, "user", CancellationToken.None);
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
            DriveService service = await authenticate();
            FilesResource.ListRequest request = service.Files.List();
            request.Q = "title='mpBackup' and mimeType='application/vnd.google-apps.folder' and trashed=false";
            FileList folder = request.Execute();
            if (folder.Items.Count != 1)
            {
                log.Info("Creating the mpBackup folder on Drive.");
                FilesResource.InsertRequest createFolder = service.Files.Insert(new Google.Apis.Drive.v2.Data.File()
                    {
                        MimeType = "application/vnd.google-apps.folder",
                        Title = "mpBackup",
                        Description = "Files archived with mpBackup are kept here."
                    });
                createFolder.Execute();
            }
            else
            {
                this.backupFolderId = folder.Items[0].Id;
            }
        }
    }
}