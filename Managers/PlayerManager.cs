using Il2CppAssets.Scripts.Database;
using Multiplayer.Data;
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
        /// Synchronizes local <see cref="Player"/>'s stats with the server. Should be called when field(s) need(s) to be updated.
        /// </summary>
        internal static void SyncLocalPlayer()
        {
            if (!Client.Connected) return;

            var achievementsConverted = new Dictionary<long, byte>();
            foreach ((DateTime timestamp, Achievement achievement) in LocalPlayer.MultiplayerStats.Achievements)
            {
                achievementsConverted.Add(new DateTimeOffset(timestamp.ToUniversalTime()).ToUnixTimeSeconds(), achievement.Id);
            }

            var payload = new
            {
                Uid = LocalPlayer.Uid,
                Name = LocalPlayer.MultiplayerStats.Name,
                AvatarName = LocalPlayer.MultiplayerStats.AvatarName,
                Bio = LocalPlayer.MultiplayerStats.Bio,
                Level = LocalPlayer.MultiplayerStats.Level,
                Achievements = achievementsConverted,
                Token = Client.Token,
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
            CachePlayer(player);
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
        /// Caches the <see cref="Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> to cache.</param>
        /// <returns><see langword="true"/> if <see cref="Player"/> was successfully cached or <see langword="false"/> if they were cached already.</returns>
        private static bool CachePlayer(Player player)
        {
            if (CachedPlayers.ContainsKey(player.Uid)) return false;

            CachedPlayers.Add(player.Uid, player);
            return true;
        }

        internal static void Init()
        {
            CachedPlayers = new();
            LocalPlayerName = DataHelper.nickname;
            LocalPlayerUid = DataHelper.PeroUid;
            Task.Run(async () => 
            {
                LocalPlayer = await GetPlayer(LocalPlayerUid);
                UIManager.MainMenu.Open();
            });
        }
    }
}
