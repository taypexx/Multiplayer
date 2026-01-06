using Il2CppAssets.Scripts.Database;
using Multiplayer.Data.Lobbies;
using Multiplayer.Static;
using PopupLib.UI;
using System.Net.Http.Json;

namespace Multiplayer.Managers
{
    internal static class LobbyManager
    {
        internal static Dictionary<int,Lobby> CachedLobbies { get; private set; }
        internal static List<Lobby> PublicLobbies 
        { 
            get 
            { 
                if (CachedLobbies is null) return null;

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
        internal static bool IsInLobby => LocalLobby != null;
        internal static bool CanChangePlaylist => IsInLobby && (LocalLobby.Host == PlayerManager.LocalPlayer || (!LocalLobby.Locked && LocalLobby.ChartSelection == LobbyChartSelection.Playlist));

        internal static bool IsAutoUpdating = false;

        /// <summary>
        /// Starts the auto update loop and updates the lobby every <see cref="AutoUpdateInterval"/>.
        /// </summary>
        /// <returns></returns>
        internal static async Task AutoUpdateStart(Lobby lobby)
        {
            IsAutoUpdating = true;

            while (IsAutoUpdating && Client.Connected)
            {
                await Task.Delay(Settings.Config.LobbyUpdateIntervalMS);
                await UIManager.LobbyWindow.Update(lobby);
                
                if (lobby == LocalLobby)
                {
                    Main.Dispatcher.Enqueue(() => 
                    {
                        UIManager.MainLobbyDisplay.Update();
                        UIManager.UpdatePnlPreparation();
                    });
                    if (LocalLobby.Locked && LocalLobby.Host != PlayerManager.LocalPlayer && Main.CurrentScene == "UISystem_PC")
                    {
                        _ = UIManager.ShowInfoAndStartGame();
                    }
                }
            }
        }

        /// <summary>
        /// Sends a request to the server to continue and remove the first playlist entry.
        /// </summary>
        internal static async Task<bool> PlaylistContinue()
        {
            if (!Client.Connected || !IsInLobby || LocalLobby.Host != PlayerManager.LocalPlayer) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid
            };

            var response = await Client.PostAsync("lobbyPlaylistContinue", payload);
            bool success = response != null;

            if (success) LocalLobby.Playlist.RemoveAt(0);
            return success;
        }

        /// <summary>
        /// Sends a request to the server to add a new entry to the playlist.
        /// </summary>
        internal static async Task<bool> PlaylistAdd(MusicInfo musicInfo, int difficulty)
        {
            if (!Client.Connected || !IsInLobby || LocalLobby.IsPlaylistFull) return false;

            string entry = ChartManager.GetEntry(musicInfo, difficulty);
            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Entry = entry
            };

            var response = await Client.PostAsync("lobbyPlaylistAdd", payload);
            bool success = response != null;

            if (success)
            {
                LocalLobby.Playlist.Add(new(musicInfo,difficulty,entry));
                Main.Dispatcher.Enqueue(() => 
                {
                    PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "PlaylistAdded"));
                    UIManager.LobbyPlaylistWindow.Update(LocalLobby);
                    UIManager.UpdatePnlPreparation();
                });
            } else PopupUtils.ShowInfo(Localization.Get("Lobby", "ChartHidden"));

