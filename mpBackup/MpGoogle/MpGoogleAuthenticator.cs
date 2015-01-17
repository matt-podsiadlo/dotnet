using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
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
        private readonly ICodeReceiver codeReceiver;

        public MpGoogleAuthenticator(Stream clientSecretsStream, string[] scopes)
        {

            this.flow = new AuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecretsStream = clientSecretsStream,
                    Scopes = scopes
                });
            this.codeReceiver = new MpCodeReceiver();
        }

        public async Task<UserCredential> authorizeAsync(string userId, CancellationToken taskCancellationToken)
        {
            // Try to load a token from the data store.
            var token = await this.flow.LoadTokenAsync(userId, taskCancellationToken);
            // If the stored token is null or it doesn't have a refresh token and the access token is expired we need 
            // to retrieve a new authorization code.
            if (token == null || (token.RefreshToken == null && token.IsExpired(flow.Clock)))
            {
                // Create a authorization code request.
                string redirectUri = this.codeReceiver.RedirectUri;
                AuthorizationCodeRequestUrl codeRequest = this.flow.CreateAuthorizationCodeRequest(redirectUri);

                // Receive the code.
                var response = await this.codeReceiver.ReceiveCodeAsync(codeRequest, taskCancellationToken)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(response.Code))
                {
                    TokenErrorResponse errorResponse = new TokenErrorResponse(response);
                    log.Info("Received an error. The response is: " + errorResponse.ErrorDescription);
                    throw new TokenResponseException(errorResponse);
                }

                log.Debug("Received \"" + response.Code + "\" code");

                // Get the token based on the code.
                token = await this.flow.ExchangeCodeForTokenAsync(userId, response.Code, this.codeReceiver.RedirectUri,
                    taskCancellationToken);
            }
            return new UserCredential(flow, userId, token);
        }

    }
}