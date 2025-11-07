using LocalizeLib;
using Multiplayer.Managers;
using System.Net.Http.Json;

namespace Multiplayer.Data.Stats
{
    public class MultiplayerStats
    {
        public Player Player { get; private set; }
        public string MDUid { get; private set; }
        public string Name { get; private set; }
        public LocalString NameLocal { get; private set; }
        public string AvatarName { get; internal set; }

        public List<Player> Friends { get; internal set; }
        public Dictionary<string, string> FriendRequests { get; private set; }
        public Dictionary<DateTime, Achievement> Achievements { get; internal set; }

        public ushort ELO { get; private set; }
        public bool Banned { get; private set; }
        public string Rank => GetRank(true);

        public MultiplayerStats(Player player, string mdUid = "", string name = "Player")
        {
            Player = player;
            MDUid = mdUid;
            Name = name;
            NameLocal = new(Name);
            AvatarName = "default";
            Friends = new();
            FriendRequests = new();
            Achievements = new();
            ELO = 1500;
            Banned = false;

            Update();
        }

        internal async void Update()
        {
            var response = await Client.GetAsync("player/" + Player.Uid);
            if (response == null) return;

            var updatedData = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            Name = (string)updatedData["Name"];
            NameLocal = new(Name);
            AvatarName = (string)updatedData["AvatarName"];

            Friends.Clear();
            foreach (string friendUid in (List<string>)updatedData["Friends"])
            {
                Friends.Add(PlayerManager.GetPlayer(friendUid));
            }

            Achievements.Clear();
            foreach ((long unixTimestamp, byte id) in (Dictionary<uint, byte>)updatedData["Achievements"])
            {
                Achievements.Add(DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime, AchievementManager.Achievements[id]);
            }

            FriendRequests.Clear();
            FriendRequests = (Dictionary<string, string>)updatedData["FriendRequests"];

            ELO = (ushort)updatedData["ELO"];
            Banned = (bool)updatedData["Banned"];
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
