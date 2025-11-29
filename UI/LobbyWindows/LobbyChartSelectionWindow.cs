using LocalizeLib;
using Multiplayer.Data.LobbyEnums;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class LobbyChartSelectionWindow : BaseMultiplayerWindow
    {
        internal LobbyChartSelection Value { get; private set; } = LobbyChartSelection.Playlist;

        private ForumObject HostPlaylistButton;
        private ForumObject PlaylistButton;
        private ForumObject RandomButton;

        private Dictionary<ForumObject, LobbyChartSelection> ChartSelectionValues;
        private LocalString MainDescription => Localization.Get("LobbyCreation", "ChartSelectionDescription");

        internal LobbyChartSelectionWindow() : base(Localization.Get("LobbyCreation", "ChartSelection"), UIManager.LobbyCreationWindow, "Lobbies.png")
        {
            AddReturnButton(MainDescription);
            HostPlaylistButton = AddButton(Localization.Get("Lobby", "HostPlaylist"), null, MainDescription);
            PlaylistButton = AddButton(Localization.Get("Lobby", "Playlist"), null, MainDescription);
            RandomButton = AddButton(Localization.Get("Lobby", "Random"), null, MainDescription);

            ChartSelectionValues = new()
            {
                [HostPlaylistButton] = LobbyChartSelection.HostPlaylist,
                [PlaylistButton] = LobbyChartSelection.Playlist,
                [RandomButton] = LobbyChartSelection.Random
            };
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];
            if (button == ReturnButton) return;

            // REMOVE LATER
            if (button == RandomButton)
            {
                PopupUtils.ShowInfo(Localization.Get("Global", "ComingSoon"));
                Window.Show();
            }

            Value = ChartSelectionValues[button];

            UIManager.LobbyCreationWindow.UpdateDescription();
            UIManager.LobbyCreationWindow.Window.Show();
        }
    }
}
