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
        /// <param name="uid">UID of a player (Pero UID).</param>
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

        /// <summary>
        /// Syncronizes the local <see cref="Player"/> with the server.
        /// </summary>
        /// <returns><see langword="true"/> if the synchronization was successfull, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> SyncLocalPlayer()
        {
            if (LocalPlayer == null) return false;

            var content = await Client.PostAsync("syncLocal",LocalPlayer);

            return content != null;
        }

        internal static void Init()
        {
            var localUid = DataHelper.PeroUid;
            var localName = DataHelper.nickname;

            if (localUid == null || localName == null || localUid == string.Empty || localName == string.Empty)
            {
                //UIManager.WarnNotification(Localization.Get("Warning","NoAccount"));
                return;
            }

            CachedPlayers = new();
            LocalPlayer = GetPlayer(localUid);
        }
    }
}
