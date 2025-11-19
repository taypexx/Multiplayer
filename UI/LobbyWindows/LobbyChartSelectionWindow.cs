using LocalizeLib;
using Multiplayer.Data.LobbyEnums;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class LobbyChartSelectionWindow : BaseMultiplayerWindow
    {
        internal LobbyChartSelection Value { get; private set; } = LobbyChartSelection.HostOnly;

        private ForumObject HostOnlyButton;
        private ForumObject PlaylistButton;
        private ForumObject RandomButton;

        private Dictionary<ForumObject, LobbyChartSelection> ChartSelectionValues;
        private LocalString MainDescription => Localization.Get("LobbyCreation", "ChartSelectionDescription");

        internal LobbyChartSelectionWindow() : base(Localization.Get("LobbyCreation", "ChartSelection"), UIManager.LobbyCreationWindow, "Lobbies.png")
        {
            AddReturnButton(MainDescription);
            HostOnlyButton = AddButton(Localization.Get("LobbyCreation", "HostOnly"), null, MainDescription);
            PlaylistButton = AddButton(Localization.Get("LobbyCreation", "Playlist"), null, MainDescription);
            RandomButton = AddButton(Localization.Get("LobbyCreation", "Random"), null, MainDescription);

            ChartSelectionValues = new()
            {
                [HostOnlyButton] = LobbyChartSelection.HostOnly,
                [PlaylistButton] = LobbyChartSelection.Playlist,
                [RandomButton] = LobbyChartSelection.Random
            };
        }

        internal override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];
            if (button == ReturnButton) return;

            Value = ChartSelectionValues[button];

            UIManager.LobbyCreationWindow.UpdateDescription();
            UIManager.LobbyCreationWindow.Window.Show();
        }
    }
}
