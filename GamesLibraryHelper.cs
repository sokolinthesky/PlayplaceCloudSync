using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace PlayplaceCloudSync
{
    internal class GamesLibraryHelper
    {
        private readonly IPlayniteAPI playniteApi;

        public GamesLibraryHelper(IPlayniteAPI playniteApi)
        {
            this.playniteApi = playniteApi;
        }

        public IItemCollection<Game> findAllGames()
        {
            return playniteApi.Database.Games;
        }

    }
}
