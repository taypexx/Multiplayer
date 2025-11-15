using Il2CppAssets.Scripts.Database;
using LocalizeLib;
using Multiplayer.Managers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Data.Stats
{
    public class MultiplayerStats
    {
        public Player Player { get; private set; }
        public string Name { get; private set; }
        public LocalString NameLocal { get; private set; }
        public string AvatarName { get; internal set; }
        public string Bio { get; internal set; }
        public int Level { get; private set; }

        public List<Player> Friends { get; internal set; }
        public Dictionary<string, string> FriendRequests { get; private set; }
        public Dictionary<DateTime, Achievement> Achievements { get; internal set; }

        public ushort ELO { get; private set; }
        public bool Banned { get; private set; }
        public string Rank => GetRank(true);

        public MultiplayerStats(Player player)
        {
            Player = player;
            Name = PlayerManager.LocalPlayerName ?? player.Uid;
            NameLocal = new(Name);
            AvatarName = "head_0";
            Bio = "This player did not set their bio.";
            Level = 1;

            Friends = new();
            FriendRequests = new();
            Achievements = new();

            ELO = 1500;
            Banned = false;
        }

        /// <summary>
        /// Synchronizes <see cref="MultiplayerStats"/> with the server.
        /// </summary>
        /// <returns><see langword="true"/> if update was successful, otherwise <see langword="false"/>.</returns>
        internal async Task<bool> Update()
        {
            var payload = new
            {
                SelfUid = PlayerManager.LocalPlayerUid ?? Player.Uid,
                TargetUid = Player.Uid,
                Name = PlayerManager.LocalPlayerName,
                Token = Client.Token
            };

            var response = await Client.PostAsync("getPlayer",payload);
            if (response == null) return false;

            var updatedData = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

            Name = updatedData["Name"].GetString();
            NameLocal = new(Name);
            AvatarName = updatedData["AvatarName"].GetString();
            Bio = updatedData["Bio"].GetString();
            Level = Player == PlayerManager.LocalPlayer ? PlayerManager.LocalPlayerLVL : updatedData["Level"].GetInt32();

            Friends.Clear();
            var updatedFriends = JsonSerializer.Deserialize<List<string>>(updatedData["Friends"].GetRawText());
            foreach (string friendUid in updatedFriends)
            {
                Friends.Add(PlayerManager.GetPlayer(friendUid).Result);
            }

            Achievements.Clear();
            try
            {
                var updatedAchievements = JsonSerializer.Deserialize<Dictionary<long, byte>>(updatedData["Achievements"].GetRawText());
                foreach ((long unixTimestamp, byte id) in updatedAchievements)
                {
                    Achievements.Add(DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime, AchievementManager.Achievements[id]);
                }
            }
            catch (Exception e)
            {
                //Main.Logger.Warning(e.ToString());
            }

            FriendRequests.Clear();
            try
            {
                FriendRequests = JsonSerializer.Deserialize<Dictionary<string, string>>(updatedData["FriendRequests"].GetRawText());
            }
            catch (Exception e)
            {
                //Main.Logger.Warning(e.ToString());
            }

            ELO = updatedData["ELO"].GetUInt16();
            Banned = updatedData["Banned"].GetBoolean();

            return true;
        }

        private string GetRank(bool includingSubrank = false)
        {
            for (int i = 0; i < Data.Rank.RanksList.Count; i++)
            {
                Rank rank = Data.Rank.RanksList[i];
                if (ELO >= rank.ELO)
                {
                    if (includingSubrank && rank.SubRanks > 0)
                    {
                        return $"{rank.Name} {Data.Rank.SubdivisionSuffixes[(int)Math.Floor((decimal)((ELO - rank.ELO) / Data.Rank.SubdivisionGap))]}";
                    }
                    else
                    {
                        return rank.Name;
                    }
                }
            }
            return Data.Rank.RanksList.Last().Name;
        }
    }
}
