using Il2CppSirenix.Serialization.Utilities;
using LocalizeLib;
using Multiplayer.Data.LobbyEnums;
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
        private LocalString SetMsg = Localization.Get("LobbyCreation", "Set");
        private string InvalidNameMsg = string.Format(Localization.Get("LobbyCreation", "LobbyNameInvalid").ToString(), NameCharactersMin, NameCharactersMax);
        private string InvalidMaxPlayersMsg = string.Format(Localization.Get("LobbyCreation", "MaxPlayersInvalid").ToString(), PlayersMin, PlayersMax);
        private string InvalidPasswordMsg = string.Format(Localization.Get("LobbyCreation", "PasswordInvalid").ToString(), PasswordCharactersMin, PasswordCharactersMax);

        internal const int PlayersMin = 2;
        internal const int PlayersMax = 10;
        internal const int NameCharactersMin = 3;
        internal const int NameCharactersMax = 16;
        internal const int PasswordCharactersMin = 4;
        internal const int PasswordCharactersMax = 16;

        private Dictionary<LobbyGoal, string> GoalColors = new()
        {
            [LobbyGoal.Accuracy] = Constants.Yellow,
            [LobbyGoal.Score] = Constants.Pink,
            [LobbyGoal.Custom] = Constants.Blue,
        };
        private Dictionary<LobbyPlayType, string> PlayTypeColors = new()
        {
            [LobbyPlayType.All] = Constants.Yellow,
            [LobbyPlayType.VanillaOnly] = Constants.Blue,
            [LobbyPlayType.CustomOnly] = Constants.Pink,
        };
        private Dictionary<LobbyChartSelection, string> ChartSelectionColors = new()
        {
            [LobbyChartSelection.HostOnly] = Constants.Yellow,
            [LobbyChartSelection.Playlist] = Constants.Pink,
            [LobbyChartSelection.Random] = Constants.Blue,
        };

        internal LobbyCreationWindow() : base(Localization.Get("LobbyCreation", "Title"), UIManager.LobbiesWindow, "Lobbies.png")
        {
            CreatePrompt = new(Localization.Get("LobbyCreation", "CreateConfirm"), Localization.Get("LobbyCreation", "Create"));
            CreatePrompt.AutoReset = true;
            CreatePrompt.OnCompletion += OnCreate;

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

        private void OnCreate(BaseWindow window)
        {
            if (CreatePrompt.Result != true) 
            {
                Window.Show();
                return;
            };

            // add debounce here
        }

        private void OnNameFieldChanged(BaseWindow window)
        {
            if (Utilities.IsValidString(NamePrompt.Result, NameCharactersMin, NameCharactersMax))
            {
                NameField = NamePrompt.Result;
                PopupUtils.ShowInfo(SetMsg);
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
            int? maxPlayers = Utilities.GetValidNumber(MaxPlayersPrompt.Result, PlayersMin, PlayersMax);
            if (maxPlayers != null)
            {
                MaxPlayersField = (int)maxPlayers;
                PopupUtils.ShowInfo(SetMsg);
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
            if (Utilities.IsValidString(PasswordPrompt.Result, PasswordCharactersMin, PasswordCharactersMax))
            {
                PasswordField = PasswordPrompt.Result;
                PopupUtils.ShowInfo(SetMsg);
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
                    GoalColors[UIManager.LobbyGoalWindow.Value], UIManager.LobbyGoalWindow.Value,
                    PlayTypeColors[UIManager.LobbyPlayTypeWindow.Value], UIManager.LobbyPlayTypeWindow.Value,
                    ChartSelectionColors[UIManager.LobbyChartSelectionWindow.Value], UIManager.LobbyChartSelectionWindow.Value
                 )
            );

            foreach (ForumObject button in ButtonsWindows.Keys)
            {
                button.Contents = MainDescription;
            }
        }
    }
}
