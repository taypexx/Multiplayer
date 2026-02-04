using Il2Cpp;
using LocalizeLib;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI
{
    internal sealed class MainMenu : BaseMultiplayerWindow
    {
        private ForumObject MyProfileButton;
        private ForumObject LobbiesButton;
        private ForumObject CompetitiveButton;
        private ForumObject SettingsButton;
        private ForumObject FAQButton;
        private ForumObject CreditsButton;

        private static LocalString MainDescription;
        private static LocalString Credits;

        internal MainMenu() : base(Localization.Get("MainMenu", "Title"), null, "MainMenu.png")
        {
            MainDescription = Localization.Get("MainMenu", "Description");
            Credits = new(string.Format("———| DEVELOPMENT |———\n\n<color=f542adff>taypexx</color> — Mod & backend development\n<color=f542adff>7OU</color> — Backend development\n<color=1eff00ff>PBalint817</color> — Additional libraries\n<color=fff700ff>???</color> — Traditional Chinese translation\n<color=fff700ff>???</color> — Simplified Chinese translation\n<color=fff700ff>???</color> — Korean translation\n<color=fff700ff>???</color> — Japanese translation\n\n———| TESTER TEAM |———\n\n{0}",Constants.Testers));
        }

        internal void CreateButtons()
        {
            MyProfileButton = AddButton(Localization.Get("MainMenu", "MyProfile"), null, MainDescription);
            LobbiesButton = AddButton(Localization.Get("MainMenu","Lobbies"), null, MainDescription);
            CompetitiveButton = AddButton(Localization.Get("MainMenu", "Competitive"), null, MainDescription);
            SettingsButton = AddButton(Localization.Get("MainMenu", "Settings"), UIManager.SettingsWindow, MainDescription);
            FAQButton = AddButton(Localization.Get("MainMenu", "FAQ"), null, MainDescription);
            CreditsButton = AddButton(Localization.Get("MainMenu", "CreditsTitle"), null, Credits);
        }

        /// <summary>
        /// Updates the <see cref="LobbiesButton"/> and changes its title.
        /// </summary>
        internal void UpdateLobbiesButton()
        {
            LobbiesButton.Titles = LobbyManager.IsInLobby ? Localization.Get("MainMenu", "MyLobby") : Localization.Get("MainMenu", "Lobbies");
        }

        /// <summary>
        /// Opens the main menu if connected to the server, otherwise tries to connect first.
        /// </summary>
        internal void Open(BaseMultiplayerWindow windowToOpen = null)
        {
            if (UIManager.Debounce) return;

            if (Client.Connected)
            {
                if (PlayerManager.LocalPlayer is null) return;
                if (PlayerManager.LocalPlayer.MultiplayerStats.Banned)
                {
                    UIManager.WarnNotification(Localization.Get("MainMenu", "LocalPlayerBanned"));
                }
                else
                {
                    if (windowToOpen is null) windowToOpen = this;
                    windowToOpen.Window.Show();
                }
            }
            else _ = Client.Connect();
        }

        protected override void OnShow(BaseWindow window)
        {
            base.OnShow(window);
            UIManager.ProfileWindow.ReturnWindow = this;
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == CreditsButton) return;
            base.OnButtonClick(window, objectIndex);

            if (button == MyProfileButton)
            {
                _ = UIManager.OpenProfileWindow(PlayerManager.LocalPlayer, false);
            }
            else if (button == LobbiesButton)
            {
                if (LobbyManager.IsInLobby) 
                {
                    _ = UIManager.OpenLobbyWindow();
                } else
                {
                    UIManager.LobbiesWindow.Window.Show();
                }
            } 
            else if (button == CompetitiveButton)
            {
                PopupUtils.ShowInfo(Localization.Get("Global", "ComingSoon"));
                Window.Show();
            }
            else if (button == FAQButton)
            {
                Utilities.OpenBrowserLink($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/faq");
            }
        }
    }
}
