using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI;
using UnityEngine.Events;

namespace Multiplayer.UI.NavigationButtons
{
    internal sealed class PlayButton : BaseNavigationButton
    {
        private static void Click()
        {
            if (!LobbyManager.IsInLobby) 
            {
                PopupUtils.ShowInfo(Localization.Get("Lobby", "NoLobby"));
                return;
            }
            if (LobbyManager.LocalLobby.Host != PlayerManager.LocalPlayer)
            {
                PopupUtils.ShowInfo(Localization.Get("Lobby", "NotHost"));
                return;
            }
            UIManager.PlayConfirmPrompt.Show();
        }
        internal PlayButton() : base("Play.png", 2, false, "BtnMultiplayerPlay")
        {
            ButtonAction = (UnityAction)new Action(Click);
            Create();
        }
    }
}
