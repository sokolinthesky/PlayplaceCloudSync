using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlayplaceCloudSync.View;
using PlayplaceCloudSync.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PlayplaceCloudSync
{
    public class PlayplaceCloudSync : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private PlayplaceCloudSyncSettingsViewModel settings { get; set; }
        private DropboxHelper dropboxHelper { get; set; }

        public override Guid Id { get; } = Guid.Parse("f9f02b21-390d-4c46-b06a-09f501bd2935");

        public PlayplaceCloudSync(IPlayniteAPI api) : base(api)
        {
            settings = new PlayplaceCloudSyncSettingsViewModel(this);
            dropboxHelper = new DropboxHelper(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            string menuSection = "@Playplace Cloud Sync";
            return new List<MainMenuItem>
            {
                new MainMenuItem
                {
                    Description = "Open auth/sync window",
                    MenuSection = menuSection,
                    Action = _ =>
                    {
                        OpenAuthWindow();
                    }
                }
            };
        }

        private void OpenAuthWindow()
        {
            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });
            window.Height = 300;
            window.Width = 550;
            window.Title = "Auth/Sync";
            window.Content = new AuthView();
            window.DataContext = new AuthViewModel(PlayniteApi, settings, dropboxHelper, this);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            // Add code to be executed when Playnite is initialized.
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new PlayplaceCloudSyncSettingsView();
        }
    }
}