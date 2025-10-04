using Multiplayer.Data;
using Multiplayer.Managers;
using PopupLib.UI.Components;

namespace Multiplayer.UI
{
    internal sealed class MainMenu : BaseMultiplayerWindow
    {
        private ForumObject MyProfileButton;

        internal MainMenu() 
        {
            Window.OnInternalShow += OnShow;
        }

        internal void CreateButtons()
        {
            MyProfileButton = AddButton(Localization.Get("MainMenu", "MyProfile"), UIManager.ProfileWindow, null, "fishingcat.jpg");
            AddReturnButton();
        }

        private void OnShow(PopupLib.UI.Windows.Abstract.BaseWindow window)
        {
            UIManager.ProfileWindow.Update(PlayerManager.LocalPlayer);
        }
    }
}
