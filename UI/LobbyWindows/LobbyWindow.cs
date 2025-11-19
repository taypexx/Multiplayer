using LocalizeLib;
using Multiplayer.Data;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class LobbyWindow : BaseMultiplayerWindow
    {
        private Dictionary<ForumObject, Player> ButtonsPlayers;
        private LocalString MainDescription;

        private bool ActionButtonIsJoin;
        private Dictionary<int, LocalString> ActionButtonTitles;
        private Dictionary<int, LocalString> ActionButtonResponses;

        private ForumObject ActionButton;

        private PromptWindow JoinPrompt;
        private InputWindow PasswordPrompt;
        private PromptWindow LeavePrompt;
        private PromptWindow DisbandPrompt;

        private Lobby Lobby;
        internal bool IsAutoUpdating { get; private set; } = false;
        internal bool UpdateDebounce { get; private set; } = false;

        internal LobbyWindow() : base(Localization.Get("Lobby", "Title"), UIManager.PublicLobbiesWindow, "Lobbies.png")
        {
            ButtonsPlayers = new();
            ActionButtonTitles = new()
            {
                [0] = Localization.Get("Lobby", "Join"),
                [1] = Localization.Get("Lobby", "Leave"),
                [2] = Localization.Get("Lobby", "Disband"),
                [3] = Localization.Get("Lobby", "Full")
            };
            ActionButtonResponses = new()
            {
                [0] = Localization.Get("Lobby", "JoinSuccess"),
                [1] = Localization.Get("Lobby", "LeaveSuccess"),
                [2] = Localization.Get("Lobby", "DisbandSuccess"),
                [3] = Localization.Get("Lobby", "FullMessage")
            };

            JoinPrompt = new(Localization.Get("Lobby", "JoinPrompt"));
            JoinPrompt.AutoReset = true;
            JoinPrompt.OnCompletion += OnActionDecided;

            PasswordPrompt = new(Localization.Get("Lobby", "PasswordPrompt"));
            PasswordPrompt.AutoReset = true;
            PasswordPrompt.OnCompletion += OnActionDecided;

            LeavePrompt = new(Localization.Get("Lobby", "LeavePrompt"));
            LeavePrompt.AutoReset = true;
            LeavePrompt.OnCompletion += OnActionDecided;

            DisbandPrompt = new(Localization.Get("Lobby", "DisbandPrompt"));
            DisbandPrompt.AutoReset = true;
            DisbandPrompt.OnCompletion += OnActionDecided;

            AddReturnButton();
            ActionButton = AddButton(ActionButtonTitles[0], null, MainDescription);
        }

        /// <summary>
        /// Starts the auto update loop and updates the lobby every <see cref="AutoUpdateInterval"/>.
        /// </summary>
        /// <returns></returns>
        private async Task AutoUpdateStart()
        {
            IsAutoUpdating = true;

            while (IsAutoUpdating)
            {
                await Task.Delay(LobbyManager.AutoUpdateInterval);
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
                ActionButton.Titles = ActionButtonIsJoin ? lobby.Players.Count < lobby.MaxPlayers ? ActionButtonTitles[0] : ActionButtonTitles[3] : lobby.Host == PlayerManager.LocalPlayer ? ActionButtonTitles[2] : ActionButtonTitles[1];

                MainDescription = new(string.Format(
                    Localization.Get("Lobby", "Description").ToString(),
                    lobby.Name,
                    lobby.Id,
                    lobby.Host.MultiplayerStats.Name,
                    lobby.Players.Count, lobby.MaxPlayers,
                    lobby.IsPrivate ? Constants.Red : Constants.Green,
                    lobby.IsPrivate ? Localization.Get("Lobby", "PrivateStatus").ToString() : Localization.Get("Lobby", "PublicStatus").ToString(),
                    lobby.Locked ? Localization.Get("Global", "Yes").ToString() : Localization.Get("Global", "No").ToString()
                ));

                Title = lobby.NameLocal;
                JoinPrompt.Title = Title;
                LeavePrompt.Title = Title;
                DisbandPrompt.Title = Title;
                ActionButton.Contents = MainDescription;

                ReturnButton.Contents = MainDescription;
                ReturnWindow = LobbyManager.LocalLobby == Lobby ? UIManager.MainMenu : Lobby.IsPrivate ? UIManager.LobbiesWindow : UIManager.PublicLobbiesWindow;

                foreach (string playerUid in lobby.Players)
                {
                    Player player = PlayerManager.GetCachedPlayer(playerUid);
                    if (ButtonsPlayers.ContainsValue(player)) continue;

                    ForumObject button = AddButton(
                        lobby.Host == player ? new($"<color={Constants.Yellow}>{player.MultiplayerStats.Name}</color>") : player.MultiplayerStats.NameLocal,
                        null, MainDescription
                    );
                    ButtonsPlayers.Add(button, player);
                }

                if (!lobby.IsPrivate) UIManager.PublicLobbiesWindow.UpdateLobbyButton(lobby);

                UpdateDebounce = false;
            });
        }

        private async void OnActionDecided(BaseWindow window)
        {
            if (window == JoinPrompt && JoinPrompt.Result == true || window == PasswordPrompt && !string.IsNullOrEmpty(PasswordPrompt.Result))
            {
                UIManager.Debounce = true;
                UpdateDebounce = true;

                bool success = await LobbyManager.JoinLobby(Lobby, PasswordPrompt.Result);
                LocalString msg = success ? ActionButtonResponses[0] : window == PasswordPrompt ? Localization.Get("Lobby", "IncorrectPassword") : Localization.Get("Warning", "Unknown");

                UpdateDebounce = false;
                if (success) await Update(Lobby);

                Main.Dispatcher.Enqueue(() =>
                {
                    PopupUtils.ShowInfo(msg);

                    UIManager.Debounce = false;
                    Window.Show();
                });
            }
            else if (window == LeavePrompt && LeavePrompt.Result == true || window == DisbandPrompt && DisbandPrompt.Result == true)
            {
                UIManager.Debounce = true;
                UpdateDebounce = true;

                bool success = await LobbyManager.LeaveLobby();
                LocalString msg = success ? ActionButtonResponses[window == LeavePrompt ? 1 : 2] : Localization.Get("Warning", "Unknown");

                UpdateDebounce = false;
                if (success) await Update(Lobby);

                Main.Dispatcher.Enqueue(() =>
                {
                    PopupUtils.ShowInfo(msg);

                    UIManager.Debounce = false;
                    UIManager.LobbiesWindow.Window.Show();
                });
            }
            else Window.Show();
        }

        internal override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == ActionButton)
            {
                // If the local player is in another lobby
                if (LobbyManager.LocalLobby != null && LobbyManager.LocalLobby != Lobby)
                {
                    PopupUtils.ShowInfoAndLog(Localization.Get("Lobby", "AlreadyInLobby"));
                    Window.Show();
                    return;
                }
                // If the lobby is locked
                if (Lobby.Locked)
                {
                    PopupUtils.ShowInfoAndLog(Localization.Get("Lobby", "LobbyIsLocked"));
                    Window.Show();
                    return;
                }

                if (ActionButtonIsJoin && Lobby.Players.Count < Lobby.MaxPlayers)
                {
                    if (Lobby.IsPrivate) PasswordPrompt.Show();
                    else JoinPrompt.Show();
                }
                else if (!ActionButtonIsJoin)
                {
                    (Lobby.Host == PlayerManager.LocalPlayer ? DisbandPrompt : LeavePrompt).Show();
                }
                else
                {
                    PopupUtils.ShowInfoAndLog(ActionButtonResponses[3]);
                    Window.Show();
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
