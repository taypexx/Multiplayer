using Multiplayer.Data;

namespace Multiplayer.Managers
{
    internal static class LobbyManager
    {
        internal static List<Lobby> CachedLobbies { get; private set; }
        internal static List<Lobby> PublicLobbies 
        { 
            get 
            { 
                if (CachedLobbies == null) return null;

                field.Clear();
                foreach (Lobby lobby in CachedLobbies)
                {
                    if (lobby.IsPrivate) continue;
                    field.Add(lobby);
                }
                return field;
            } 
            private set; 
        }
        internal static Lobby LocalLobby { get; set; }

        internal static TimeSpan AutoUpdateInterval => Settings.Config.SlowNetworkMode ? TimeSpan.FromSeconds(8) : TimeSpan.FromSeconds(4);

        /// <summary>
        /// Finds a <see cref="Lobby"/> by their <paramref name="id"/>.
        /// </summary>
        /// <param name="id">ID of a <see cref="Lobby"/>.</param>
        /// <returns>A <see cref="Lobby"/> that was cached or a new instance.</returns>
        internal static async Task<Lobby> GetLobby(int id)
        {
            if (!Client.Connected) return null;

            foreach (Lobby lobby in CachedLobbies)
            {
                if (lobby.Id == id)
                {
                    return lobby;
                }
            }

            // If not cached
            Lobby newLobby = new(id);
            CacheLobby(newLobby);
            await newLobby.Update(false);

            return newLobby;
        }

        /// <summary>
        /// Sends a request to the server to join the lobby.
        /// </summary>
        /// <param name="lobby"></param>
        /// <returns></returns>
        internal static async Task<bool> JoinLobby(Lobby lobby, string passwordGuess = null)
        {
            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Token = Client.Token,
                Id = lobby.Id,
                PasswordGuess = passwordGuess
            };

            var response = await Client.PostAsync("joinLobby", payload);
            bool success = response != null;

            if (success) LocalLobby = lobby;
            return success;
        }

        internal static async Task<bool> LeaveLobby(Lobby lobby)
        {
            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Token = Client.Token
            };

            var response = await Client.PostAsync("leaveLobby", payload);
            bool success = response != null;

            if (success) LocalLobby = null;
            return success;
        }

        /// <summary>
        /// Caches the <see cref="Lobby"/>.
        /// </summary>
        /// <param name="lobby"><see cref="Lobby"/> to cache.</param>
        /// <returns><see langword="true"/> if <see cref="Lobby"/> was successfully cached or <see langword="false"/> if it was cached already.</returns>
        private static bool CacheLobby(Lobby lobby)
        {
            if (CachedLobbies.Contains(lobby)) return false;

            CachedLobbies.Add(lobby);
            return true;
        }

        internal static void Init()
        {
            CachedLobbies = new();
            PublicLobbies = new();
        }
    }
}
