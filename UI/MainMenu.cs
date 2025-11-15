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

        internal MainMenu() : base(Localization.Get("MainMenu", "Title"), null, "MainMenu.png")
        {
            BioWindow = new(Localization.Get("MainMenu", "BioDescription"));
            BioWindow.AutoReset = true;
            BioWindow.OnCompletion += OnBioCompletion;

            PnlHead.onClose += (Action)OnPnlHeadClose;
        }

        internal void CreateButtons()
        {
            MyProfileButton = AddButton(Localization.Get("MainMenu", "MyProfile"), UIManager.ProfileWindow, MainDescription);
            AvatarButton = AddButton(Localization.Get("MainMenu", "Avatar"), PnlHead, MainDescription);
            BioButton = AddButton(Localization.Get("MainMenu", "Bio"), BioWindow, MainDescription);
            FriendRequestsButton = AddButton(Localization.Get("MainMenu", "FriendRequests"), UIManager.FriendRequestsWindow, MainDescription);
            LobbiesButton = AddButton(Localization.Get("MainMenu","Lobbies"), null, MainDescription);
            CompetitiveButton = AddButton(Localization.Get("MainMenu", "Competitive"), null, MainDescription);
            CreditsButton = AddButton(Localization.Get("MainMenu", "CreditsTitle"), null, Localization.Get("MainMenu","Credits"));
            AddReturnButton(MainDescription);
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

        /// <summary>
        /// Opens the main menu if connected to the server, otherwise tries to connect first.
        /// </summary>
        internal void Open()
        {
            if (UIManager.Debounce) return;

            if (Client.Connected)
            {
                if (PlayerManager.LocalPlayer == null) return;
                if (!PlayerManager.LocalPlayer.MultiplayerStats.Banned)
                {
                    if (LobbyManager.LocalLobby == null)
                    {
                        Window.Show();
                    } else
                    {
                        UIManager.LobbyWindow.Window.Show();
                    }
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
                    Client.Connect().ContinueWith(t =>
                    {
                        Main.Dispatcher.Enqueue(() =>
                        {
                            if (Client.Connected) Main.InitConnect();

                            UIManager.Debounce = false;
                            Open();
                        });
                    });
                }
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

        internal override void OnShow(PopupLib.UI.Windows.Abstract.BaseWindow window)
        {
            base.OnShow(window);
            UIManager.ProfileWindow.ReturnWindow = this;
            UIManager.ProfileWindow.Update(PlayerManager.LocalPlayer).ContinueWith(t =>
            {
                Main.Dispatcher.Enqueue(() =>
                {
                    UIManager.FriendRequestsWindow.Update();
                });
            });
        }

        internal override void OnButtonClick(PopupLib.UI.Windows.Interfaces.IListWindow window, int objectIndex)
        {
            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == AvatarButton)
            {
                base.OnButtonClick(window, objectIndex);
                PnlHeadWasOpened = true;
            } else if (button == LobbiesButton)
            {
                Window.ForceClose();
                if (LobbyManager.LocalLobby is null)
                {
                    UIManager.LobbiesWindow.Window.Show();
                } else
                {
                    // Open local lobby
                }
            } else
            {
                base.OnButtonClick(window, objectIndex);
            }
        }
    }
}
