using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.UI.Panels;
using Il2CppSirenix.Serialization.Utilities;
using LocalizeLib;
using Multiplayer.Managers;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using UnityEngine;

namespace Multiplayer.UI
{
    internal sealed class MainMenu : BaseMultiplayerWindow
    {
        private ForumObject MyProfileButton;
        private ForumObject AvatarButton;
        private ForumObject BioButton;
        private ForumObject FriendRequestsButton;
        private ForumObject LobbiesButton;
        private ForumObject CompetitiveButton;
        private ForumObject CreditsButton;

        private InputWindow BioWindow;
        private static PnlHead PnlHead => GameObject.Find("UI/Forward/Tips/PnlHead").GetComponent<PnlHead>();
        private static bool PnlHeadWasOpened = false;

        private static LocalString MainDescription => Localization.Get("MainMenu", "Description");
        private static LocalString Credits;

        internal MainMenu() : base(Localization.Get("MainMenu", "Title"), null, "MainMenu.png")
        {
            BioWindow = new(Localization.Get("MainMenu", "BioDescription"));
            BioWindow.AutoReset = true;
            BioWindow.OnCompletion += OnBioCompletion;

            PnlHead.onClose += (Action)OnPnlHeadClose;

            Credits = new(string.Format("———| DEVELOPMENT |———\n\n<color=f542adff>taypexx</color> — Muse Dash mod development\n<color=f542adff>7OU</color> — Backend development\n<color=1eff00ff>PBalint817</color> — Additional libraries\n<color=fff700ff>???</color> — Traditional Chinese translation\n<color=fff700ff>???</color> — Simplified Chinese translation\n<color=fff700ff>???</color> — Korean translation\n<color=fff700ff>???</color> — Japanese translation\n\n———| TESTER TEAM |———\n\n{0}",Main.Testers));
        }

        internal void CreateButtons()
        {
            MyProfileButton = AddButton(Localization.Get("MainMenu", "MyProfile"), UIManager.ProfileWindow, MainDescription);
            AvatarButton = AddButton(Localization.Get("MainMenu", "Avatar"), PnlHead, MainDescription);
            BioButton = AddButton(Localization.Get("MainMenu", "Bio"), BioWindow, MainDescription);
            FriendRequestsButton = AddButton(Localization.Get("MainMenu", "FriendRequests"), MainDescription);
            LobbiesButton = AddButton(Localization.Get("MainMenu","Lobbies"), null, MainDescription);
            CompetitiveButton = AddButton(Localization.Get("MainMenu", "Competitive"), null, MainDescription);
            CreditsButton = AddButton(Localization.Get("MainMenu", "CreditsTitle"), null, Credits);
            AddReturnButton(MainDescription);
        }

        /// <summary>
        /// Tries to connect to the server.
        /// </summary>
        private async void TryConnect()
        {
            UIManager.Debounce = true;

            await Client.Connect();

            Main.Dispatcher.Enqueue(() =>
            {
                if (Client.Connected) Main.InitConnect();

                UIManager.Debounce = false;
                Open();
            });
        }

        /// <summary>
        /// Updates and opens the <see cref="FriendRequestsWindow"/>.
        /// </summary>
        private async void OpenFriendRequests()
        {
            UIManager.Debounce = true;

            await PlayerManager.LocalPlayer.Update();

            Main.Dispatcher.Enqueue(() =>
            {
                UIManager.Debounce = false;
                UIManager.FriendRequestsWindow.Window.Show();
            });
        }

        /// <summary>
        /// Opens the main menu if connected to the server, otherwise tries to connect first.
        /// </summary>
        internal void Open()
        {
            if (UIManager.Debounce) return;

            if (Client.Connected)
            {
                if (PlayerManager.LocalPlayer == null) return;
                if (PlayerManager.LocalPlayer.MultiplayerStats.Banned)
                {
                    UIManager.WarnNotification(Localization.Get("MainMenu", "LocalPlayerBanned"));
                } else
                {
                    if (LobbyManager.LocalLobby == null) Window.Show();
                    else OpenLobbyWindow(LobbyManager.LocalLobby);
                }
            } else
            {
                if (Client.TriedConnecting) Client.Disconnect();
                else TryConnect();
            }
        }

        /// <summary>
        /// Calls every time the bio window gets closed.
        /// </summary>
        internal void OnBioCompletion(PopupLib.UI.Windows.Abstract.BaseWindow window)
        {
            Window.Show();
            if (BioWindow.Result.IsNullOrWhitespace()) return;

            PlayerManager.LocalPlayer.MultiplayerStats.Bio = BioWindow.Result;
            PlayerManager.SyncLocalPlayer();
        }

        /// <summary>
        /// Calls every time <see cref="Il2CppAssets.Scripts.UI.Panels.PnlHead"/> gets closed.
        /// </summary>
        internal void OnPnlHeadClose()
        {
            if (PlayerManager.LocalPlayer is null) return;

            string newAvatarName = "head_" + DataHelper.selectedHeadIndex.ToString();
            if (PlayerManager.LocalPlayer.MultiplayerStats.AvatarName != newAvatarName)
            {
                PlayerManager.LocalPlayer.MultiplayerStats.AvatarName = newAvatarName;
                PlayerManager.SyncLocalPlayer();
            }

            if (PnlHeadWasOpened)
            {
                PnlHeadWasOpened = false;
                Open();
            }
        }

        internal override void OnShow(PopupLib.UI.Windows.Abstract.BaseWindow window)
        {
            base.OnShow(window);
            UIManager.ProfileWindow.ReturnWindow = this;
            _ = UIManager.ProfileWindow.Update(PlayerManager.LocalPlayer,false);
        }

        internal override void OnButtonClick(PopupLib.UI.Windows.Interfaces.IListWindow window, int objectIndex)
        {
            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == CreditsButton) return;
            base.OnButtonClick(window, objectIndex);

            if (button == AvatarButton)
            {
                PnlHeadWasOpened = true;
            } else if (button == LobbiesButton)
            {
                if (LobbyManager.LocalLobby is null)
                {
                    UIManager.LobbiesWindow.Window.Show();
                } else
                {
                    OpenLobbyWindow(LobbyManager.LocalLobby);
                }
            } else if (button == FriendRequestsButton)
            {
                OpenFriendRequests();
            }
        }
    }
}
