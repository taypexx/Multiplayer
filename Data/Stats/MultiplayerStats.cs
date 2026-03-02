using Il2CppAssets.Scripts.Database;
using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using System.Net.Http.Json;
using System.Text.Json;
using System.Reflection;

namespace Multiplayer.Data.Stats
{
    public enum PlayerStatus : byte
    {
        Offline, Online, InLobby, InBattle
    }

    public class MultiplayerStats
    {
        public Player Player { get; private set; }

        public PlayerStatus Status {
            get => Player == PlayerManager.LocalPlayer ? (LobbyManager.IsInLobby ? (Main.IsUIScene ? PlayerStatus.InLobby : PlayerStatus.InBattle) : PlayerStatus.Online) : field;
            private set; 
        }

        public string Name { get; private set; }
        public string Bio { get; internal set; }

        public string AvatarName {
            get => Player == PlayerManager.LocalPlayer ? "head_" + DataHelper.selectedHeadIndex.ToString() : field;
            private set; 
        }

        public int Level
        {
            get => Player == PlayerManager.LocalPlayer ? DataHelper.Level : field;
            private set;
        }

        public ushort ELO { get; private set; }
        public bool Banned { get; private set; }
        public string Rank => Ranks.GetRank(ELO);

        public bool FriendsCached { get; private set; }
        public bool FriendRequestsCached { get; private set; }
        public HashSet<string> Friends { get; private set; }
        public HashSet<string> FriendRequests { get; private set; }
        public Dictionary<DateTime, Achievement> Achievements { get; internal set; }

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

        public MultiplayerStats(Player player)
        {
            Player = player;
            Status = PlayerStatus.Offline;

            Name = "Player" + player.Uid;
            Bio = "This user does not have anything interesting to say.";

            AvatarName = "head_0";
            Level = 1;
            ELO = 1500;
            Banned = false;

            FriendsCached = false;
            FriendRequestsCached = false;
            Friends = new();
            FriendRequests = new();
            Achievements = new();

            GirlIndex = 0;
            ElfinIndex = -1;

            FavGirlIndex = -1;
            FavElfinIndex = -2;
        }

        /// <summary>
        /// Updates fields of the <see cref="MultiplayerStats"/> by the given <paramref name="updatedData"/> JSON dictionary.
        /// </summary>
        /// <param name="updatedData">JSON dictionary containing fields as keys and their values.</param>
        internal void UpdateFields(Dictionary<string, JsonElement> updatedData)
        {
            Status = (PlayerStatus)updatedData["Status"].GetByte();
            Name = updatedData["Name"].GetString();
            Bio = updatedData["Bio"].GetString();
            AvatarName = updatedData["AvatarName"].GetString();
            Level = updatedData["Level"].GetInt32();
            ELO = updatedData["ELO"].GetUInt16();
            Banned = updatedData["Banned"].GetBoolean();

            try
            {
                Friends = JsonSerializer.Deserialize<HashSet<string>>(updatedData["Friends"]);
            }
            catch { }

            FriendRequests.Clear();
            try
            {
                FriendRequests = JsonSerializer.Deserialize<HashSet<string>>(updatedData["FriendRequests"]);
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
                Uid = PlayerManager.LocalPlayerUid,
                TargetUid = Player.Uid,
                Name = PlayerManager.LocalPlayerName
            });
            if (response is null) return;

            UpdateFields(await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>());
        }

        /// <summary>
        /// Caches every <see cref="Players.Player"/> from friends.
        /// </summary>
        internal async Task CacheFriends()
        {
            var response = await Client.PostAsync("getFriends", new
            {
                Uid = PlayerManager.LocalPlayerUid,
                TargetUid = Player.Uid
            });
            if (response is null) return;

            try
            {
                var friends = await response.Content.ReadFromJsonAsync<List<Dictionary<string, JsonElement>>>();
                foreach (var friendData in friends)
                {
                    var friendUid = friendData["Uid"].GetString();
                    var friend = PlayerManager.GetCachedPlayer(friendUid);
                    if (friend is null)
                    {
                        friend = await PlayerManager.CreatePlayer(friendUid, friendData);
                    }
                    else friend.MultiplayerStats.UpdateFields(friendData);
                }

                FriendsCached = true;
            }
            catch { }
        }

        /// <summary>
        /// Caches every <see cref="Players.Player"/> from friend requests.
        /// </summary>
        internal async Task CacheFriendRequests()
        {
            var response = await Client.PostAsync("getFriendRequests", new
            {
                Uid = PlayerManager.LocalPlayerUid
            });
            if (response is null) return;

            var friendRequests = await response.Content.ReadFromJsonAsync<List<Dictionary<string, JsonElement>>>();
            foreach (var otherData in friendRequests)
            {
                var otherUid = otherData["Uid"].GetString();
                var otherPlayer = PlayerManager.GetCachedPlayer(otherUid);
                if (otherPlayer is null)
                {
                    otherPlayer = await PlayerManager.CreatePlayer(otherUid, otherData);
                }
                else otherPlayer.MultiplayerStats.UpdateFields(otherData);
            }

            FriendRequestsCached = true;
        }
    }
}
