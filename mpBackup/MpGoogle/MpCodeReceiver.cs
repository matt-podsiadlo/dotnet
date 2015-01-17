using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mpBackup.MpGoogle
{
    public class MpCodeReceiver : ICodeReceiver
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>The call back format. Expects one port parameter.</summary>
        private const string LoopbackCallback = "http://localhost:{0}/authorize/";

        /// <summary>Close HTML tag to return the browser so it will close itself.</summary>
        private const string ClosePageResponse =
@"<html>
  <head><title>OAuth 2.0 Authentication Token Received</title></head>
  <body>
    Received verification code. Closing...
    <script type='text/javascript'>
      window.setTimeout(function() {
          window.open('', '_self', ''); 
          window.close(); 
        }, 1000);
      if (window.opener) { window.opener.checkToken(); }
    </script>
  </body>
</html>";

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

        public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url,
            CancellationToken taskCancellationToken)
        {
            var authorizationUrl = url.Build().ToString();
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(RedirectUri);
                try
                {
                    listener.Start();

                    log.Debug("Open a browser with \"" + authorizationUrl + "\" URL");
                    Process.Start(authorizationUrl);

                    // Wait to get the authorization code response.
                    var context = await listener.GetContextAsync().ConfigureAwait(false);
                    NameValueCollection coll = context.Request.QueryString;

                    // Write a "close" response.
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                    {
                        writer.WriteLine(ClosePageResponse);
                        writer.Flush();
                    }
                    context.Response.OutputStream.Close();

                    // Create a new response URL with a dictionary that contains all the response query parameters.
                    return new AuthorizationCodeResponseUrl(coll.AllKeys.ToDictionary(k => k, k => coll[k]));
                }
                finally
                {
                    listener.Close();
                }
            }
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
    }
}
