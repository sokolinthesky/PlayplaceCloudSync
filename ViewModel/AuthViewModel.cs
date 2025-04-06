using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Playnite.SDK.Models;
using Playnite.SDK;
using System.IO;
using static Dropbox.Api.Files.SearchMatchType;

namespace PlayplaceCloudSync.ViewModel
{
    class AuthViewModel
    {
        public PlayplaceCloudSyncSettingsViewModel Settings { get; }
        public string AuthUri { get; set; }

        private readonly IPlayniteAPI playniteApi;
        private readonly DropboxHelper dropboxHelper;
        private readonly PlayplaceCloudSync plugin;

        private readonly string dropboxTokenPath;

        public AuthViewModel(IPlayniteAPI playniteApi, PlayplaceCloudSyncSettingsViewModel settings, DropboxHelper dropboxHelper, PlayplaceCloudSync plugin)
        {
            this.playniteApi = playniteApi;
            this.dropboxHelper = dropboxHelper;
            this.plugin = plugin;

            dropboxTokenPath = Path.Combine(plugin.GetPluginUserDataPath(), "dpb-token.json");

            Settings = settings;
            AuthUri = dropboxHelper.authUri();
        }


        public RelayCommand DropboxAuthCommand
        {
            get => new RelayCommand(() =>
            {
               if (File.Exists(dropboxTokenPath))
                {
                    File.Delete(dropboxTokenPath);
                }

                File.WriteAllText(dropboxTokenPath, Settings.Settings.DropboxAuthCode);
            });
        }

    }
}
