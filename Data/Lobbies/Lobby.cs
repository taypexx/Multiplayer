using Il2CppAssets.Scripts.Database;
using LocalizeLib;
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
        public LocalString NameLocal { get; private set; }

        public bool IsPrivate { get; private set; }
        public LobbyPlayType PlayType { get; private set; }
        public LobbyChartSelection ChartSelection { get; private set; }
        public LobbyGoal Goal { get; private set; }

        public bool Locked { get; internal set; }
        public bool EveryoneReady { get; private set; }
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
            NameLocal = new(Name);

            IsPrivate = true;
            PlayType = LobbyPlayType.All;
            ChartSelection = LobbyChartSelection.HostPlaylist;
            Goal = LobbyGoal.Accuracy;

            Locked = false;
            EveryoneReady = false;
            Host = null;
            Players = new();
            MaxPlayers = 2;

            PlaylistSize = 5;
            Playlist = new();
        }

        internal bool IsMember(Player player)
        {
            return Players.Contains(player.Uid);
        }

        internal bool HasInPlaylist(string entry)
        {
            foreach (var playlistEntry in Playlist)
            {
                if (playlistEntry.Entry == entry) return true;
            }
            return false;
        }

        internal PlaylistEntry GetFromPlaylist(string entry)
        {
            foreach (var playlistEntry in Playlist)
            {
                if (playlistEntry.Entry == entry) return playlistEntry;
            }
            return null;
        }

        /// <summary>
        /// Synchronizes the <see cref="Lobby"/> with the server.
        /// </summary>
        /// <param name="updatePlayers">Whether to updates players of the lobby.</param>
        /// <returns><see langword="true"/> if update was successful, otherwise <see langword="false"/>.</returns>
        internal async Task<bool> Update(bool updatePlayers = false)
        {
            var payload = new
            {
                Client.Token,
                PlayerManager.LocalPlayer.Uid,
                Id
            };

            var response = await Client.PostAsync("getLobby", payload);
            if (response == null)
            {
                // Lobby was disbanded
                if (Host == PlayerManager.LocalPlayer)
                {
                    UIManager.Debounce = true;
                    await LobbyManager.LeaveLobby(true);
                    UIManager.Debounce = false;
                }
                LobbyManager.ClearLobbyFromCache(this);

                return false;
            }

            var updatedData = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

            Name = updatedData["Name"].GetString();
            NameLocal = new(Name);

            IsPrivate = updatedData["IsPrivate"].GetBoolean();
            PlayType = (LobbyPlayType)updatedData["PlayType"].GetByte();
            ChartSelection = (LobbyChartSelection)updatedData["ChartSelection"].GetByte();
            Goal = (LobbyGoal)updatedData["Goal"].GetByte();

            Locked = updatedData["Locked"].GetBoolean();
            Host = await PlayerManager.GetPlayer(updatedData["HostUid"].GetString());
            MaxPlayers = updatedData["MaxPlayers"].GetUInt16();

            try
            {
                Players = JsonSerializer.Deserialize<List<string>>(updatedData["Players"].GetRawText());
            }
            catch { }

            foreach (string playerUid in Players)
            {
                Player player = PlayerManager.GetCachedPlayer(playerUid);
                if (player is null)
                {
                    player = await PlayerManager.GetPlayer(playerUid);
                }
                else if (updatePlayers) await player.Update();
            }

            if (this == LobbyManager.LocalLobby)
            {
                try
                {
                    EveryoneReady = JsonSerializer.Deserialize<List<string>>(updatedData["ReadyPlayers"].GetRawText()).Count == Players.Count;
                }
                catch { }

                try
                {
                    var playlist = JsonSerializer.Deserialize<List<string>>(updatedData["Playlist"].GetRawText());

                    // Add the new entries from other players
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

                    // Remove the entries that were removed by other players
                    foreach (var playlistEntry in Playlist)
                    {
                        if (playlist.Contains(playlistEntry.Entry)) continue;

                        Playlist.Remove(playlistEntry);
                    }

                    Main.Dispatcher.Enqueue(() => UIManager.LobbyPlaylistWindow.Update(this));
                }
                catch { }
            }

            PlaylistSize = updatedData["PlaylistSize"].GetUInt16();
            LastUpdated = DateTime.Now;
            return true;
        }
    }
}
