using LocalizeLib;
using Multiplayer.Data.LobbyEnums;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class LobbyGoalWindow : BaseMultiplayerWindow
    {
        internal LobbyGoal Value { get; private set; } = LobbyGoal.Accuracy;

        private ForumObject AccuracyButton;
        private ForumObject ScoreButton;
        private ForumObject CustomButton;

        private Dictionary<ForumObject, LobbyGoal> GoalValues;
        private LocalString MainDescription => Localization.Get("LobbyCreation", "GoalDescription");

        internal LobbyGoalWindow() : base(Localization.Get("LobbyCreation", "Goal"), UIManager.LobbyCreationWindow, "Lobbies.png")
        {
            AddReturnButton(MainDescription);
            AccuracyButton = AddButton(Localization.Get("Lobby", "Accuracy"), null, MainDescription);
            ScoreButton = AddButton(Localization.Get("Lobby", "Score"), null, MainDescription);
            CustomButton = AddButton(Localization.Get("Lobby", "Custom"), null, MainDescription);

            GoalValues = new()
            {
                [AccuracyButton] = LobbyGoal.Accuracy,
                [ScoreButton] = LobbyGoal.Score,
                [CustomButton] = LobbyGoal.Custom
            };
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];
            if (button == ReturnButton) return;

            Value = GoalValues[button];

            UIManager.LobbyCreationWindow.UpdateDescription();
            UIManager.LobbyCreationWindow.Window.Show();
        }
    }
}
