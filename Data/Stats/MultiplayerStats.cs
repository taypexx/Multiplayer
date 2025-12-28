using LocalizeLib;
using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
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

        public List<Player> Friends { get; private set; }
        public Dictionary<string, string> FriendRequests { get; private set; }
        public Dictionary<DateTime, Achievement> Achievements { get; internal set; }
        public List<string> Hiddens { get; internal set; }

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
            Hiddens = new();

            ELO = 1500;
            Banned = false;
        }

        /// <summary>
        /// Gets the <see cref="MultiplayerStats"/> from the server.
        /// </summary>
        /// <returns><see langword="true"/> if update was successful, otherwise <see langword="false"/>.</returns>
        internal async Task<bool> Update()
        {
            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid ?? Player.Uid,
                TargetUid = Player.Uid,
                Name = PlayerManager.LocalPlayerName
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
            catch {}

            FriendRequests.Clear();
            try
            {
                FriendRequests = JsonSerializer.Deserialize<Dictionary<string, string>>(updatedData["FriendRequests"].GetRawText());
            }
            catch {}

            ELO = updatedData["ELO"].GetUInt16();
            Banned = updatedData["Banned"].GetBoolean();

            return true;
        }

        private string GetRank(bool includingSubrank = false)
        {
            for (int i = 0; i < Players.Rank.RanksList.Count; i++)
            {
                Rank rank = Players.Rank.RanksList[i];
                if (ELO >= rank.ELO)
                {
                    if (includingSubrank && rank.SubRanks > 0)
                    {
                        return $"{rank.Name} {Players.Rank.SubdivisionSuffixes[(int)Math.Floor((decimal)((ELO - rank.ELO) / Players.Rank.SubdivisionGap))]}";
                    }
                    else
                    {
                        return rank.Name.ToString();
                    }
                }
            }
            return Players.Rank.RanksList.Last().Name.ToString();
        }
    }
}
