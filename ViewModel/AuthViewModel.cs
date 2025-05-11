using System.Collections.Generic;
using Newtonsoft.Json;
using Playnite.SDK.Models;
using Playnite.SDK;
using System.IO;
using System;

namespace PlayplaceCloudSync.ViewModel
{
    class AuthViewModel
    {
        public PlayplaceCloudSyncSettingsViewModel Settings { get; }
        public string AuthUri { get; set; }

        private readonly IPlayniteAPI playniteApi;
        private readonly DropboxHelper dropboxHelper;
        private readonly PlayplaceCloudSync plugin;

        private readonly string dbxAuthCodePath;

        public AuthViewModel(IPlayniteAPI playniteApi, PlayplaceCloudSyncSettingsViewModel settings, DropboxHelper dropboxHelper, PlayplaceCloudSync plugin)
        {
            this.playniteApi = playniteApi;
            this.dropboxHelper = dropboxHelper;
            this.plugin = plugin;

            dbxAuthCodePath = Path.Combine(plugin.GetPluginUserDataPath(), "dbx-auth-code.json");

            Settings = settings;
            AuthUri = dropboxHelper.AuthUri();
        }


        public RelayCommand DropboxAuthCommand
        {
            get => new RelayCommand(() =>
            {
                if (File.Exists(dbxAuthCodePath))
                {
                    File.Delete(dbxAuthCodePath);
                }

                File.WriteAllText(dbxAuthCodePath, Settings.Settings.DropboxAuthCode);

                playniteApi.Dialogs.ShowMessage("Dropbox auth code successfully saved!");
            });
        }

        public RelayCommand UploadGamesLibrary
        {
            get => new RelayCommand(() =>
            {
                var games = playniteApi.Database.Games;

                var jsonString = JsonConvert.SerializeObject(games, Formatting.Indented);
                var filePath = Path.Combine(plugin.GetPluginUserDataPath(), "to-upload-library.json");

                File.WriteAllText(filePath, jsonString);

                dropboxHelper.Upload(File.OpenRead(filePath), "/library.json");

                playniteApi.Dialogs.ShowMessage("Game library successfully uploaded to Dropbox!");
            });
        }

        public RelayCommand DownloadGamesLibrary
        {
            get => new RelayCommand(() =>
            {
                var libraryContent = dropboxHelper.Download("/library.json");

                var filePath = Path.Combine(plugin.GetPluginUserDataPath(), "downloaded-library.json");
                File.WriteAllText(filePath, libraryContent);

                Import(libraryContent, playniteApi.Database.Games);

                playniteApi.Dialogs.ShowMessage("Game library successfully imported from from Dropbox!");
            });
        }

        public void Import<T>(string content, IItemCollection<T> db) where T : DatabaseObject
        {
            IEnumerable<T> items;

            try
            {
                items = JsonConvert.DeserializeObject<IEnumerable<T>>(content);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to deserialize JSON content.", ex);
            }

            foreach (var item in items)
            {
                if (!db.Contains(item))
                {
                    db.Add(item);
                }
                else
                {
                    db.Update(item);
                }
            }
        }
    }
}
