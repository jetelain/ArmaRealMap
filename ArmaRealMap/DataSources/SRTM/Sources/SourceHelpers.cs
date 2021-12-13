using SRTM.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace SRTM.Sources
{
    public class SourceHelpers
    {
        /// <summary>
        /// Downloads a remote file and stores the data in the local one.
        /// </summary>
        public static bool Download(string local, string remote, bool logErrors = false)
        {
            var client = new HttpClient();
            return PerformDownload(client, local, remote, logErrors);
        }

        /// <summary>
        /// Downloads a remote file and stores the data in the local one. The given credentials are used for authorization.
        /// </summary>
        public static bool DownloadWithCredentials(NetworkCredential credentials, string local, string remote,
            bool logErrors = false)
        {
            //HttpClientHandler handler = new HttpClientHandler { Credentials = credentials };
            var client = new HttpClient();

            var authenticationString = $"{credentials.UserName}:{credentials.Password}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(authenticationString));
            client.DefaultRequestHeaders.Add("Cookie", "DATA=XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

            return PerformDownload(client, local, remote, logErrors);
        }

        private static bool PerformDownload(HttpClient client, string local, string remote, bool logErrors = false)
        {
            var Logger = LogProvider.For<SourceHelpers>();

            try
            {
                var data = client.GetByteArrayAsync(remote).ConfigureAwait(false).GetAwaiter().GetResult();
                File.WriteAllBytes(local, data);
                return true;
            }
            catch (Exception ex)
            {
                if (logErrors)
                {
                    Logger.ErrorException("Download failed.", ex);
                }
            }
            return false;
        }
    }
}
