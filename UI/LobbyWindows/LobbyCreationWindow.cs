using Il2CppSirenix.Serialization.Utilities;
using LocalizeLib;
using Multiplayer.Data.Lobbies;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;

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
        private ForumObject CreateButton;

        private PromptWindow CreatePrompt;
        private InputWindow NamePrompt;
        private InputWindow MaxPlayersPrompt;
        private InputWindow PasswordPrompt;

        private string NameField = "Lobby";
        private int MaxPlayersField = 5;
        private string PasswordField;
        private LobbyGoal GoalField => UIManager.LobbyGoalWindow.Value;
        private LobbyPlayType PlayTypeField => UIManager.LobbyPlayTypeWindow.Value;
        private LobbyChartSelection ChartSelectionField => UIManager.LobbyChartSelectionWindow.Value;

        private LocalString MainDescription;
        private string InvalidNameMsg = string.Format(Localization.Get("LobbyCreation", "LobbyNameInvalid").ToString(), Constants.NameCharactersMin, Constants.NameCharactersMax);
        private string InvalidMaxPlayersMsg = string.Format(Localization.Get("LobbyCreation", "MaxPlayersInvalid").ToString(), Constants.PlayersMin, Constants.PlayersMax);
        private string InvalidPasswordMsg = string.Format(Localization.Get("LobbyCreation", "PasswordInvalid").ToString(), Constants.PasswordCharactersMin, Constants.PasswordCharactersMax);

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
        }

        internal void CreateButtons()
        {
            CreateButton = AddButton(Localization.Get("LobbyCreation", "CreateButton"), CreatePrompt);
            NameButton = AddButton(Localization.Get("LobbyCreation", "LobbyName"), NamePrompt);
            MaxPlayersButton = AddButton(Localization.Get("LobbyCreation", "MaxPlayers"), MaxPlayersPrompt);
            PasswordButton = AddButton(Localization.Get("LobbyCreation", "Password"), PasswordPrompt);
            GoalButton = AddButton(Localization.Get("LobbyCreation", "Goal"), UIManager.LobbyGoalWindow);
            PlayTypeButton = AddButton(Localization.Get("LobbyCreation", "PlayType"), UIManager.LobbyPlayTypeWindow);
            ChartSelectionButton = AddButton(Localization.Get("LobbyCreation", "ChartSelection"), UIManager.LobbyChartSelectionWindow);
            AddReturnButton();

            UpdateDescription();
        }

        private async Task OnCreate(BaseWindow window)
        {
            if (CreatePrompt.Result != true) 
            {
                Window.Show();
                return;
            };

            UIManager.Debounce = true;

            bool success = await LobbyManager.CreateLobby(MaxPlayersField,GoalField,PlayTypeField,ChartSelectionField,NameField,PasswordField);

            Main.Dispatcher.Enqueue(() =>
            {
                UIManager.Debounce = false;

                if (success) _ = UIManager.OpenLobbyWindow(LobbyManager.LocalLobby);
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
            } else
            {
                PasswordField = null;
                if (!PasswordPrompt.Result.IsNullOrWhitespace()) PopupUtils.ShowInfo(InvalidPasswordMsg);
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
                    Constants.ChartSelectionColors[ChartSelectionField], ChartSelectionField
                 )
            );

            foreach (ForumObject button in ButtonsWindows.Keys)
            {
                button.Contents = MainDescription;
            }
        }
    }
}
