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
    class GoogleTest
    {
        /// <summary>The Drive API scopes.</summary>
        private static readonly string[] Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive };

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public GoogleTest()
        {
            log.Info("Starting the Google Drive service.");
            run();
        }

        private async Task run()
        {
            GoogleWebAuthorizationBroker.Folder = "mpBackup";
            UserCredential credential;

            using (FileStream stream = new System.IO.FileStream("client_secrets.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                log.Info("Attempting to authenticate with Google.");
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, Scopes, "user", CancellationToken.None);
            }

            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "mpBackup"
            });

            try
            {
                await uploadFileAsync(service);
            }
            catch (Exception e)
            {
                log.Error("An error was caught: ", e);
            }
        }

        private Task uploadFileAsync(DriveService service)
        {
            throw new NotImplementedException();
        }
    }
}