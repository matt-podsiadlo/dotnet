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
using mpBackup.MpUtilities;
using Google.Apis.Upload;
using mpBackup.MpGoogle;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using System.Net;

namespace mpBackup
{
    class GoogleBackupProvider : MpBackupProvider
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
        private DriveService service;

        public event EventHandler<string> AuthenticationRequired;
        public event EventHandler AuthenticationSuccessful;

        public GoogleBackupProvider(MpBackupProcess backupProcess)
        {
            log.Info("Creating the Google Drive service.");
            this.backupProcess = backupProcess;            
        }

        /// <summary>
        /// Get the names (with .extension) of all files currently in the mpBackup folder on Drive.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> getUploadedFileNames(CancellationToken cancellationToken)
        {
            FilesResource.ListRequest request = this.service.Files.List();
            request.Q = "'" + this.backupFolderId + "' in parents"; // List only files in the mpBackup directory
            FileList files;
            files = request.Execute();
            return files.Items.Select(t => t.Title).ToList();
        }

        /// <summary>
        /// Uses the Google Drive API to upload a file to drive. Upload progress is tracked, cancellation is supported.
        /// </summary>
        /// <param name="localFile">Local file to upload.</param>
        /// <param name="cancellationToken">Token to cancel the upload cleanly.</param>
        public override async void uploadFile(MpFileUpload localFile, CancellationToken cancellationToken)
        {
            string contentType = ContentTypes.getMimetypeForExtension(localFile.fileExtension);
            FileStream uploadStream = new System.IO.FileStream(localFile.fullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            MpInsertMediaUpload insertCustom = new MpInsertMediaUpload(this.service, new Google.Apis.Drive.v2.Data.File
            {
                Title = localFile.fileName,
                // Ensuring the file ends up inside the backup folder:
                Parents = new List<ParentReference>() { new ParentReference() { Id = this.backupFolderId } }
            }, uploadStream, contentType, localFile);
            insertCustom.ChunkSize = FilesResource.InsertMediaUpload.MinimumChunkSize * 2;

            Task uploadTask = insertCustom.UploadAsync(cancellationToken);
            uploadTask.ContinueWith(t =>
            {
                log.Error("Upload of [" + localFile.fileName + "] was canceled.");
            }, TaskContinuationOptions.OnlyOnCanceled);
            uploadTask.ContinueWith(t =>
            {
                // NotOnRanToCompletion - this code will be called if the upload fails
                log.Error("Upload of [" + localFile.fileName + "] failed.", t.Exception);
            }, TaskContinuationOptions.NotOnRanToCompletion);
            uploadTask.ContinueWith(t =>
            {
                uploadStream.Dispose();
            });
            try
            {
                uploadTask.Wait();
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions.Count(ex => ex.GetType() != typeof(TaskCanceledException)) > 0)
                {
                    log.Error("Upload ended unexpectedly: ", e);
                }
            }
            
        }        

        /// <summary>
        /// This should only run once (synchronously), when the application is initially launched. Initializes the backup folder and GUID for files.
        /// </summary>
        public override async Task<bool> initialize(CancellationToken cancellationToken)
        {
            try
            {
                await authenticate(cancellationToken);
            }
            catch (Exception e)
            {
                return false;
            }
            if (this.backupProcess.settingsManager.settings.onlineFolderId == null || this.backupProcess.settingsManager.settings.onlineFolderId == "")
            {
                log.Info("Initializing Google Drive structure.");
                FilesResource.ListRequest request = this.service.Files.List();
                request.Q = "title='mpBackup' and mimeType='" + ContentTypes.GOOGLE_DRIVE_FOLDER + "' and trashed=false";
                FileList folder = request.Execute();
                if (folder.Items.Count != 1)
                {
                    log.Info("Creating the mpBackup folder on Drive.");
                    FilesResource.InsertRequest createFolder = this.service.Files.Insert(new Google.Apis.Drive.v2.Data.File()
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
                    this.backupProcess.settingsManager.saveSettings(false);
                    this.backupFolderId = folder.Items[0].Id;
                }
            }
            else
            {
                this.backupFolderId = this.backupProcess.settingsManager.settings.onlineFolderId;
            }
            return true;
        }

        public override async Task authenticate(CancellationToken cancellationToken)
        {
            UserCredential credential = null;
            base.cancelAuthentication = new CancellationTokenSource();
            if (!System.IO.File.Exists(this.clientSecrets))
            {
                throw new Exception("The client secrets for Google API does not exist!");
            }
            using (FileStream stream = new System.IO.FileStream(this.clientSecrets, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                log.Info("Attempting to authenticate with Google.");
                List<CancellationToken> cancellationTokens = new List<CancellationToken>()
                {
                    cancellationToken, base.cancelAuthentication.Token
                };
                try
                {
                    credential = getCredential("user", stream, cancellationTokens);
                }
                catch (Exception e)
                {
                    throw e;
                }
                if (base.cancelAuthentication != null) base.cancelAuthentication.Dispose();
                base.cancelAuthentication = null;
            }
            this.service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "mpBackup"
            });
        }

