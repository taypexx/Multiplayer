using Multiplayer.Data;
using Multiplayer.Managers;
using PopupLib.UI.Components;

namespace Multiplayer.UI
{
    internal sealed class MainMenu : BaseMultiplayerWindow
    {
        private ForumObject MyProfileButton;

        internal MainMenu() : base(Localization.Get("MainMenu", "Title"))
        {
            //Window.OnInternalShow += OnShow;
        }

        internal void CreateButtons()
        {
            MyProfileButton = AddButton(Localization.Get("MainMenu", "MyProfile"), UIManager.ProfileWindow, null, "fishingcat.jpg");
            AddReturnButton(null,"gemi.jpg");
        }

        internal override void OnShow(PopupLib.UI.Windows.Abstract.BaseWindow window)
        {
            base.OnShow(window);
            UIManager.ProfileWindow.Update(PlayerManager.LocalPlayer);
        }
    }
}
