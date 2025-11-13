using Il2CppAssets.Scripts.Database;
using Multiplayer.Data;

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
            if (!Client.Connected) return null;

            foreach (Player player in CachedPlayers) 
            {
                if (player.Uid == uid)
                {
                    return player;
                }
            }
            
            // If not cached
            Player newPlayer = new(uid);
            newPlayer.Update(true);
            CachePlayer(newPlayer);

            return newPlayer;
        }

        /// <summary>
        /// Synchronizes local <see cref="Player"/>'s stats with the server. Should be called when field(s) need(s) to be updated.
        /// </summary>
        internal static async void SyncLocalPlayer()
        {
            if (!Client.Connected) return;

            var achievementsConverted = new Dictionary<long, byte>();
            foreach ((DateTime timestamp, Achievement achievement) in LocalPlayer.MultiplayerStats.Achievements)
            {
                achievementsConverted.Add(new DateTimeOffset(timestamp.ToUniversalTime()).ToUnixTimeSeconds(),achievement.Id);
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
            var response = await Client.PostAsync("updatePlayer",payload);
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
            CachedPlayers = new();
            LocalPlayer = GetPlayer(DataHelper.PeroUid);
        }
    }
}
