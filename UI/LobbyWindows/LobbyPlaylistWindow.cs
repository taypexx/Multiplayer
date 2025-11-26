using LocalizeLib;
using Multiplayer.Data;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class LobbyPlaylistWindow : BaseMultiplayerWindow
    {
        private LocalString MainDescription => Localization.Get("Lobby", "PlaylistDescription");
        private Dictionary<ForumObject, PlaylistEntry> ButtonsEntries;

        internal LobbyPlaylistWindow() : base(Localization.Get("Lobby", "PlaylistTitle"), UIManager.LobbyWindow, "Lobbies.png")
        {
            AddReturnButton(MainDescription);
            ButtonsEntries = new();
        }

        internal void Update(Lobby lobby)
        {
            RemoveAllButtons(true);
            ButtonsEntries.Clear();

            foreach (PlaylistEntry entry in lobby.Playlist)
            {
                ForumObject button =  AddButton(new(String.Format(
                    "{0} {1}★", 
                    entry.MusicInfo.name,
                    entry.MusicInfo.GetMusicLevelStringByDiff(entry.Difficulty)
                )), null, MainDescription);
                ButtonsEntries.Add(button, entry);
            }
        }

        internal override void OnButtonClick(IListWindow window, int objectIndex)
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
