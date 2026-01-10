using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using UnityEngine.Events;

namespace Multiplayer.UI.NavigationButtons
{
    internal sealed class LobbyButton : BaseNavigationButton
    {
        private static void Click()
        {
            if (!Client.Connected)
            {
                UIManager.MainMenu.Open(UIManager.LobbiesWindow);
                return;
            }
            if (LobbyManager.IsInLobby)
            {
                _ = UIManager.OpenLobbyWindow();
            } 
            else UIManager.LobbiesWindow.Window.Show();
        }

        internal LobbyButton() : base("People.png", 2, true, "BtnMultiplayerLobby")
        {
            ButtonAction = (UnityAction)new Action(Click);
            Create();
        }
    }
}
