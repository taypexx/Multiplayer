using Multiplayer.Managers;
using Multiplayer.UI.Abstract;
using UnityEngine.Events;

namespace Multiplayer.UI.NavigationButtons
{
    internal sealed class PlaylistButton : BaseNavigationButton
    {
        internal PlaylistButton() : base("Playlist.png", 1, false, "BtnMultiplayerPlaylist")
        {
            ButtonAction = (UnityAction)new Action(UIManager.OnPlaylistButtonClick);
            Create();
        }
    }
}
