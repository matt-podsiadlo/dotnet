using System;
using System.Threading;
using System.Threading.Tasks;

namespace mpBackup
{
    /// <summary>
    /// Provides specification for implementing custom backup providers.
    /// </summary>
    public abstract class MpBackupProvider
    {
        /// <summary>
        /// If the backup provider is currently awaiting authentication, this will be available to cancel the operation.
        /// </summary>
        public CancellationTokenSource cancelAuthentication { get; set; }

        #region Events
        /// <summary>
        /// Fires when the backup provider requires user input to authenticate with its online service.
        /// Authentication URL is provided as event data.
        /// </summary>
        public event EventHandler<string> AuthenticationRequired;
        
        /// <summary>
        /// Fires when the backup provider is succesfuly authenticated. GUI can bind onto this event to dispose any spawned elements.
        /// </summary>
        public event EventHandler AuthenticationSuccessful;
        #endregion

        #region Public Methods
        /// <summary>
        /// Instruct the provider to upload a single file to the cloud, asynchronously.
        /// </summary>
        /// <param name="localFile">File to be uploaded.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public abstract void uploadFile(MpFileUpload localFile, CancellationToken cancellationToken);
        /// <summary>
        /// Authenticate the backup provider with its online service. This metod waits for a valid Http response until succesful, or canceled.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public abstract Task authenticate(CancellationToken cancellationToken); // TODO implement internet connectivity checks before authentication is performed.
        /// <summary>
        /// Initializes the backup provider.
        /// </summary>
        /// <param name="cancellationToken"></param>
        public abstract Task<bool> initialize(CancellationToken cancellationToken);
        #endregion

        protected virtual void OnAuthenticationSuccessful(EventArgs e)
        {
            this.cancelAuthentication = null;
            if (this.AuthenticationSuccessful != null)
            {
                AuthenticationSuccessful(this, e);
            }
        }
    }
}
