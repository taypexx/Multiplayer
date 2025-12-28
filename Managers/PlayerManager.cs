using Il2CppAssets.Scripts.Database;
using Multiplayer.Data.Players;
using Multiplayer.Static;

namespace Multiplayer.Managers
{
    internal static class PlayerManager
    {
        internal static Dictionary<string,Player> CachedPlayers { get; private set; }

        internal static Player LocalPlayer { get; private set; }
        internal static string LocalPlayerUid { get; private set; }
        internal static string LocalPlayerName { get; private set; }
        internal static int LocalPlayerLVL { get; set; }

        /// <summary>
        /// Synchronizes profile stats of the local <see cref="Player"/>.
        /// </summary>
        internal static void SyncProfile()
        {
            if (!Client.Connected) return;

            var payload = new
            {
                Uid = LocalPlayerUid,
                Name = LocalPlayer.MultiplayerStats.Name,
                AvatarName = LocalPlayer.MultiplayerStats.AvatarName,
                Bio = LocalPlayer.MultiplayerStats.Bio,
                Level = LocalPlayer.MultiplayerStats.Level,
            };
            _ = Client.PostAsync("updatePlayer", payload);
        }

        /// <summary>
        /// Synchronizes achievements of the local <see cref="Player"/> with the server.
        /// </summary>
        internal static void SyncAchievements()
        {
            if (!Client.Connected) return;

            var achievementsConverted = new Dictionary<long, byte>();
            foreach ((DateTime timestamp, Achievement achievement) in LocalPlayer.MultiplayerStats.Achievements)
            {
                achievementsConverted.Add(new DateTimeOffset(timestamp.ToUniversalTime()).ToUnixTimeSeconds(), achievement.Id);
            }

            var payload = new
            {
                Uid = LocalPlayerUid,
                Achievements = achievementsConverted,
            };
            _ = Client.PostAsync("updatePlayer", payload);
        }

        /// <summary>
        /// Updates the list of hidden charts of the local <see cref="Player"/> and syncs with the server. PLEASE USE DISPATCHER.
        /// </summary>
        internal static void SyncHiddens()
        {
            if (!Client.Connected) return;

            LocalPlayer.MultiplayerStats.Hiddens.Clear();
            foreach (string entry in GlobalDataBase.dbMusicTag.Hide)
            {
                LocalPlayer.MultiplayerStats.Hiddens.Add(ChartManager.GetEntryKey(entry));
            }

            var payload = new
            {
                Uid = LocalPlayerUid,
                Hiddens = LocalPlayer.MultiplayerStats.Hiddens,
            };
            _ = Client.PostAsync("updatePlayer", payload);
        }

        /// <summary>
        /// Finds/creates a <see cref="Player"/> by their <paramref name="uid"/>.
        /// </summary>
        /// <param name="uid">UID of a <see cref="Player"/>.</param>
        /// <returns>A <see cref="Player"/> who was cached or a new instance.</returns>
        internal static async Task<Player> GetPlayer(string uid)
        {
            if (!Client.Connected) return null;

            Player player = GetCachedPlayer(uid);
            if (player != null) return player;
            
            // If not cached
            player = new(uid);
            CachedPlayers.Add(player.Uid, player);
            await player.Update(true);

            return player;
        }

        /// <summary>
        /// Finds a <see cref="Player"/> by their <paramref name="uid"/> in cache.
        /// </summary>
        /// <param name="uid">UID of a <see cref="Player"/>.</param>
        /// <returns>A cached <see cref="Player"/>.</returns>
        internal static Player GetCachedPlayer(string uid)
        {
            if (CachedPlayers.TryGetValue(uid, out Player player)) return player;
            return null;
        }

        /// <summary>
        /// Clears the cache of the <see cref="Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> which cache will be cleared.</param>
        private static void ClearPlayerFromCache(Player player)
        {
            if (!CachedPlayers.ContainsKey(player.Uid)) return;

            CachedPlayers.Remove(player.Uid);
        }

        private static async Task CacheCleaner()
        {
            DateTime current;
            while (true)
            {
                await Task.Delay(Constants.CacheCheckInterval);
                current = DateTime.Now;

                foreach (Player player in CachedPlayers.Values)
                {
                    if (current - player.LastUpdated >= Constants.PlayerCacheExpiration && player != LocalPlayer)
                    {
                        // FIX THIS SHIT
                        //ClearPlayerFromCache(player);
                    }
                }
            }
        }

        private static async Task InitLocalPlayer()
        {
            LocalPlayer = await GetPlayer(LocalPlayerUid);
            Main.Dispatcher.Enqueue(() =>
            {
                SyncHiddens();
                UIManager.MainMenu.Open();
            });
        }

        internal static async Task Init()
        {
            CachedPlayers = new();
            LocalPlayerName = DataHelper.nickname;
            LocalPlayerUid = DataHelper.PeroUid;
            _ = CacheCleaner();
            await InitLocalPlayer();
        }
    }
}
