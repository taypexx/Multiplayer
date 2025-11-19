using Multiplayer.Data;
using Multiplayer.Static;

namespace Multiplayer.Managers
{
    internal static class LobbyManager
    {
        internal static Dictionary<int,Lobby> CachedLobbies { get; private set; }
        internal static List<Lobby> PublicLobbies 
        { 
            get 
            { 
                if (CachedLobbies == null) return null;

                field.Clear();
                foreach (Lobby lobby in CachedLobbies.Values)
                {
                    if (lobby.IsPrivate) continue;
                    field.Add(lobby);
                }
                return field;
            } 
            private set; 
        }
        internal static Lobby LocalLobby { get; set; }

        internal static TimeSpan AutoUpdateInterval => Settings.Config.SlowNetworkMode ? TimeSpan.FromSeconds(6) : TimeSpan.FromSeconds(3);

        /// <summary>
        /// Sends a ready request to the server indicating that you are ready to play the chart (or not).
        /// </summary>
        /// <returns><see langword="true"/> if ready was set, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> SetReady(bool isReady)
        {
            if (!Client.Connected || LocalLobby is null) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Token = Client.Token,
                Ready = isReady
            };

            var response = await Client.PostAsync("lobbyReady", payload);
            bool success = response != null;

            return success;
        }

        /// <summary>
        /// Sends a lock request to lock the current lobby.
        /// </summary>
        /// <returns><see langword="true"/> if the state was changed, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> LockLobby(bool isLocked)
        {
            if (!Client.Connected || LocalLobby is null || LocalLobby.Host != PlayerManager.LocalPlayer) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Token = Client.Token,
                Locked = isLocked
            };

            var response = await Client.PostAsync("lobbyLock", payload);
            bool success = response != null;

            if (success) LocalLobby.Locked = isLocked;
            return success;
        }

        /// <summary>
        /// Sends a request to the server to join the <see cref="Lobby"/>.
        /// </summary>
        /// <param name="lobby"></param>
        /// <returns><see langword="true"/> if join was successful, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> JoinLobby(Lobby lobby, string passwordGuess = null)
        {
            if (!Client.Connected || LocalLobby != null || lobby is null || lobby.Locked) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Token = Client.Token,
                Id = lobby.Id,
                PasswordGuess = passwordGuess
            };

            var response = await Client.PostAsync("joinLobby", payload);
            bool success = response != null;

            if (success)
            {
                LocalLobby = lobby;
                Main.Dispatcher.Enqueue(() => 
                {
                    UIManager.MainLobbyDisplay.Create(lobby);
                    UIManager.UpdatePnlPreparation();
                });
            }
            return success;
        }

        /// <summary>
        /// Sends a request to the server to leave the <see cref="Lobby"/>.
        /// </summary>
        /// <param name="lobby"></param>
        /// <returns><see langword="true"/> if left successfully, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> LeaveLobby(bool leaveAnyway = false)
        {
            if (!Client.Connected || LocalLobby is null) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Token = Client.Token
            };

            var response = await Client.PostAsync("leaveLobby", payload);
            bool success = response != null;

            if (success || leaveAnyway)
            {
                LocalLobby = null;
                Main.Dispatcher.Enqueue(() => 
                {
                    UIManager.MainLobbyDisplay.Destroy();
                    UIManager.UpdatePnlPreparation();
                });
            }
            return success;
        }

        /// <summary>
        /// Finds a <see cref="Lobby"/> by their <paramref name="id"/>.
        /// </summary>
        /// <param name="id">ID of a <see cref="Lobby"/>.</param>
        /// <param name="checkIfExists">Additionally check if the <see cref="Lobby"/> even exists. Will return <see langword="null"/> if it doesn't.</param>
        /// <returns>A <see cref="Lobby"/> that was cached or a new instance.</returns>
        internal static async Task<Lobby> GetLobby(int id, bool checkIfExists = false)
        {
            if (!Client.Connected) return null;

            Lobby lobby = GetCachedLobby(id);
            if (lobby != null) return lobby;

            if (checkIfExists)
            {
                var response = await Client.PostAsync("lobbyExists", new { Uid = PlayerManager.LocalPlayerUid, Token = Client.Token, Id = id });
                if (response is null) return null;
            }

            // If not cached
            lobby = new(id);
            CacheLobby(lobby);
            await lobby.Update(false);

            return lobby;
        }

        /// <summary>
        /// Finds a <see cref="Lobby"/> by their <paramref name="id"/> in cache.
        /// </summary>
        /// <param name="id">ID of a <see cref="Lobby"/>.</param>
        /// <returns>A cached <see cref="Lobby"/>.</returns>
        internal static Lobby GetCachedLobby(int id)
        {
            if (CachedLobbies.TryGetValue(id, out Lobby lobby)) return lobby;
            return null;
        }

        /// <summary>
        /// Caches the <see cref="Lobby"/>.
        /// </summary>
        /// <param name="lobby"><see cref="Lobby"/> to cache.</param>
        /// <returns><see langword="true"/> if <see cref="Lobby"/> was successfully cached or <see langword="false"/> if it was cached already.</returns>
        private static bool CacheLobby(Lobby lobby)
        {
            if (CachedLobbies.ContainsKey(lobby.Id)) return false;

            CachedLobbies.Add(lobby.Id,lobby);
            return true;
        }

        internal static void Init()
        {
            CachedLobbies = new();
            PublicLobbies = new();
        }
    }
}
