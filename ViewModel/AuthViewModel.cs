using System.Collections.Generic;
using Newtonsoft.Json;
using Playnite.SDK.Models;
using Playnite.SDK;
using System.IO;

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
            });
        }

        public void Import<T>(string content, IItemCollection<T> db) where T : DatabaseObject
        {
            IEnumerable<T> items = JsonConvert.DeserializeObject<IEnumerable<T>>(content);

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