        private UserCredential getCredential(string userId, Stream clientSecretsStream, List<CancellationToken> cancellationTokens, IDataStore dataStore = null)
        {
            IAuthorizationCodeFlow flow = new AuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecretsStream = clientSecretsStream,
                Scopes = Scopes,
                DataStore = dataStore ?? new FileDataStore("mpBackup")
            });

            // Try to load a token from the data store.
            Task<TokenResponse> token = flow.LoadTokenAsync(userId, cancellationTokens.First());
            while (cancellationTokens.Count(c => c.CanBeCanceled) > 0)
            {
                if (cancellationTokens.Count(c => c.IsCancellationRequested) != 0)
                {
                    log.Error("Authorization canceled/timed out.");
                    cancellationTokens.Where(c => c.IsCancellationRequested).First().ThrowIfCancellationRequested();
                }
                else if (token.IsCompleted)
                {
                    // If the stored token is null or it doesn't have a refresh token and the access token is expired we need 
                    // to retrieve a new authorization code.
                    if (token.Result == null || (token.Result.RefreshToken == null && token.Result.IsExpired(flow.Clock)))
                    {
                        MpCodeReceiver codeReceiver = new MpCodeReceiver();
                        log.Info("Authorization token was not found in local storage. Attempting to request one from Google.");
                        // Create a authorization code request.
                        string redirectUri = codeReceiver.RedirectUri;
                        AuthorizationCodeRequestUrl codeRequest = flow.CreateAuthorizationCodeRequest(redirectUri);
                        if (AuthenticationRequired != null) AuthenticationRequired(this, codeRequest.Build().ToString());

                        // Receive the code.
                        AuthorizationCodeResponseUrl response = null;
                        try
                        {
                            response = codeReceiver.receiveCode(codeRequest, cancellationTokens);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Receiving the authentication code failed.");
                        }
                        
                        if (string.IsNullOrEmpty(response.Code))
                        {
                            TokenErrorResponse errorResponse = new TokenErrorResponse(response);
                            log.Error("Received an error. The response is: " + errorResponse.ErrorDescription);
                            throw new TokenResponseException(errorResponse);
                        }

                        log.Debug("Received the authorization code.");
                        OnAuthenticationSuccessful(EventArgs.Empty);

                        // Get the token based on the code.
                        token = flow.ExchangeCodeForTokenAsync(userId, response.Code, codeReceiver.RedirectUri,
                            cancellationTokens.First());
                        while (cancellationTokens.Count(c => c.CanBeCanceled) > 0)
                        {
                            if (cancellationTokens.Count(c => c.IsCancellationRequested) != 0)
                            {
                                log.Error("Authorization canceled/timed out.");
                                cancellationTokens.Where(c => c.IsCancellationRequested).First().ThrowIfCancellationRequested();
                            }
                            else if (token.IsCompleted)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        log.Debug("Using authentication token from local storage.");
                        break;
                    }
                }
                Thread.Sleep(50);
            }
            return new UserCredential(flow, userId, token.Result);
        }
    }
}