            return success;
        }

        /// <summary>
        /// Sends a request to the server to remove the entry from the playlist.
        /// </summary>
        internal static async Task<bool> PlaylistRemove(MusicInfo musicInfo, int difficulty)
        {
            if (!Client.Connected || !IsInLobby) return false;

            string entry = ChartManager.GetEntry(musicInfo, difficulty);
            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Entry = entry
            };

            var response = await Client.PostAsync("lobbyPlaylistRemove", payload);
            bool success = response != null;

            if (success)
            {
                LocalLobby.Playlist.Remove(LocalLobby.GetFromPlaylist(entry));
                Main.Dispatcher.Enqueue(() =>
                {
                    PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "PlaylistRemoved"));
                    UIManager.LobbyPlaylistWindow.Update(LocalLobby);
                    UIManager.UpdatePnlPreparation();
                });
            }
            return success;
        }

        /// <summary>
        /// Sends a ready request to the server indicating that you are ready to play the chart (or not).
        /// </summary>
        /// <returns><see langword="true"/> if ready was set, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> SetReady(bool isReady)
        {
            if (!Client.Connected || !IsInLobby) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Ready = isReady
            };

            var response = await Client.PostAsync("lobbyReady", payload);
            bool success = response != null;

            return success;
        }

        /// <summary>
        /// Sends a lock request to the server to lock the current lobby.
        /// </summary>
        /// <returns><see langword="true"/> if the state was changed, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> LockLobby(bool isLocked)
        {
            if (!Client.Connected || !IsInLobby || LocalLobby.Host != PlayerManager.LocalPlayer) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Locked = isLocked
            };

            var response = await Client.PostAsync("lobbyLock", payload);
            bool success = response != null;
            if (success) LocalLobby.Locked = isLocked;
            return success;
        }

        /// <summary>
        /// Tries to restore the lobby of the player.
        /// </summary>
        /// <param name="lobbyId">Id of the lobby current local player is in.</param>
        private static async Task<bool> RestoreLobby()
        {
            if (!Client.Connected || IsInLobby) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
            };

            var response = await Client.PostAsync("hasLobby", payload);
            if (response is null) return false;

            int id = await response.Content.ReadFromJsonAsync<int>();
            if (id == 0) return false;

            Lobby lobby = await GetLobby(id, true);
            if (lobby is null) return false;

            AfterJoin(lobby);
            return true;
        }

        /// <summary>
        /// Sends a create request to the server
        /// </summary>
        /// <returns><see langword="true"/> if it was created successfully, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> CreateLobby(int maxPlayers, LobbyGoal goal, LobbyPlayType playType, LobbyChartSelection chartSelection, string name, int playlistSize, string password = null)
        {
            if (!Client.Connected || IsInLobby) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                MaxPlayers = maxPlayers,
                Goal = (byte)goal,
                PlayType = (byte)playType,
                ChartSelection = (byte)chartSelection,
                PlaylistSize = playlistSize,
                Name = name,
                Password = password
            };

            var response = await Client.PostAsync("createLobby", payload);
            bool success = response != null;
            if (success)
            {
                int id = await response.Content.ReadFromJsonAsync<int>();
                if (id != 0)
                {
                    Lobby lobby = await GetLobby(id);
                    AfterJoin(lobby);
                } 
                else return false;
            }
            return success;
        }

        /// <summary>
        /// Sends a request to the server to join the <see cref="Lobby"/>.
        /// </summary>
        /// <param name="lobby"></param>
        /// <returns><see langword="true"/> if join was successful, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> JoinLobby(Lobby lobby, string passwordGuess = null)
        {
            if (!Client.Connected || IsInLobby || lobby is null || lobby.Locked) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Id = lobby.Id,
                PasswordGuess = passwordGuess
            };

            var response = await Client.PostAsync("joinLobby", payload);
            bool success = response != null;

            if (success) AfterJoin(lobby);
            return success;
        }

        /// <summary>
        /// Sends a request to the server to leave the <see cref="Lobby"/>.
        /// </summary>
        /// <param name="lobby"></param>
        /// <returns><see langword="true"/> if left successfully, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> LeaveLobby(bool leaveAnyway = false)
        {
            if (!Client.Connected || !IsInLobby) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
            };

            var response = await Client.PostAsync("leaveLobby", payload);
            bool success = response != null;

            if (success || leaveAnyway) AfterLeave();
            return success;
        }

        /// <summary>
        /// Assigns LocalLobby to the new <see cref="Lobby"/> and updates everything.
        /// </summary>
        private static void AfterJoin(Lobby lobby)
        {
            LocalLobby = lobby;
            _ = AutoUpdateStart(lobby);

            Main.Dispatcher.Enqueue(() =>
            {
                UIManager.MainLobbyDisplay.Create(lobby);
                UIManager.UpdatePnlPreparation();
                UIManager.MainMenu.UpdateLobbiesButton();
            });
        }

        /// <summary>
        /// Removes LocalLobby and updates everything.
        /// </summary>
        private static void AfterLeave()
        {
            LocalLobby = null;
            IsAutoUpdating = false;

            Main.Dispatcher.Enqueue(() =>
            {
                UIManager.MainLobbyDisplay.Destroy();
                UIManager.UpdatePnlPreparation();
                UIManager.MainMenu.UpdateLobbiesButton();
            });
        }

        /// <summary>
        /// Finds a <see cref="Lobby"/> by their <paramref name="id"/>.
        /// </summary>
        /// <param name="id">ID of a <see cref="Lobby"/>.</param>
        /// <param name="checkIfExists">Additionally check if the <see cref="Lobby"/> exists on the server. Will return <see langword="null"/> if it doesn't.</param>
        /// <returns>A <see cref="Lobby"/> that was cached or a new instance.</returns>
        internal static async Task<Lobby> GetLobby(int id, bool checkIfExists = false)
        {
            if (!Client.Connected) return null;

            Lobby lobby = GetCachedLobby(id);
            if (lobby != null) return lobby;

            if (checkIfExists)
            {
                var response = await Client.PostAsync("lobbyExists", new { Uid = PlayerManager.LocalPlayerUid, Id = id });
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
        internal static void CacheLobby(Lobby lobby)
        {
            if (CachedLobbies.ContainsKey(lobby.Id)) return;

            CachedLobbies.Add(lobby.Id,lobby);
        }

        /// <summary>
        /// Removes the <see cref="Lobby"/> from cache.
        /// </summary>
        /// <param name="lobby"><see cref="Lobby"/> to remove.</param>
        internal static void ClearLobbyFromCache(Lobby lobby)
        {
            if (!CachedLobbies.ContainsKey(lobby.Id)) return;

            CachedLobbies.Remove(lobby.Id);
        }

        private static async Task CacheCleaner()
        {
            DateTime current;
            while (true)
            {
                await Task.Delay(Constants.CacheCheckInterval);
                current = DateTime.Now;

                foreach (Lobby lobby in CachedLobbies.Values)
                {
                    if (current - lobby.LastUpdated >= Constants.LobbyCacheExpiration && lobby != LocalLobby)
                    {
                        // check this too
                        //ClearLobbyFromCache(lobby);
                    }
                }
            }
        }

        internal static async Task Init()
        {
            CachedLobbies = new();
            PublicLobbies = new();
            _ = CacheCleaner();
            await RestoreLobby();
        }
    }
}
