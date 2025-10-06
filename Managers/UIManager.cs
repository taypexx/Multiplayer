using LocalizeLib;
using Multiplayer.UI;
using PopupLib.UI;
using PopupLib.UI.Windows;
using UnityEngine.Events;

namespace Multiplayer.Managers
{
    internal static class UIManager
    {
        private static bool Debounce = false;

        private static MessageWindow Warning;

        internal static MainMenu MainMenu { get; private set; }
        internal static MainMenuOpenButton MainMenuOpenButton { get; private set; }

        internal static ProfileWindow ProfileWindow { get; private set; }

        internal static FriendsWindow FriendsWindow { get; private set; }
        internal static AchievementsWindow AchievementsWindow { get; private set; }
        internal static MoeStatsWindow MoeStatsWindow { get; private set; }
        //internal static HQStatsWindow HQStatsWindow

        internal static void WarnNotification(LocalString warning)
        {
            Warning.Text = warning;
            Warning.Show();
        }

        internal static async void OpenMainMenu()
        {
            if (Debounce) return;

            if (Client.IsConnected)
            {
                if (PlayerManager.LocalPlayer == null) return;
                if (!PlayerManager.LocalPlayer.MultiplayerStats.Banned)
                {
                    MainMenu.Window.Show();
                } else
                {
                    WarnNotification(Localization.Get("MainMenu", "LocalPlayerBanned"));
                }
            } else
            {
                if (Client.TriedConnecting)
                {
                    Client.Disconnect();
                }
                else
                {
                    Debounce = true;
                    PopupUtils.ShowInfoAndLog(Localization.Get("MainMenu", "Connecting"));
                    await Client.Connect();

                    Debounce = false;
                    if (Client.IsConnected)
                    {
                        OpenMainMenu();
                    }
                }
            }
        }

        internal static void InitUISystemMain()
        {
            MainMenuOpenButton.Init();
            MainMenuOpenButton.ButtonComponent.onClick.AddListener((UnityAction)new Action(OpenMainMenu));
        }

        internal static void Init()
        {
            if (MainMenu != null) return;

            Warning = new(new(), Localization.Get("Warning","Title"))
            {
                AutoReset = true
            };

            MainMenu = new();
            MainMenuOpenButton = new();

            ProfileWindow = new();

            FriendsWindow = new();
            AchievementsWindow = new();
            MoeStatsWindow = new();
            //HQStatsWindow = new();

            ProfileWindow.CreateButtons();
            MainMenu.CreateButtons();
        }
    }
}
