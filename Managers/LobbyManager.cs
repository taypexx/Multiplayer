using Il2CppAssets.Scripts.Database;
using LocalizeLib;
using Multiplayer.Data.Lobbies;
using Multiplayer.Static;
using Multiplayer.UI.Extensions;
using PopupLib.UI;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Managers
{
    internal static class LobbyManager
    {
        internal static Dictionary<int,Lobby> CachedLobbies { get; private set; }
        internal static IEnumerable<Lobby> PublicLobbies => CachedLobbies.Values.Where(l => !l.IsPrivate).AsEnumerable();

        internal static Lobby LocalLobby { get; set; }
        internal static bool IsInLobby => LocalLobby != null;
        internal static bool CanChangePlaylist => IsInLobby && (LocalLobby.Host == PlayerManager.LocalPlayer || (!LocalLobby.Locked && LocalLobby.ChartSelection == LobbyChartSelection.Playlist));
        internal static bool IsPlaylistChartComingUp => IsInLobby && LocalLobby.Locked && LocalLobby.CurrentPlaylistEntry != null;

        internal static bool IsAutoUpdating = false;

        /// <summary>
        /// Starts auto updating the UI according to the current <see cref="Lobby"/>.
        /// </summary>
        internal static async Task AutoUpdateStart(Lobby lobby)
        {
            IsAutoUpdating = true;

            while (IsAutoUpdating && Client.Connected)
            {
                await Task.Delay(Settings.Config.LobbyUpdateIntervalMS);

                // Websocket handles the lobby update (if local lobby), we just update the window without lobby
                await UIManager.LobbyWindow.Update(lobby, lobby != LocalLobby);

                if (lobby != LocalLobby) continue;
                
                await SyncLobby();
                Main.Dispatch(() => 
                {
                    UIManager.LobbyPlaylistWindow.Update(lobby);
                    UIManager.MainLobbyDisplay.Update();
                    PnlPreparationExtension.UpdatePnlPreparation();
                    PnlHomeExtension.UpdateAllPages();
                });

                // Start condition (for everyone except host)
                if (Main.IsUIScene && LocalLobby.Locked && LocalLobby.Host != PlayerManager.LocalPlayer && LocalLobby.CurrentPlaylistEntry != null)
                {
                    _ = Intermission.Start();
                }
            }
        }

        /// <summary>
        /// Synchronizes the local <see cref="Lobby"/> via websocket.
        /// </summary>
        internal static async Task SyncLobby()
        {
            var payload = new
            {
                Type = "Sync",
                Body = new 
                {
                    Uid = PlayerManager.LocalPlayerUid,
                    Id = LocalLobby.Id,
                    GetPlayers = true,
                    PingMS = Client.PingMS
                }
            };
            await Client.WebsocketSend(payload, true);
        }

        /// <summary>
        /// Sends a request to the server to kick the player from the <see cref="Lobby"/>.
        /// </summary>
        /// <param name="playerUid">UID of a rogue.</param>
        /// <returns><see langword="true"/> if the player was kicked, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> KickPlayer(string playerUid)
        {
            if (!Client.Connected || !IsInLobby || LocalLobby.Host != PlayerManager.LocalPlayer) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                TargetUid = playerUid
            };

            var response = await Client.PostAsync("lobbyKick", payload);
            bool success = response != null;

            if (success && LocalLobby.Players.Contains(playerUid))
            {
                LocalLobby.Players.Remove(playerUid);
            }

            return success;
        }

        /// <summary>
        /// Sends a request to the server to continue and remove the first <see cref="PlaylistEntry"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the first <see cref="PlaylistEntry"/> was removed, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> PlaylistContinue()
        {
            if (!Client.Connected || !IsInLobby || LocalLobby.Host != PlayerManager.LocalPlayer) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid
            };

            // If the current index is not the last one
            if (LocalLobby.CurrentPlaylistEntryIndex < LocalLobby.Playlist.Count - 1)
            {
                var response = await Client.PostAsync("lobbyPlaylistContinue", payload);
                bool success = response != null;

                return success;
            }
            // If that was the last chart in the playlist
            else
            {
                LocalLobby.Playlist.Clear();
                return await LockLobby(false);
            }
        }

        /// <summary>
        /// Sends a request to the server to add a new <see cref="PlaylistEntry"/> to the playlist.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="PlaylistEntry"/> was added, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> PlaylistAdd(MusicInfo musicInfo, int difficulty)
        {
            if (!Client.Connected || !IsInLobby || LocalLobby.IsPlaylistFull) return false;

            string entry = ChartManager.GetEntry(musicInfo, difficulty);
            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
                Entry = entry
            };

            var response = await Client.PostAsync("lobbyPlaylistAdd", payload, false, true, true);
            int result = 4;
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    result = await response.Content.ReadFromJsonAsync<int>();
                }
                catch { }
            }

            bool success = result == 0;
            if (success && !LocalLobby.HasInPlaylist(entry))
            {
                LocalLobby.Playlist.Add(new(musicInfo, difficulty, entry));
                Main.Dispatch(() => 
                {
                    PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "PlaylistAdded"));
                    UIManager.LobbyPlaylistWindow.Update(LocalLobby);
                    PnlPreparationExtension.UpdatePnlPreparation();
                });
            } 
            else Main.Dispatch(() =>
            {
                LocalString reason;
                switch (result)
                {
                    case 1:
                        reason = Localization.Get("Warning", "Unknown");
                        break;
                    case 2:
                        reason = Localization.Get("Lobby", "ChartHidden");
                        break;
                    case 3:
                        reason = Localization.Get("Lobby", "ChartNotSynced");
                        break;
                    default:
                        reason = Localization.Get("Warning", "Unknown");
                        break;
                }
                PopupUtils.ShowInfo(reason);
            });

            return success;
        }

        /// <summary>
        /// Sends a request to the server to remove the <see cref="PlaylistEntry"/> from the playlist.
        /// </summary>
        /// <returns><see langword="true"/> if the <see cref="PlaylistEntry"/> was removed, otherwise <see langword="false"/>.</returns>
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
                Main.Dispatch(() =>
                {
                    PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "PlaylistRemoved"));
                    UIManager.LobbyPlaylistWindow.Update(LocalLobby);
                    PnlPreparationExtension.UpdatePnlPreparation();
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
        /// Sends a lock request to the server to lock the current <see cref="Lobby"/>.
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
        /// Tries to restore the <see cref="Lobby"/> of the local player.
        /// </summary>
        /// <param name="lobbyId">Id of the <see cref="Lobby"/> current local player is in.</param>
        /// <returns><see langword="true"/> if the <see cref="Lobby"/> was restored, otherwise <see langword="false"/>.</returns>
        private static async Task<bool> RestoreLobby()
        {
            if (!Client.Connected || IsInLobby) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
            };

            var response = await Client.PostAsync("hasLobby", payload);
            if (response is null) return false;

            var idJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (idJson.ValueKind == JsonValueKind.Null || !idJson.TryGetInt32(out int id)) return false;

            Lobby lobby = await GetLobby(id, true);
            if (lobby is null) return false;

            OnJoin(lobby);
            return true;
        }

        /// <summary>
        /// Sends a request to create a new <see cref="Lobby"/> to the server.
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
                    OnJoin(lobby);
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

            if (success) OnJoin(lobby);
            return success;
        }

        /// <summary>
        /// Sends a request to the server to leave the <see cref="Lobby"/>.
        /// </summary>
        /// <returns><see langword="true"/> if left successfully, otherwise <see langword="false"/>.</returns>
        internal static async Task<bool> LeaveLobby(bool leaveAnyway = false)
        {
            if ((!Client.Connected || !IsInLobby || LocalLobby.Locked) && !leaveAnyway) return false;

            var payload = new
            {
                Uid = PlayerManager.LocalPlayerUid,
            };

            var response = await Client.PostAsync("leaveLobby", payload);
            bool success = response != null;

            if (success || leaveAnyway) OnLeave();
            return success;
        }

        /// <summary>
        /// Assigns LocalLobby to the new <see cref="Lobby"/> and updates everything.
        /// </summary>
        private static void OnJoin(Lobby lobby)
        {
            LocalLobby = lobby;
            _ = Client.WebsocketStart();
            _ = AutoUpdateStart(lobby);

            Main.Dispatch(() =>
            {
                UIManager.PlayConfirmPrompt.Title = (LocalString)lobby.Name;
                UIManager.MainLobbyDisplay.Create(lobby);
                UIManager.ChatLobbyDisplay.Create(lobby, true);
                PnlPreparationExtension.UpdatePnlPreparation();
                UIManager.MainMenu.UpdateLobbiesButton();
                PnlHomeExtension.Create();
            });
        }

        /// <summary>
        /// Sets LocalLobby to <see langword="null"/> and updates everything.
        /// </summary>
        private static void OnLeave()
        {
            var prevLobby = LocalLobby;
            LocalLobby = null;
            _ = Client.WebsocketClose();
            IsAutoUpdating = false;

            Main.Dispatch(() =>
            {
                UIManager.MainLobbyDisplay.Destroy();
                UIManager.ChatLobbyDisplay.Destroy();
                PnlPreparationExtension.UpdatePnlPreparation();
                UIManager.MainMenu.UpdateLobbiesButton();
                PnlHomeExtension.Destroy();
                ClearLobbyFromCache(prevLobby);
            });
        }

        /// <summary>
        /// Gets all public lobbies from the server and also updates the cached ones.
        /// </summary>
        internal static async Task UpdatePublicLobbies()
        {
            var response = await Client.GetAsync("getLobbies", false, false);
            if (response == null) return;

            try
            {
                var lobbies = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
                foreach ((string id_, var body) in lobbies)
                {
                    var id = int.Parse(id_);
                    if (IsInLobby && LocalLobby.Id == id) continue;

                    Lobby lobby = GetCachedLobby(id);
                    if (lobby == null)
                    {
                        lobby = new(id);
                        CacheLobby(lobby);
                    }
                    await lobby.UpdateFields(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body));
                }
            }
            catch { return; }
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
            if (lobby == null || !CachedLobbies.ContainsKey(lobby.Id)) return;

            CachedLobbies.Remove(lobby.Id);
        }

        /// <summary>
        /// Starts cleaning lobbies from cache over time.
        /// </summary>
        private static async Task CacheCleaner()
        {
            DateTime current;
            while (Client.Connected)
            {
                await Task.Delay(Constants.CacheCheckIntervalMS);
                current = DateTime.Now;

                foreach (Lobby lobby in CachedLobbies.Values)
                {
                    if (current - lobby.LastUpdated >= Constants.LobbyCacheExpiration && lobby != LocalLobby)
                    {
                        ClearLobbyFromCache(lobby);
                    }
                }
            }
        }

        internal static async Task Init()
        {
            CachedLobbies = new();
            await RestoreLobby();
            _ = CacheCleaner();
        }
    }
}
