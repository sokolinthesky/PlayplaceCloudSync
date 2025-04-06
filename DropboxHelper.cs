using System;
using System.Threading.Tasks;
using Dropbox.Api;
using System.IO;

namespace PlayplaceCloudSync
{
    
    public class DropboxHelper
    {
        private readonly Uri redirectUri = null;
        private readonly string appKey = "bejyrfv8fdvvwm3";
        private readonly string appSecret = "<app-secret>";

        public string authUri()
        {
            return DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, appKey, redirectUri).ToString();
        }

        public async void Upload(string code, FileStream fileStream, string filePath)
        {
            var token = await getAccessTokenAsync(code);
            var dbx = new DropboxClient(token);

            var responseUpload = await dbx.Files.UploadAsync(
                    filePath,
                    Dropbox.Api.Files.WriteMode.Overwrite.Instance,
                    body: fileStream);

        }

        public async Task<string> Download(string filePath, string code)
        {
            var token = await getAccessTokenAsync(code);
            var dbx = new DropboxClient(token);

            var responseDownload = await dbx.Files.DownloadAsync(filePath);
           
            return await responseDownload.GetContentAsStringAsync();
        }

        private async Task<string> getAccessTokenAsync(string code)
        {
            var response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(
                    code,
                    appKey,
                    appSecret,
                    null);

            return response.AccessToken;
        }
    }
}
