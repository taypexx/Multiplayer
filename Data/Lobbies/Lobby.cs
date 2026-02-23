using Il2CppAssets.Scripts.Database;
using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Data.Lobbies
{
    public class Lobby
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public LobbyPlayType PlayType { get; private set; }
        public LobbyChartSelection ChartSelection { get; private set; }
        public LobbyGoal Goal { get; private set; }

        public Comparison<object> GoalComparison => Goal switch
        {
            LobbyGoal.Accuracy => PlayerManager.AccuracyComparison,
            LobbyGoal.Score => PlayerManager.ScoreComparison,
            LobbyGoal.Custom => PlayerManager.CustomComparison,
            _ => PlayerManager.AccuracyComparison
        };

        public bool IsPrivate { get; private set; }
        public bool Locked { get; internal set; }
        public HashSet<string> ReadyPlayers { get; private set; }
        public bool EveryoneReady => ReadyPlayers.Count == Players.Count;
        public bool EveryoneFinished => ReadyPlayers.Count == 0;

        public Player Host { get; private set; }
        public HashSet<string> Players { get; private set; }
        public ushort MaxPlayers { get; private set; }

        public List<PlaylistEntry> Playlist { get; private set; }
        public ushort PlaylistSize { get; private set; }
        public bool IsPlaylistFull => Playlist.Count >= PlaylistSize;

        private ushort CurrentGlobalPlaylistEntryIndex { get; set; }
        public ushort CurrentPlaylistEntryIndex { get; private set; }
        public PlaylistEntry? CurrentPlaylistEntry => 
            Playlist is null 
            ? null 
            : Playlist.Count > 0
                ? Playlist[CurrentPlaylistEntryIndex]
                : null;

        internal DateTime LastUpdated { get; private set; }

        internal Lobby(int id)
        {
            Id = id;
            Name = "Lobby";

            IsPrivate = true;
            PlayType = LobbyPlayType.All;
            ChartSelection = LobbyChartSelection.HostPlaylist;
            Goal = LobbyGoal.Accuracy;

            Locked = false;
            ReadyPlayers = new();
            Host = null;
            Players = new();
            MaxPlayers = 2;

            PlaylistSize = 5;
            CurrentPlaylistEntryIndex = 0;
            Playlist = new();
        }

        /// <summary>
        /// Checks if the <see cref="Player"/> is in <see langword="this"/> <see cref="Lobby"/>.
        /// </summary>
        internal bool IsMember(Player player)
        {
            return Players.Contains(player.Uid);
        }

        internal List<Player> GetPlayerList()
        {
            var list = new List<Player>();
            foreach (var playerUid in Players)
            {
                var player = PlayerManager.GetCachedPlayer(playerUid);
                if (player == null) continue;
                list.Add(player);
            }
            return list;
        }

        internal string GetBattleInfo(Player player)
        {
            var battleInfo = string.Empty;
            var battleStats = player.BattleStats;

            if (battleStats.Alive)
            {
                switch (Goal)
                {
                    case LobbyGoal.Accuracy:

                        if (battleStats.TrueAP)
                        {
                            battleInfo = $"<color=#{Constants.Red}>TP</color>";
                        }
                        else if (battleStats.AP)
                        {
                            battleInfo = $"<color=#{Constants.Yellow}>AP</color>";
                            if (battleStats.Earlies > 0)
                            {
                                battleInfo += $" <color=#{Constants.Blue}>{battleStats.Earlies}E</color>";
                            }
                            if (battleStats.Lates > 0)
                            {
                                battleInfo += $" <color=#{Constants.Red}>{battleStats.Lates}L</color>";
                            }
                        }
                        else if (battleStats.FC)
                        {
                            battleInfo = $"<color=#{Constants.Blue}>FC</color> {battleStats.Accuracy}%  {battleStats.Greats}G";
                        }
                        else
                        {
                            battleInfo = $"{battleStats.Accuracy}%  {battleStats.Misses}M";
                            if (battleStats.Greats > 0) battleInfo += $" {battleStats.Greats}G";
                        }

                        break;
                    case LobbyGoal.Score:
                        battleInfo = battleStats.Score.ToString();//$"<color=#{Constants.Yellow}>{battleStats.Score}</color>";
                        break;
                    case LobbyGoal.Custom:
                        break;
                }
            }
            else battleInfo = $"<color=#{Constants.Red}>Down</color>";

            return battleInfo;
        }

        /// <summary>
        /// Syncs the current playlist index with the server one.
        /// </summary>
        internal void SyncPlaylistEntry()
        {
            CurrentPlaylistEntryIndex = CurrentGlobalPlaylistEntryIndex;
        }

        /// <summary>
        /// Checks if a <see cref="PlaylistEntry"/> with the provided <paramref name="entry"/> is in the playlist.
        /// </summary>
        internal bool HasInPlaylist(string entry)
        {
            foreach (var playlistEntry in Playlist)
            {
                if (playlistEntry.Entry == entry) return true;
            }
            return false;
        }

        /// <returns>A <see cref="PlaylistEntry"/> which has the given <paramref name="entry"/>.</returns>
        internal PlaylistEntry GetFromPlaylist(string entry)
        {
            foreach (var playlistEntry in Playlist)
            {
                if (playlistEntry.Entry == entry) return playlistEntry;
            }
            return null;
        }

        /// <summary>
        /// Updates fields of the <see cref="Lobby"/> by the given <paramref name="updatedData"/> JSON dictionary.
        /// </summary>
        /// <param name="updatedData">JSON dictionary containing fields as keys and their values.</param>
        /// <param name="updatePlayers">Whether to updates players of the <see cref="Lobby"/>.</param>
        /// <param name="playersUpdated">Treats <paramref name="updatedData"/>["Players"] as a <see cref="Dictionary{TKey,TValue}"/> if <see langword="true"/>, or as <see cref="List{T}"/> if <see langword="false"/>.</param>
        internal async Task UpdateFields(Dictionary<string, JsonElement> updatedData, bool updatePlayers = false, bool playersUpdated = false)
        {
            Name = updatedData["Name"].GetString();
            IsPrivate = updatedData["IsPrivate"].GetBoolean();
            Locked = updatedData["Locked"].GetBoolean();
            MaxPlayers = updatedData["MaxPlayers"].GetUInt16();
            Host = await PlayerManager.GetPlayer(updatedData["HostUid"].GetString());
            PlaylistSize = updatedData["PlaylistSize"].GetUInt16();
            CurrentGlobalPlaylistEntryIndex = updatedData["CurrentPlaylistEntry"].GetUInt16();

            PlayType = (LobbyPlayType)updatedData["PlayType"].GetByte();
            ChartSelection = (LobbyChartSelection)updatedData["ChartSelection"].GetByte();
            Goal = (LobbyGoal)updatedData["Goal"].GetByte();

            try // Loves to error
            {
                if (playersUpdated)
                {
                    var newPlayers = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(updatedData["Players"]);
                    Players = newPlayers.Keys.ToHashSet();
                    foreach ((var playerUid, var playerStats) in newPlayers)
                    {
                        (await PlayerManager.GetPlayer(playerUid)).MultiplayerStats.UpdateFields(playerStats);
                    }
                } 
                else Players = JsonSerializer.Deserialize<HashSet<string>>(updatedData["Players"]);
            }
            catch { }

            // Cache new players, update current ones if needed
            foreach (string playerUid in Players)
            {
                Player player = PlayerManager.GetCachedPlayer(playerUid);
                if (player is null)
                {
                    await PlayerManager.GetPlayer(playerUid);
                }
                else if (updatePlayers) await player.Update();
            }

            if (this == LobbyManager.LocalLobby)
            {
                try
                {
                    ReadyPlayers = JsonSerializer.Deserialize<HashSet<string>>(updatedData["ReadyPlayers"]);
                }
                catch { }
                try
                {
                    var playlist = JsonSerializer.Deserialize<List<string>>(updatedData["Playlist"]);

                    // Add new entries from other players
                    foreach (string entry in playlist)
                    {
                        string[] str = entry.Split("#");
                        MusicInfo musicInfo = ChartManager.GetMusicInfo(str[0]);
                        int difficulty = int.Parse(str[1]);

                        if (HasInPlaylist(entry)) continue;

                        PlaylistEntry playlistEntry = new(musicInfo, difficulty, entry);
                        if (playlistEntry is null) continue;
                        Playlist.Add(playlistEntry);
                    }

                    // Remove entries that were removed by other players
                    foreach (var playlistEntry in Playlist)
                    {
                        if (playlist.Contains(playlistEntry.Entry)) continue;

                        Playlist.Remove(playlistEntry);
                    }
                }
                catch { }
            }

            LastUpdated = DateTime.Now;
        }

        /// <summary>
        /// Gets the <see cref="Lobby"/> data from the server and updates fields.
        /// </summary>
        /// <param name="updatePlayers">Whether to updates players of the <see cref="Lobby"/>.</param>
        internal async Task Update(bool updatePlayers = false)
        {
            var response = await Client.PostAsync("getLobby", new
            {
                Client.Token,
                PlayerManager.LocalPlayer.Uid,
                Id
            });

            // If the lobby was disbanded
            if (response == null) return;

            await UpdateFields(await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>());
        }
    }
}
