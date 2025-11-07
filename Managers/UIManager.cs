using LocalizeLib;
using Multiplayer.UI;
using PopupLib.UI;
using PopupLib.UI.Windows;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.Managers
{
    internal static class UIManager
    {
        internal static bool Debounce = false;
        internal static Text WindowTitle { get; private set; }
        private static MessageWindow Warning;

        internal static MainMenu MainMenu { get; private set; }
        internal static MainMenuOpenButton MainMenuOpenButton { get; private set; }

        internal static ProfileWindow ProfileWindow { get; private set; }

        internal static FriendsWindow FriendsWindow { get; private set; }
        internal static AchievementsWindow AchievementsWindow { get; private set; }

        internal static void WarnNotification(LocalString warning)
        {
            Warning.Text = warning;
            Warning.Show();
        }

        internal static void OpenMainMenu()
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
                    _ = Client.Connect().ContinueWith(t =>
                    {
                        Main.Dispatcher.Enqueue(() =>
                        {
                            if (Client.IsConnected)
                            {
                                AchievementManager.Init();
                                PlayerManager.Init();
                            }

                            Debounce = false;
                            OpenMainMenu();
                        });
                    });
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

            var windowTitleGo = GameObject.Instantiate(
                GameObject.Find("UI/Forward/Tips/PnlAchievementsTips/TxtTittle"),
                GameObject.Find("UI/Forward/Tips/PnlBulletinNew").transform
            );
            windowTitleGo.transform.localPosition = new(0f, 320f, 0f);
            WindowTitle = windowTitleGo.GetComponent<Text>();

            Warning = new(new(), Localization.Get("Warning","Title"))
            {
                AutoReset = true
            };

            MainMenu = new();
            MainMenuOpenButton = new();

            ProfileWindow = new();

            FriendsWindow = new();
            AchievementsWindow = new();

            ProfileWindow.CreateButtons();
            MainMenu.CreateButtons();
        }
    }
}
