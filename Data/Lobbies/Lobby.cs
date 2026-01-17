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

        public bool IsPrivate { get; private set; }
        public bool Locked { get; internal set; }
        public List<string> ReadyPlayers { get; private set; }
        public bool EveryoneReady => ReadyPlayers.Count == Players.Count;

        public Player Host { get; private set; }
        public List<string> Players { get; private set; }
        public ushort MaxPlayers { get; private set; }

        public ushort PlaylistSize { get; private set; }
        public bool IsPlaylistFull => Playlist.Count >= PlaylistSize;
        public List<PlaylistEntry> Playlist { get; private set; }
        public PlaylistEntry CurrentPlaylistEntry => Playlist?.First();

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
            Playlist = new();
        }

        /// <summary>
        /// Checks if the <see cref="Player"/> is in <see langword="this"/> <see cref="Lobby"/>.
        /// </summary>
        internal bool IsMember(Player player)
        {
            return Players.Contains(player.Uid);
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

        internal async Task UpdateFields(Dictionary<string, JsonElement> updatedData, bool updatePlayers = false, bool playersUpdated = false)
        {
            Name = updatedData["Name"].GetString();
            IsPrivate = updatedData["IsPrivate"].GetBoolean();
            Locked = updatedData["Locked"].GetBoolean();
            MaxPlayers = updatedData["MaxPlayers"].GetUInt16();
            Host = await PlayerManager.GetPlayer(updatedData["HostUid"].GetString());
            PlaylistSize = updatedData["PlaylistSize"].GetUInt16();

            PlayType = (LobbyPlayType)updatedData["PlayType"].GetByte();
            ChartSelection = (LobbyChartSelection)updatedData["ChartSelection"].GetByte();
            Goal = (LobbyGoal)updatedData["Goal"].GetByte();

            try // Loves to error
            {
                if (playersUpdated)
                {
                    var newPlayers = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(updatedData["Players"]);
                    Players = newPlayers.Keys.ToList();
                    foreach ((var playerUid, var playerStats) in newPlayers)
                    {
                        (await PlayerManager.GetPlayer(playerUid)).MultiplayerStats.UpdateFields(playerStats);
                    }
                } 
                else Players = JsonSerializer.Deserialize<List<string>>(updatedData["Players"]);
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
                    ReadyPlayers = JsonSerializer.Deserialize<List<string>>(updatedData["ReadyPlayers"]);
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

                    Main.Dispatcher.Enqueue(() => UIManager.LobbyPlaylistWindow.Update(this));
                }
                catch { }
            }

            LastUpdated = DateTime.Now;
        }

        /// <summary>
        /// Gets the <see cref="Lobby"/> data from the server and updates itself.
        /// </summary>
        /// <param name="updatePlayers">Whether to updates players of the lobby.</param>
        /// <param name="body">(Optional) The recieved lobby data body from somewhere else.</param>
        internal async Task Update(bool updatePlayers = false)
        {
            var response = await Client.PostAsync("getLobby", new
            {
                Client.Token,
                PlayerManager.LocalPlayer.Uid,
                Id
            });

            // If the lobby was disbanded
            if (response == null)
            {
                // Leave if the local player was in this lobby
                if (IsMember(PlayerManager.LocalPlayer))
                {
                    UIManager.Debounce = true;
                    await LobbyManager.LeaveLobby(true);
                    UIManager.Debounce = false;
                }
                LobbyManager.ClearLobbyFromCache(this);
                return;
            }

            await UpdateFields(await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>());
        }
    }
}
