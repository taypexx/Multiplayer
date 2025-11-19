using LocalizeLib;
using Multiplayer.Data.LobbyEnums;
using Multiplayer.Managers;
using Multiplayer.Static;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Data
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

        internal Lobby(int id)
        {
            Id = id;
            Name = "Lobby";
            NameLocal = new(Name);

            IsPrivate = true;
            PlayType = LobbyPlayType.All;
            ChartSelection = LobbyChartSelection.HostOnly;
            Goal = LobbyGoal.Accuracy;

            Locked = false;
            EveryoneReady = false;
            Host = null;
            Players = new();
            MaxPlayers = 2;
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
                Token = Client.Token,
                Uid = PlayerManager.LocalPlayer.Uid,
                Id = Id
            };

            var response = await Client.PostAsync("getLobby", payload);
            if (response == null) 
            {
                // Lobby was disbanded
                if (this == LobbyManager.LocalLobby)
                {
                    UIManager.Debounce = true;
                    await LobbyManager.LeaveLobby(true);
                    UIManager.Debounce = false;
                }
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
                EveryoneReady = JsonSerializer.Deserialize<List<string>>(updatedData["ReadyPlayers"].GetRawText()).Count == Players.Count;
            }
            catch (Exception e)
            {
                //Main.Logger.Warning(e.ToString());
            }

            foreach (string playerUid in Players)
            {
                Player player = PlayerManager.GetCachedPlayer(playerUid);
                if (player is null)
                {
                    player = await PlayerManager.GetPlayer(playerUid);
                } else if (updatePlayers) await player.Update();
            }

            return true;
        }

        internal bool IsMember(Player player)
        {
            return Players.Contains(player.Uid);
        }
    }
}
