using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;
using static Dropbox.Api.TeamLog.ActorLogInfo;
using static Dropbox.Api.Team.DesktopPlatform;

namespace PlayplaceCloudSync
{
    
    public class DropboxHelper
    {
        public readonly Uri RedirectUri = null;
        public readonly string AppKey = "bejyrfv8fdvvwm3";

        public string authUri()
        {
            return DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, AppKey, RedirectUri).ToString();
        }
    }
}
