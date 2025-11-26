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
        private Dictionary<int, LocalString> DifficultyNames;
        private Dictionary<ForumObject, PlaylistEntry> ButtonsEntries;

        internal LobbyPlaylistWindow() : base(Localization.Get("Lobby", "PlaylistTitle"), UIManager.LobbyWindow, "Lobbies.png")
        {
            AddReturnButton(MainDescription);

            DifficultyNames = new()
            {
                [1] = Localization.Get("Global", "Easy"),
                [2] = Localization.Get("Global", "Hard"),
                [3] = Localization.Get("Global", "Master"),
                [4] = Localization.Get("Global", "Hidden"),
                [5] = Localization.Get("Global", "Touhou"),
            };
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
                UIManager.PnlStage.SelectAllTagAndJumpToAssginIndex(entry.MusicInfo.uid);
            }
        }
    }
}
