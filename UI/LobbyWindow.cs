using LocalizeLib;
using Multiplayer.Data;
using Multiplayer.Managers;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI
{
    internal sealed class LobbyWindow : BaseMultiplayerWindow
    {
        private Dictionary<ForumObject, Player> ButtonsPlayers;
        private LocalString MainDescription;

        private bool ActionButtonIsJoin;
        private Dictionary<int,LocalString> ActionButtonTitles;
        private Dictionary<int,LocalString> ActionButtonResponses;

        private ForumObject ActionButton;

        private PromptWindow JoinPrompt;
        private PromptWindow LeavePrompt;
        private PromptWindow DisbandPrompt;

        private Lobby Lobby;
        private bool IsAutoUpdating = false;
        private bool UpdateDebounce = false;
        private TimeSpan AutoUpdateInterval = TimeSpan.FromSeconds(5);

        internal LobbyWindow() : base(Localization.Get("Lobby", "Title"), UIManager.PublicLobbiesWindow, "Lobbies.png")
        {
            ButtonsPlayers = new();
            ActionButtonTitles = new()
            {
                [0] = Localization.Get("Lobby", "Join"),
                [1] = Localization.Get("Lobby", "Leave"),
                [2] = Localization.Get("Lobby", "Disband")
            };
            ActionButtonResponses = new()
            {
                [0] = Localization.Get("Lobby", "JoinSuccess"),
                [1] = Localization.Get("Lobby", "LeaveSuccess"),
                [2] = Localization.Get("Lobby", "DisbandSuccess")
            };

            JoinPrompt = new(Localization.Get("Lobby","JoinPrompt"));
            JoinPrompt.AutoReset = true;
            JoinPrompt.OnCompletion += OnActionDecided;

            LeavePrompt = new(Localization.Get("Lobby", "LeavePrompt"));
            LeavePrompt.AutoReset = true;
            LeavePrompt.OnCompletion += OnActionDecided;

            DisbandPrompt = new(Localization.Get("Lobby", "DisbandPrompt"));
            DisbandPrompt.AutoReset = true;
            DisbandPrompt.OnCompletion += OnActionDecided;

            AddReturnButton();
            ActionButton = AddButton(ActionButtonTitles[0],null, MainDescription);
        }

        private async Task AutoUpdateStart()
        {
            IsAutoUpdating = true;

            while (IsAutoUpdating)
            {
                await Task.Delay(AutoUpdateInterval);
                await Update(Lobby);
            }
        }

        /// <summary>
        /// Updates the <see cref="LobbyWindow"/> to display the information about the given <see cref="Data.Lobby"/>.
        /// </summary>
        /// <param name="lobby"><see cref="Data.Lobby"/> that will now appear in the window.</param>
        /// <param name="updatePlayers">Whether to update players in the lobby.</param>
        internal async Task Update(Lobby lobby, bool updatePlayers = false)
        {
            if (UpdateDebounce || lobby is null) return;
            UpdateDebounce = true;

            Lobby = lobby;
            await lobby.Update(updatePlayers);

            Main.Dispatcher.Enqueue(() =>
            {
                RemoveAllButtons(true, ActionButton);
                ButtonsPlayers.Clear();

                ActionButtonIsJoin = !lobby.IsMember(PlayerManager.LocalPlayer);
                ActionButton.Titles = ActionButtonIsJoin ? ActionButtonTitles[0] : Lobby.Host == PlayerManager.LocalPlayer ? ActionButtonTitles[2] : ActionButtonTitles[1];

                MainDescription = new(string.Format(
                    Localization.Get("Lobby", "Description").ToString(),
                    lobby.Name,
                    lobby.Host.MultiplayerStats.Name,
                    lobby.Players.Count, lobby.MaxPlayers,
                    lobby.IsPrivate ? "f542adff" : "1eff00ff",
                    lobby.IsPrivate ? Localization.Get("Lobby", "PrivateStatus").ToString() : Localization.Get("Lobby", "PublicStatus").ToString()
                ));

                Title = Lobby.NameLocal;
                JoinPrompt.Title = Title;
                LeavePrompt.Title = Title;
                DisbandPrompt.Title = Title;
                ReturnButton.Contents = MainDescription;

                foreach (string playerUid in lobby.Players)
                {
                    Player player = PlayerManager.GetCachedPlayer(playerUid);
                    if (ButtonsPlayers.ContainsValue(player)) continue;

                    ForumObject button = AddButton(
                        lobby.Host == player ? new($"<color=fff700ff>{player.MultiplayerStats.Name}</color>") : player.MultiplayerStats.NameLocal,
                        UIManager.ProfileWindow, MainDescription
                    );
                    ButtonsPlayers.Add(button, player);
                }

                if (!lobby.IsPrivate)
                {
                    // TODO: update the specific button related to the current lobby
                }

                UpdateDebounce = false;
            });
        }

        private async void OnActionDecided(BaseWindow window)
        {
            if (window == JoinPrompt && JoinPrompt.Result == true)
            {
                UIManager.Debounce = true;
                UpdateDebounce = true;

                var payload = new
                {
                    Uid = PlayerManager.LocalPlayer.Uid,
                    Token = Client.Token,
                    Id = Lobby.Id
                };

                LocalString msg;
                var response = await Client.PostAsync("joinLobby", payload);
                if (response != null)
                {
                    msg = ActionButtonResponses[0];
                }
                else
                {
                    msg = Localization.Get("Warning", "Unknown");
                }

                UpdateDebounce = false;
                LobbyManager.LocalLobby = Lobby;
                await Update(Lobby);

                PopupUtils.ShowInfoAndLog(msg);

                UIManager.Debounce = false;
                Window.Show();
            }
            else if ((window == LeavePrompt && LeavePrompt.Result == true) || (window == DisbandPrompt && DisbandPrompt.Result == true))
            {
                UIManager.Debounce = true;
                UpdateDebounce = true;

                var payload = new
                {
                    Uid = PlayerManager.LocalPlayer.Uid,
                    Token = Client.Token
                };

                LocalString msg;
                var response = await Client.PostAsync("leaveLobby", payload);
                if (response != null)
                {
                    msg = ActionButtonResponses[window == LeavePrompt ? 1 : 2];
                }
                else
                {
                    msg = Localization.Get("Warning", "Unknown");
                }

                UpdateDebounce = false;
                LobbyManager.LocalLobby = null;
                await Update(Lobby);

                PopupUtils.ShowInfoAndLog(msg);

                UIManager.Debounce = false;
                ReturnWindow.Window.Show();
            }
            else
            {
                Window.Show();
            }
        }

        internal override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == ActionButton)
            {
                if (ActionButtonIsJoin)
                {
                    JoinPrompt.Show();
                }
                else
                {
                    (Lobby.Host == PlayerManager.LocalPlayer ? DisbandPrompt : LeavePrompt).Show();
                }
            }
            else if (ButtonsPlayers.TryGetValue(button, out Player player))
            {
                OpenProfileWindow(player);
            }
        }

        internal override void OnShow(BaseWindow window)
        {
            base.OnShow(window);

            UIManager.ProfileWindow.ReturnWindow = this;

            _ = AutoUpdateStart();
        }

        internal override void OnCompletion(BaseWindow window)
        {
            base.OnCompletion(window);

            IsAutoUpdating = false;
        }
    }
}
