using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using mpBackup.mpGUI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mpBackup.MpGoogle
{
    public class MpCodeReceiver
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>The call back format. Expects one port parameter.</summary>
        private const string LoopbackCallback = "http://localhost:{0}/authorize/";

        private AuthenticationForm authForm;

        private string redirectUri;
        public string RedirectUri
        {
            get
            {
                if (!string.IsNullOrEmpty(redirectUri))
                {
                    return redirectUri;
                }

                return redirectUri = string.Format(LoopbackCallback, GetRandomUnusedPort());
            }
        }

        public AuthorizationCodeResponseUrl receiveCode(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
        {
            CancellationTokenSource globalTimeout = new CancellationTokenSource(60000); // TODO use app.config for timeout
            string authorizationUrl = url.Build().ToString();
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(RedirectUri);
                listener.Start();
                log.Debug("Open a browser with \"" + authorizationUrl + "\" URL");
                Thread browserThread = new Thread(() => openAuthorizationForm(authorizationUrl));
                browserThread.SetApartmentState(ApartmentState.STA);
                browserThread.Start();
                var context = listener.GetContextAsync();
                try
                {
                    while (!taskCancellationToken.IsCancellationRequested && !globalTimeout.Token.IsCancellationRequested)
                    {

                        if (taskCancellationToken.IsCancellationRequested || globalTimeout.Token.IsCancellationRequested)
                        {
                            log.Error("The operation was canceled or timed out.");
                            taskCancellationToken.ThrowIfCancellationRequested();
                        }
                        else if (context.IsCompleted)
                        {
                            NameValueCollection coll = context.Result.Request.QueryString;
                            context.Result.Response.OutputStream.Close();
                            return new AuthorizationCodeResponseUrl(coll.AllKeys.ToDictionary(k => k, k => coll[k]));
                        }
                        Thread.Sleep(100); 
                    }
                }
                finally
                {
                    listener.Close();
                    if (browserThread.IsAlive)
                    {
                        this.authForm.BeginInvoke(new MethodInvoker(this.authForm.Close));
                        this.authForm.BeginInvoke(new MethodInvoker(this.authForm.Dispose));
                    }
                }
                throw new Exception("Receiving the authentication code was interrupted.");
            }

        }

        private Task<AuthorizationCodeResponseUrl> cancelListening(Task<Task<AuthorizationCodeResponseUrl>> t, HttpListener listener)
        {
            log.Error("Listening was cancelled due to a timeout.");
            listener.Stop();
            return t.Result;
        }

        /// <summary>Returns a random, unused port.</summary>
        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            try
            {
                listener.Start();
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }

        private void openAuthorizationForm(string authUrl)
        {
            this.authForm = new AuthenticationForm(authUrl);
            Application.Run(this.authForm);
        }
    }
}
