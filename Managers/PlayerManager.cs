using Il2CppAssets.Scripts.Database;
using Multiplayer.Data.Players;
using Multiplayer.Static;

namespace Multiplayer.Managers
{
    internal static class PlayerManager
    {
        private static Dictionary<string, Player> CachedPlayers;

        internal static Player LocalPlayer { get; private set; }
        internal static string LocalPlayerUid { get; private set; }
        internal static string LocalPlayerName { get; private set; }

        internal static HashSet<string> LocalPlayerHiddens;

        internal static Comparison<object> AccuracyComparison = (p1, p2) => 
        {
            if (p1 is not Player || p2 is not Player) return p1 is not Player ? 1 : -1;
            var battlestats1 = ((Player)p1).BattleStats;
            var battlestats2 = ((Player)p2).BattleStats;

            if (!battlestats1.Alive || !battlestats2.Alive)
            {
                return battlestats1.Alive ? 1 : -1;
            }
            else if (battlestats1.Accuracy == battlestats2.Accuracy)
            {
                return ((battlestats1.Earlies + battlestats1.Lates) * -1).CompareTo((battlestats2.Earlies + battlestats2.Lates) * -1);
            }
            else return battlestats1.Accuracy.CompareTo(battlestats2.Accuracy);
        };

        internal static Comparison<object> ScoreComparison = (p1, p2) =>
        {
            if (p1 is not Player || p2 is not Player) return p1 is not Player ? 1 : -1;

            var battlestats1 = ((Player)p1).BattleStats;
            var battlestats2 = ((Player)p2).BattleStats;

            if (!battlestats1.Alive || !battlestats2.Alive)
            {
                return battlestats1.Alive ? 1 : -1;
            }
            else return battlestats1.Score.CompareTo(battlestats2.Score);
        };

        internal static Comparison<object> CustomComparison = (p1, p2) =>
        {
            if (p1 is not Player || p2 is not Player) return p1 is not Player ? 1 : -1;

            var battlestats1 = ((Player)p1).BattleStats;
            var battlestats2 = ((Player)p2).BattleStats;

            if (!battlestats1.Alive || !battlestats2.Alive)
            {
                return battlestats1.Alive ? 1 : -1;
            }
            else return 0; // hmm, what could it be
        };

        /// <summary>
        /// Synchronizes profile stats of the local <see cref="Player"/>.
        /// </summary>
        internal static void SyncProfile()
        {
            if (!Client.Connected) return;

            var localStats = LocalPlayer.MultiplayerStats;
            var payload = new
            {
                Uid = LocalPlayerUid,
                Status = localStats.Status,
                Name = localStats.Name,
                AvatarName = localStats.AvatarName,
                Bio = localStats.Bio,
                Level = localStats.Level,
                GirlIndex = localStats.GirlIndex,
                ElfinIndex = localStats.ElfinIndex,
                FavGirlIndex = localStats.FavGirlIndex,
                FavElfinIndex = localStats.FavElfinIndex
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
                Achievements = achievementsConverted
            };
            _ = Client.PostAsync("updatePlayer", payload);
        }

        /// <summary>
        /// Updates the list of hidden charts of the local <see cref="Player"/> and syncs with the server. PLEASE USE DISPATCHER.
        /// </summary>
        internal static void SyncHiddens()
        {
            if (!Client.Connected) return;

            LocalPlayerHiddens.Clear();
            foreach (string hiddenUid in GlobalDataBase.dbMusicTag.Hide)
            {
                LocalPlayerHiddens.Add(ChartManager.GetEntryKey(hiddenUid));
            }

            var payload = new
            {
                Uid = LocalPlayerUid,
                Hiddens = LocalPlayerHiddens
            };
            _ = Client.PostAsync("updatePlayer", payload);
        }

        /// <summary>
        /// Syncs the current list of loaded custom charts of the local <see cref="Player"/>.
        /// </summary>
        internal static void SyncCustoms()
        {
            if (!Client.Connected) return;

            var payload = new
            {
                Uid = LocalPlayerUid,
                Customs = ChartManager.CustomCharts.Keys
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
            if (player == null || !CachedPlayers.ContainsKey(player.Uid)) return;

            CachedPlayers.Remove(player.Uid);
        }

        /// <summary>
        /// Starts cleaning players from cache over time.
        /// </summary>
        private static async Task CacheCleaner()
        {
            DateTime current;
            while (Client.Connected)
            {
                await Task.Delay(Constants.CacheCheckIntervalMS);
                current = DateTime.Now;

                foreach (Player player in CachedPlayers.Values)
                {
                    if (current - player.LastUpdated >= Constants.PlayerCacheExpiration && player != LocalPlayer)
                    {
                        // We don't need to clear our mates
                        if (LobbyManager.IsInLobby && LobbyManager.LocalLobby.IsMember(player)) continue;

                        // Uncomment whenever you feel like it won't break anything
                        //ClearPlayerFromCache(player);
                    }
                }
            }
        }

        internal static async Task Init()
        {
            CachedPlayers = new();
            LocalPlayerName = DataHelper.nickname;
            LocalPlayerUid = DataHelper.PeroUid;
            LocalPlayerHiddens = new();
            LocalPlayer = await GetPlayer(LocalPlayerUid);

            SyncHiddens();
            SyncCustoms();
            _ = CacheCleaner();
        }
    }
}
