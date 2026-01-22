using Il2CppAssets.Scripts.Database;
using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using System.Net.Http.Json;
using System.Text.Json;
using System.Reflection;

namespace Multiplayer.Data.Stats
{
    public class MultiplayerStats
    {
        public Player Player { get; private set; }
        public string Name { get; private set; }
        public string AvatarName { get; internal set; }
        public string Bio { get; internal set; }
        public int Level { 
            get => Player == PlayerManager.LocalPlayer ? DataHelper.Level : field;
            private set; 
        }

        public int GirlIndex { 
            get => Player == PlayerManager.LocalPlayer ? DataHelper.selectedRoleIndex : field;
            private set; 
        }
        public int ElfinIndex { 
            get => Player == PlayerManager.LocalPlayer ? DataHelper.selectedElfinIndex : field;
            private set; 
        }

        public int FavGirlIndex { 
            get
            {
                if (Player != PlayerManager.LocalPlayer) return field;

                var favGirl = Main.GetDependency("FavGirl");
                if (favGirl is null) return GirlIndex;

                return (int)favGirl.GetType("FavGirl.FavSave").GetProperty("FavGirl", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            }
            private set; 
        }
        public int FavElfinIndex {
            get
            {
                if (Player != PlayerManager.LocalPlayer) return field;

                var favGirl = Main.GetDependency("FavGirl");
                if (favGirl is null) return ElfinIndex;

                return (int)favGirl.GetType("FavGirl.FavSave").GetProperty("FavElfin", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            }
            private set;
        }

        public bool FriendsCached { get; private set; }
        public List<string> Friends { get; private set; }
        public Dictionary<string, string> FriendRequests { get; private set; }
        public Dictionary<DateTime, Achievement> Achievements { get; internal set; }
        public List<string> Hiddens { get; internal set; }

        public ushort ELO { get; private set; }
        public bool Banned { get; private set; }
        public string Rank => Ranks.GetRank(ELO);

        public MultiplayerStats(Player player)
        {
            Player = player;
            Name = PlayerManager.LocalPlayerName ?? player.Uid;
            AvatarName = "head_0";
            Bio = "This player did not set their bio.";
            Level = 1;

            GirlIndex = 0;
            ElfinIndex = -1;

            FavGirlIndex = -1;
            ElfinIndex = -2;

            FriendsCached = false;
            Friends = new();
            FriendRequests = new();
            Achievements = new();
            Hiddens = new();

            ELO = 1500;
            Banned = false;
        }

        /// <summary>
        /// Updates fields of the <see cref="MultiplayerStats"/> by the given <paramref name="updatedData"/> JSON dictionary.
        /// </summary>
        /// <param name="updatedData">JSON dictionary containing fields as keys and their values.</param>
        internal void UpdateFields(Dictionary<string, JsonElement> updatedData)
        {
            Name = updatedData["Name"].GetString();
            AvatarName = updatedData["AvatarName"].GetString();
            Bio = updatedData["Bio"].GetString();
            Level = updatedData["Level"].GetInt32();

            try
            {
                Friends = JsonSerializer.Deserialize<List<string>>(updatedData["Friends"]);
            }
            catch { }

            FriendRequests.Clear();
            try
            {
                FriendRequests = JsonSerializer.Deserialize<Dictionary<string, string>>(updatedData["FriendRequests"]);
            }
            catch { }

            Achievements.Clear();
            try
            {
                // Converting data to actual achievements with their timestamps 
                var updatedAchievements = JsonSerializer.Deserialize<Dictionary<long, byte>>(updatedData["Achievements"]);
                foreach ((long unixTimestamp, byte id) in updatedAchievements)
                {
                    Achievements.Add(DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime, AchievementManager.Achievements[id]);
                }
            }
            catch { }

            ELO = updatedData["ELO"].GetUInt16();
            Banned = updatedData["Banned"].GetBoolean();

            GirlIndex = updatedData["GirlIndex"].GetInt32();
            ElfinIndex = updatedData["ElfinIndex"].GetInt32();

            FavGirlIndex = updatedData["FavGirlIndex"].GetInt32();
            FavElfinIndex = updatedData["FavElfinIndex"].GetInt32();

            Player.PingMS = updatedData["PingMS"].GetUInt16();
        }

        /// <summary>
        /// Gets <see cref="MultiplayerStats"/> from the server and updates fields.
        /// </summary>
        internal async Task Update()
        {
            var response = await Client.PostAsync("getPlayer", new
            {
                Uid = PlayerManager.LocalPlayerUid ?? Player.Uid,
                TargetUid = Player.Uid,
                Name = PlayerManager.LocalPlayerName
            });
            if (response is null) return;

            UpdateFields(await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>());
        }

        /// <summary>
        /// Caches every <see cref="Player"/> from friends.
        /// </summary>
        internal async Task CacheFriends()
        {
            foreach (string friendUid in Friends)
            {
                await PlayerManager.GetPlayer(friendUid);
            }
            FriendsCached = true;
        }
    }
}
