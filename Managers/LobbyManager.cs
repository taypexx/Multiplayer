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
                    field.Add(lobby);
                }
                return field;
            } 
            private set; 
        }
        internal static Lobby LocalLobby { get; set; }

        /// <summary>
        /// Finds/creates a <see cref="Lobby"/> by their <paramref name="id"/>.
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
            await newLobby.Update();

            return newLobby;
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
