using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Playnite.SDK.Models;
using Playnite.SDK;

namespace PlayplaceCloudSync.ViewModel
{
    class AuthViewModel
    {
        public PlayplaceCloudSyncSettingsViewModel Settings { get; }
        public string AuthUri { get; set; }

        private readonly IPlayniteAPI playniteApi;
        private readonly DropboxHelper dropboxHelper;

        public AuthViewModel(IPlayniteAPI playniteApi, PlayplaceCloudSyncSettingsViewModel settings, DropboxHelper dropboxHelper)
        {
            this.playniteApi = playniteApi;
            this.dropboxHelper = dropboxHelper;
            Settings = settings;
            AuthUri = dropboxHelper.authUri();
        }


        public RelayCommand DropboxAuthCommand
        {
            get => new RelayCommand(() =>
            {
                //todo impl
            });
        }

    }
}
