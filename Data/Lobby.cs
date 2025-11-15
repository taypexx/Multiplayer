using LocalizeLib;
using Multiplayer.Managers;
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

        public Player Host { get; private set; }
        public List<string> Players { get; private set; }
        public ushort MaxPlayers { get; private set; }

        internal Lobby(int id)
        {
            Id = id;
            Name = "Lobby";
            NameLocal = new(Name);
            IsPrivate = true;
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
            if (response == null) return false;

            var updatedData = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

            Name = updatedData["Name"].GetString();
            NameLocal = new(Name);
            IsPrivate = updatedData["IsPrivate"].GetBoolean();
            MaxPlayers = updatedData["MaxPlayers"].GetUInt16();
            Host = await PlayerManager.GetPlayer(updatedData["HostUid"].GetString());

            try
            {
                Players = JsonSerializer.Deserialize<List<string>>(updatedData["Players"].GetRawText());
            }
            catch (Exception e)
            {
                //Main.Logger.Warning(e.ToString());
            }

            if (updatePlayers)
            {
                foreach (string playerUid in Players)
                {
                    await PlayerManager.GetPlayer(playerUid);
                }
            }

            return true;
        }

        internal bool IsMember(Player player)
        {
            return Players.Contains(player.Uid);
        }
    }
}
