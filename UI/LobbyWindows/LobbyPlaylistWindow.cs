using LocalizeLib;
using Multiplayer.Data.Lobbies;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class LobbyPlaylistWindow : BaseMultiplayerWindow
    {
        private LocalString MainDescription = new();
        private Dictionary<ForumObject, PlaylistEntry> ButtonsEntries;

        internal LobbyPlaylistWindow() : base(Localization.Get("Lobby", "PlaylistTitle"), UIManager.LobbyWindow, "Lobby.png")
        {
            ButtonsEntries = new();
        }

        internal void Update(Lobby lobby)
        {
            int prevEntries = ButtonsEntries.Count;

            ButtonsEntries.Clear();
            RemoveAllButtons();

            MainDescription = new(String.Format(
                Localization.Get("Lobby", "PlaylistDescription").ToString(),
                Constants.Yellow,
                lobby.Playlist.Count,
                lobby.PlaylistSize
            ));

            if (lobby.Playlist.Count > 0)
            {
                foreach (PlaylistEntry entry in lobby.Playlist)
                {
                    ForumObject button = AddButton((LocalString)ChartManager.GetNiceChartName(entry.MusicInfo, entry.Difficulty), null, MainDescription);
                    ButtonsEntries.Add(button, entry);
                }
            }
            else AddEmptyButton(MainDescription);

            if (Window.Activated && prevEntries != lobby.Playlist.Count) OnRefresh();
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];
            if (ButtonsEntries.TryGetValue(button, out var entry))
            {
                UIManager.JumpToChart(entry.MusicInfo.uid);
            }
        }
    }
}
