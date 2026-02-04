using Il2CppSirenix.Serialization.Utilities;
using LocalizeLib;
using Multiplayer.Data.Lobbies;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using Multiplayer.UI.Extensions;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class LobbyCreationWindow : BaseMultiplayerWindow
    {
        private ForumObject NameButton;
        private ForumObject MaxPlayersButton;
        private ForumObject PasswordButton;
        private ForumObject GoalButton;
        private ForumObject PlayTypeButton;
        private ForumObject ChartSelectionButton;
        private ForumObject PlaylistSizeButton;
        private ForumObject CreateButton;

        private PromptWindow CreatePrompt;
        private InputWindow NamePrompt;
        private InputWindow MaxPlayersPrompt;
        private InputWindow PasswordPrompt;
        private InputWindow PlaylistSizePrompt;

        private string NameField = "Lobby";
        private int MaxPlayersField = 5;
        private string PasswordField;
        private int PlaylistSizeField = 5;
        private LobbyGoal GoalField => UIManager.LobbyGoalWindow.Value;
        private LobbyPlayType PlayTypeField => UIManager.LobbyPlayTypeWindow.Value;
        private LobbyChartSelection ChartSelectionField => UIManager.LobbyChartSelectionWindow.Value;

        private LocalString MainDescription;
        private string InvalidNameMsg = string.Format(Localization.Get("LobbyCreation", "LobbyNameInvalid").ToString(), Constants.NameCharactersMin, Constants.NameCharactersMax);
        private string InvalidMaxPlayersMsg = string.Format(Localization.Get("LobbyCreation", "MaxPlayersInvalid").ToString(), Constants.PlayersMin, Constants.PlayersMax);
        private string InvalidPasswordMsg = string.Format(Localization.Get("LobbyCreation", "PasswordInvalid").ToString(), Constants.PasswordCharactersMin, Constants.PasswordCharactersMax);
        private string InvalidPlaylistSize = string.Format(Localization.Get("LobbyCreation", "PlaylistSizeInvalid").ToString(), Constants.PlaylistSizeMin, Constants.PlaylistSizeMax);

        internal LobbyCreationWindow() : base(Localization.Get("LobbyCreation", "Title"), UIManager.LobbiesWindow, "Lobby.png")
        {
            CreatePrompt = new(Localization.Get("LobbyCreation", "CreateConfirm"), Localization.Get("LobbyCreation", "Create"));
            CreatePrompt.AutoReset = true;
            CreatePrompt.OnCompletion += (window) => _ = OnCreate(window);

            NamePrompt = new();
            NamePrompt.AutoReset = true;
            NamePrompt.OnCompletion += OnNameFieldChanged;

            MaxPlayersPrompt = new();
            MaxPlayersPrompt.AutoReset = true;
            MaxPlayersPrompt.OnCompletion += OnMaxPlayersFieldChanged;

            PasswordPrompt = new();
            PasswordPrompt.AutoReset = true;
            PasswordPrompt.OnCompletion += OnPasswordFieldChanged;

            PlaylistSizePrompt = new();
            PlaylistSizePrompt.AutoReset = true;
            PlaylistSizePrompt.OnCompletion += OnPlaylistSizeChanged;
        }

        internal void CreateButtons()
        {
            CreateButton = AddButton(Localization.Get("LobbyCreation", "CreateButton"), CreatePrompt);
            NameButton = AddButton(Localization.Get("LobbyCreation", "LobbyName"), NamePrompt);
            MaxPlayersButton = AddButton(Localization.Get("LobbyCreation", "MaxPlayers"), MaxPlayersPrompt);
            PasswordButton = AddButton(Localization.Get("LobbyCreation", "SetPassword"), null);
            GoalButton = AddButton(Localization.Get("LobbyCreation", "Goal"), UIManager.LobbyGoalWindow);
            PlayTypeButton = AddButton(Localization.Get("LobbyCreation", "PlayType"), UIManager.LobbyPlayTypeWindow);
            ChartSelectionButton = AddButton(Localization.Get("LobbyCreation", "ChartSelection"), UIManager.LobbyChartSelectionWindow);
            PlaylistSizeButton = AddButton(Localization.Get("LobbyCreation", "PlaylistSize"), PlaylistSizePrompt);

            UpdateDescription();
        }

        private async Task OnCreate(BaseWindow window)
        {
            if (CreatePrompt.Result != true) 
            {
                Window.Show();
                return;
            };
            // If the local player is using sleepwalker
            if (PlayerManager.LocalPlayer.MultiplayerStats.GirlIndex == Constants.SleepwalkerRoleIndex)
            {
                PopupUtils.ShowInfo(Localization.Get("Lobby", "SleepwalkerUsedLocal"));
                Window.Show();
                return;
            }

            Main.Dispatch(() => PnlCloudExtension.Start(Localization.Get("PnlCloudMessage", "Creating").ToString()));
            UIManager.Debounce = true;

            bool success = await LobbyManager.CreateLobby(MaxPlayersField,GoalField,PlayTypeField,ChartSelectionField,NameField,PlaylistSizeField,PasswordField);

            Main.Dispatch(() =>
            {
                UIManager.Debounce = false;
                PnlCloudExtension.Finish(success);

                if (success) _ = UIManager.OpenLobbyWindow();
                else UIManager.LobbiesWindow.Window.Show();
            });
        }

        private void OnNameFieldChanged(BaseWindow window)
        {
            if (Utilities.IsValidString(NamePrompt.Result, Constants.NameCharactersMin, Constants.NameCharactersMax))
            {
                NameField = NamePrompt.Result;
            } 
            else if (!NamePrompt.Result.IsNullOrWhitespace())
            {
                PopupUtils.ShowInfo(InvalidNameMsg);
            }

            UpdateDescription();
            Window.Show();
        }

        private void OnMaxPlayersFieldChanged(BaseWindow window)
        {
            int? maxPlayers = Utilities.GetValidNumber(MaxPlayersPrompt.Result, Constants.PlayersMin, Constants.PlayersMax);
            if (maxPlayers != null)
            {
                MaxPlayersField = (int)maxPlayers;
            }
            else if (!MaxPlayersPrompt.Result.IsNullOrWhitespace())
            {
                PopupUtils.ShowInfo(InvalidMaxPlayersMsg);
            }

            UpdateDescription();
            Window.Show();
        }

        private void OnPasswordFieldChanged(BaseWindow window)
        {
            if (Utilities.IsValidString(PasswordPrompt.Result, Constants.PasswordCharactersMin, Constants.PasswordCharactersMax))
            {
                PasswordField = PasswordPrompt.Result;
                PasswordButton.Titles = Localization.Get("LobbyCreation", "RemovePassword");
            } else
            {
                PasswordField = null;
                if (!PasswordPrompt.Result.IsNullOrWhitespace()) PopupUtils.ShowInfo(InvalidPasswordMsg);
            }

            UpdateDescription();
            Window.Show();
        }

        private void OnPlaylistSizeChanged(BaseWindow window)
        {
            int? playlistSize = Utilities.GetValidNumber(PlaylistSizePrompt.Result, Constants.PlaylistSizeMin, Constants.PlaylistSizeMax);
            if (playlistSize != null)
            {
                PlaylistSizeField = (int)playlistSize;
            }
            else if (!PlaylistSizePrompt.Result.IsNullOrWhitespace())
            {
                PopupUtils.ShowInfo(InvalidPlaylistSize);
            }

            UpdateDescription();
            Window.Show();
        }

        internal void UpdateDescription()
        {
            MainDescription = new(
                string.Format(
                    Localization.Get("LobbyCreation", "Description").ToString(),
                    NameField,
                    Constants.Yellow, MaxPlayersField,
                    PasswordField is null ? Constants.Green : Constants.Yellow, 
                    PasswordField is null ? Localization.Get("LobbyCreation", "NotSet").ToString() : PasswordField,
                    Constants.GoalColors[GoalField], GoalField,
                    Constants.PlayTypeColors[PlayTypeField], PlayTypeField,
                    Constants.ChartSelectionColors[ChartSelectionField], ChartSelectionField,
                    Constants.Yellow, PlaylistSizeField
                 )
            );

            foreach (ForumObject button in ButtonsWindows.Keys)
            {
                button.Contents = MainDescription;
            }
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];
            if (button == PasswordButton)
            {
                if (PasswordField is null) PasswordPrompt.Show();
                else
                {
                    PasswordField = null;
                    PasswordButton.Titles = Localization.Get("LobbyCreation", "SetPassword");
                    UpdateDescription();
                    Window.Show();
                }
            }
        }
    }
}
