using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using mpBackup.MpGUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mpBackup.MpGoogle
{
    /// <summary>
    /// Use this class to handle Google authentication. Provides appropriate delegates to handle Task continuation.
    /// </summary>
    public class MpGoogleAuthenticator
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAuthorizationCodeFlow flow;
        private readonly MpCodeReceiver codeReceiver;
        /// <summary>The folder which is used by the <seealso cref="Google.Apis.Util.Store.FileDataStore"/>.</summary>
        public static string Folder = "Google.Apis.Auth";

        MpMessageQueue messageQueue;

        public MpGoogleAuthenticator(Stream clientSecretsStream, string[] scopes, MpMessageQueue messageQueue, IDataStore dataStore = null)
        {
            this.messageQueue = messageQueue;
            this.flow = new AuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecretsStream = clientSecretsStream,
                    Scopes = scopes,
                    DataStore = dataStore ?? new FileDataStore(Folder)
                });
            this.codeReceiver = new MpCodeReceiver();
        }

        public async Task<UserCredential> authorizeAsync(string userId, CancellationToken taskCancellationToken)
        {
            CancellationTokenSource globalTimeout = new CancellationTokenSource(60000); // TODO use app.config for timeout
            // Try to load a token from the data store.
            var token = await this.flow.LoadTokenAsync(userId, globalTimeout.Token);
            // If the stored token is null or it doesn't have a refresh token and the access token is expired we need 
            // to retrieve a new authorization code.
            if (token == null || (token.RefreshToken == null && token.IsExpired(flow.Clock)))
            {
                log.Debug("Authorization token was not found in local storage. Attempting to request one from Google.");
                // Create a authorization code request.
                string redirectUri = this.codeReceiver.RedirectUri;
                AuthorizationCodeRequestUrl codeRequest = this.flow.CreateAuthorizationCodeRequest(redirectUri);

                // Receive the code.
                AuthorizationCodeResponseUrl response = this.codeReceiver.receiveCode(codeRequest, taskCancellationToken);

                if (string.IsNullOrEmpty(response.Code))
                {
                    TokenErrorResponse errorResponse = new TokenErrorResponse(response);
                    log.Error("Received an error. The response is: " + errorResponse.ErrorDescription);
                    throw new TokenResponseException(errorResponse);
                }

                log.Debug("Received \"" + response.Code + "\" code");

                // Get the token based on the code.
                token = await this.flow.ExchangeCodeForTokenAsync(userId, response.Code, this.codeReceiver.RedirectUri,
                    taskCancellationToken);
            }
            else
            {
                log.Debug("Using authentication token from local storage.");
            }
            return new UserCredential(flow, userId, token);
        }

        /// <summary>
        /// Perform a synchronous authorization.
        /// </summary>
        /// <returns></returns>
        public UserCredential authorize(string userId, CancellationToken cancellationToken)
        {
            // Try to load a token from the data store.
            Task<TokenResponse> token = this.flow.LoadTokenAsync(userId, cancellationToken);
            while (cancellationToken.CanBeCanceled)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    log.Error("Authorization canceled/timed out.");
                    cancellationToken.ThrowIfCancellationRequested();
                }
                else if (token.IsCompleted)
                {
                    // If the stored token is null or it doesn't have a refresh token and the access token is expired we need 
                    // to retrieve a new authorization code.
                    if (token.Result == null || (token.Result.RefreshToken == null && token.Result.IsExpired(flow.Clock)))
                    {
                        log.Debug("Authorization token was not found in local storage. Attempting to request one from Google.");
                        this.messageQueue.addMessageAsync(new MpMessage()
                            {
                                text = "Authorization required",
                                displayAs = MpMessage.DisplayAs.BALOON
                            });
                        // Create a authorization code request.
                        string redirectUri = this.codeReceiver.RedirectUri;
                        AuthorizationCodeRequestUrl codeRequest = this.flow.CreateAuthorizationCodeRequest(redirectUri);

                        // Receive the code.
                        AuthorizationCodeResponseUrl response = this.codeReceiver.receiveCode(codeRequest, cancellationToken);

                        if (string.IsNullOrEmpty(response.Code))
                        {
                            TokenErrorResponse errorResponse = new TokenErrorResponse(response);
                            log.Error("Received an error. The response is: " + errorResponse.ErrorDescription);
                            throw new TokenResponseException(errorResponse);
                        }

                        log.Debug("Received the authorization code.");

                        // Get the token based on the code.
                        token = this.flow.ExchangeCodeForTokenAsync(userId, response.Code, this.codeReceiver.RedirectUri,
                            cancellationToken);
                        while (cancellationToken.CanBeCanceled)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                log.Error("Authorization canceled/timed out.");
                                cancellationToken.ThrowIfCancellationRequested();
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
            }
            return new UserCredential(flow, userId, token.Result);
        }
    }
}