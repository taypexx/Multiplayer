using Il2CppAssets.Scripts.Database;
using Multiplayer.Data;
using Il2CppSystem;

namespace Multiplayer.Managers
{
    internal static class PlayerManager
    {
        internal static List<Player> CachedPlayers { get; private set; }
        internal static Player LocalPlayer { get; private set; }

        /// <summary>
        /// Finds/creates a <see cref="Player"/> by their <paramref name="uid"/>.
        /// </summary>
        /// <param name="uid">UID of a player.</param>
        /// <returns>A <see cref="Player"/> that was cached or a new instance.</returns>
        internal static Player GetPlayer(string uid)
        {
            foreach (Player player in CachedPlayers) 
            {
                if (player.Uid == uid)
                {
                    return player;
                }
            }
            
            // If not cached
            Player newPlayer = new(uid);
            CachePlayer(newPlayer);
            return newPlayer;
        }

        /// <summary>
        /// Sends an update request to server to update some parameters in the db.
        /// </summary>
        internal static async void SyncLocalPlayer()
        {
            var payload = new 
            {
                Uid = LocalPlayer.Uid,
                Name = LocalPlayer.MultiplayerStats.Name,
                AvatarName = LocalPlayer.MultiplayerStats.AvatarName,
                Achievements = LocalPlayer.MultiplayerStats.Achievements
            };
            var response = await Client.PostAsync("update",payload);
            if (response == null) return;
        }

        /// <summary>
        /// Caches the <see cref="Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> to cache.</param>
        /// <returns><see langword="true"/> if <see cref="Player"/> was successfully cached or <see langword="false"/> if they were cached already.</returns>
        private static bool CachePlayer(Player player)
        {
            if (CachedPlayers.Contains(player)) return false;

            CachedPlayers.Add(player);
            return true;
        }

        internal static void Init()
        {
            /*if (!DataHelper.isLogin)
            {
                //UIManager.WarnNotification(Localization.Get("Warning","NoAccount"));
                return;
            }*/

            CachedPlayers = new();
            LocalPlayer = GetPlayer(DiscordManager.Client.CurrentUser.ID.ToString());

            AchievementManager.Achieve(0);
        }
    }
}
