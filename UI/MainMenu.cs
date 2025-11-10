using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Managers;
using PopupLib.UI.Components;
using UnityEngine;

namespace Multiplayer.UI
{
    internal sealed class MainMenu : BaseMultiplayerWindow
    {
        private ForumObject MyProfileButton;
        private ForumObject AvatarButton;
        private ForumObject FriendRequestsButton;
        private ForumObject LobbiesButton;
        private ForumObject CompetitiveButton;
        private ForumObject CreditsButton;

        private static PnlHead PnlHead => GameObject.Find("UI/Forward/Tips/PnlHead").GetComponent<PnlHead>();

        internal MainMenu() : base(Localization.Get("MainMenu", "Title"))
        {
        }

        internal void CreateButtons()
        {
            MyProfileButton = AddButton(Localization.Get("MainMenu", "MyProfile"), UIManager.ProfileWindow, null);
            AvatarButton = AddButton(Localization.Get("MainMenu", "Avatar"), PnlHead);
            FriendRequestsButton = AddButton(Localization.Get("MainMenu", "FriendRequests"));
            LobbiesButton = AddButton(Localization.Get("MainMenu","Lobbies"));
            CompetitiveButton = AddButton(Localization.Get("MainMenu", "Competitive"));
            CreditsButton = AddButton(Localization.Get("MainMenu", "CreditsTitle"), null, Localization.Get("MainMenu","Credits"));
            AddReturnButton(null);
        }

        internal void Open()
        {
            if (UIManager.Debounce) return;

            if (Client.Connected)
            {
                if (PlayerManager.LocalPlayer == null) return;
                if (!PlayerManager.LocalPlayer.MultiplayerStats.Banned)
                {
                    Window.Show();
                }
                else
                {
                    UIManager.WarnNotification(Localization.Get("MainMenu", "LocalPlayerBanned"));
                }
            }
            else
            {
                if (Client.TriedConnecting) Client.Disconnect();
                else
                {
                    UIManager.Debounce = true;
                    _ = Client.Connect().ContinueWith(t =>
                    {
                        Main.Dispatcher.Enqueue(() =>
                        {
                            if (Client.Connected)
                            {
                                AchievementManager.Init();
                                PlayerManager.Init();
                            }

                            UIManager.Debounce = false;
                            Open();
                        });
                    });
                }
            }
        }

        internal override void OnShow(PopupLib.UI.Windows.Abstract.BaseWindow window)
        {
            base.OnShow(window);
            UIManager.ProfileWindow.Update(PlayerManager.LocalPlayer);
        }
    }
}
