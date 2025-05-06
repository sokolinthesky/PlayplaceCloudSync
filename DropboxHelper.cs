using System;
using System.Threading.Tasks;
using Dropbox.Api;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;

namespace PlayplaceCloudSync
{

    public class DropboxHelper
    {

        private readonly PlayplaceCloudSync plugin;

        private readonly Uri redirectUri = null;
        private readonly string appKey = "bejyrfv8fdvvwm3";
        private readonly string dbxAuthCodeFilePath;
        private readonly string dbxCodeVerifierFilePath;
        private readonly string dbxAccessTokenFilePath;

        public DropboxHelper(PlayplaceCloudSync plugin) 
        {
            this.plugin = plugin;
            dbxAuthCodeFilePath = Path.Combine(plugin.GetPluginUserDataPath(), "dbx-auth-code.json");
            dbxCodeVerifierFilePath = Path.Combine(plugin.GetPluginUserDataPath(), "dbx-code-verifier.json");
            dbxAccessTokenFilePath = Path.Combine(plugin.GetPluginUserDataPath(), "dbx-access-token.json");
        }

        public string AuthUri()
        {
            var codeVerifier = DropboxOAuth2Helper.GeneratePKCECodeVerifier();
            var codeChallenge = DropboxOAuth2Helper.GeneratePKCECodeChallenge(codeVerifier);

            File.WriteAllText(dbxCodeVerifierFilePath, codeVerifier);

            return DropboxOAuth2Helper.GetAuthorizeUri(
                OAuthResponseType.Code, 
                appKey, redirectUri, null, false, false, null, false, TokenAccessType.Offline, null, IncludeGrantedScopes.None, 
                codeChallenge).ToString();
        }

        public void Upload(FileStream fileStream, string filePath)
        {
            var authCode = File.ReadAllText(dbxAuthCodeFilePath);
            var accessToken = GetAccessTokenAsync(authCode);

            var dbx = new DropboxClient(accessToken);

            dbx.Files.UploadAsync(
                    filePath,
                    Dropbox.Api.Files.WriteMode.Overwrite.Instance,
                    body: fileStream).GetAwaiter().GetResult();

        }

        public string Download(string filePath)
        {
            var authCode = File.ReadAllText(dbxAuthCodeFilePath);
            var accessToken = GetAccessTokenAsync(authCode);

            var dbx = new DropboxClient(accessToken);

            var responseDownload = dbx.Files.DownloadAsync(filePath).GetAwaiter().GetResult();
           
            return responseDownload.GetContentAsStringAsync().GetAwaiter().GetResult();
        }

        public string GetAccessTokenAsync(string code)
        {
            try
            {
                if (File.Exists(dbxAccessTokenFilePath))
                {
                    var savedTokenResponseContent = File.ReadAllText(dbxAccessTokenFilePath);
                    var savedTokenResponse = JObject.Parse(savedTokenResponseContent);

                    var expiresIn = savedTokenResponse["expires_in"].ToObject<int>();
                    if (DateTime.Now.AddSeconds(expiresIn) < DateTime.Now)
                    {
                        return savedTokenResponse["access_token"].ToString();
                    } 
                    else
                    {
                        var newAccessTokenReponse = GetAccessTokenByRefreshToken(savedTokenResponse["refresh_token"].ToString(), appKey).GetAwaiter().GetResult();
                        File.WriteAllText(dbxAccessTokenFilePath, newAccessTokenReponse.ToString());

                        return newAccessTokenReponse["access_token"].ToString();
                    }
                } 
                else 
                { 
                    var codeVerifier = File.ReadAllText(dbxCodeVerifierFilePath);

                    var resp = ProcessCodeFlowAsync(code, appKey, null, null, null, codeVerifier).GetAwaiter().GetResult();

                    File.WriteAllText(dbxAccessTokenFilePath, resp.ToString());

                    return resp["access_token"].ToString();
                }
            }
            catch (OAuth2Exception ex)
            {
                throw new OAuth2Exception($"unable to get access token: {ex.Message}");
            }
        }

        private async Task<JObject> GetAccessTokenByRefreshToken(string refreshToken, string clientId)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "client_id", clientId }
            };
            HttpClient httpClient = new HttpClient();
            try
            {
                FormUrlEncodedContent content = new FormUrlEncodedContent(dictionary);
                HttpResponseMessage response = await httpClient.PostAsync("https://api.dropbox.com/oauth2/token", content).ConfigureAwait(continueOnCapturedContext: false);
                JObject jObject = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false));
                
                jObject.Add("refresh_token", refreshToken);

                return jObject;
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        public static async Task<JObject> ProcessCodeFlowAsync(string code, string appKey, string appSecret = null, string redirectUri = null, HttpClient client = null, string codeVerifier = null)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            if (string.IsNullOrEmpty(appKey))
            {
                throw new ArgumentNullException("appKey");
            }

            if (string.IsNullOrEmpty(appSecret) && string.IsNullOrEmpty(codeVerifier))
            {
                throw new ArgumentNullException("appSecret or codeVerifier");
            }

            HttpClient httpClient = client ?? new HttpClient();
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

                if (!string.IsNullOrEmpty(codeVerifier))
                {
                    dictionary["code_verifier"] = codeVerifier;
                }

                if (!string.IsNullOrEmpty(redirectUri))
                {
                    dictionary["redirect_uri"] = redirectUri;
                }

                FormUrlEncodedContent content = new FormUrlEncodedContent(dictionary);
                HttpResponseMessage response = await httpClient.PostAsync("https://api.dropbox.com/oauth2/token", content).ConfigureAwait(continueOnCapturedContext: false);
                JObject jObject = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false));
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new OAuth2Exception(jObject["error"].ToString(), jObject.Value<string>("error_description"));
                }

                return jObject;
            }
            finally
            {
                if (client == null)
                {
                    httpClient.Dispose();
                }
            }
        }

    }
}
