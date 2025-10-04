using Il2CppAssets.Scripts.Database;
using Multiplayer.Data;
using Il2CppSystem;

namespace Multiplayer.Managers
{
    internal static class PlayerManager
    {
        internal static List<Player> CachedPlayers;
        internal static Player LocalPlayer;

        /// <summary>
        /// Finds a cached <see cref="Player"/> by their <paramref name="uid"/>.
        /// </summary>
        /// <param name="uid">UID of a player (Pero UID).</param>
        /// <returns>A <see cref="Player"/> or <see langword="null"/> if not cached.</returns>
        internal static Player GetPlayer(string uid)
        {
            foreach (Player player in CachedPlayers) 
            {
                if (player.Uid == uid)
                {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// Caches the <see cref="Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> to cache.</param>
        /// <returns><see langword="true"/> if the new <see cref="Player"/> instance was created or <see langword="false"/> if it was cached already.</returns>
        internal static bool CachePlayer(Player player)
        {
            if (GetPlayer(player.Uid) != null) { return false; }

            CachedPlayers.Add(player);
            return true;
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
            LocalPlayer = new(localUid,localName);
            CachePlayer(LocalPlayer);
        }
    }
}
