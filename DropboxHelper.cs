using System;
using System.Threading.Tasks;
using Dropbox.Api;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace PlayplaceCloudSync
{

    public class DropboxHelper
    {
        private readonly Uri redirectUri = null;
        private readonly string appKey = "bejyrfv8fdvvwm3";
        private readonly string appSecret = "<secret>";

        public string AuthUri()
        {
            return DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, appKey, redirectUri).ToString();
        }

        public void Upload(string token, FileStream fileStream, string filePath)
        {
            var dbx = new DropboxClient(token);

            dbx.Files.UploadAsync(
                    filePath,
                    Dropbox.Api.Files.WriteMode.Overwrite.Instance,
                    body: fileStream).GetAwaiter().GetResult();

        }

        public string Download(string filePath, string token)
        {
            var dbx = new DropboxClient(token);

            var responseDownload = dbx.Files.DownloadAsync(filePath).GetAwaiter().GetResult();
           
            return responseDownload.GetContentAsStringAsync().GetAwaiter().GetResult();
        }

        public string GetAccessTokenAsync(string code)
        {
            try
            {
                //todo replace by DropboxOAuth2Helper.ProcessCodeFlowAsync(
                var task =  processCodeFlowAsyncAndGetToken(code, appKey,  appSecret);
                return task.GetAwaiter().GetResult();
            }
            catch (OAuth2Exception ex)
            {
                throw new OAuth2Exception($"unable to get access token: {ex.Message}");
            }
        }

        private async Task<string> processCodeFlowAsyncAndGetToken(string code, string appKey, string appSecret = null)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            if (string.IsNullOrEmpty(appKey))
            {
                throw new ArgumentNullException("appKey");
            }

            HttpClient httpClient = new HttpClient();
            try
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
            {
                { "code", code },
                { "grant_type", "authorization_code" },
                { "client_id", appKey }
            };
                if (!string.IsNullOrEmpty(appSecret))
                {
                    dictionary["client_secret"] = appSecret;
                }

                FormUrlEncodedContent content = new FormUrlEncodedContent(dictionary);
                HttpResponseMessage response = await httpClient.PostAsync("https://api.dropbox.com/oauth2/token", content).ConfigureAwait(continueOnCapturedContext: false);
                JObject jObject = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false));

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new OAuth2Exception(jObject["error"].ToString(), jObject.Value<string>("error_description"));
                }

                return jObject["access_token"].ToString();
            }
            finally
            {
                httpClient.Dispose();
            }
            
        }
    }
}
