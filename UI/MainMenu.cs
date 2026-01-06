using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.UI.Panels;
using Il2CppSirenix.Serialization.Utilities;
using LocalizeLib;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using PopupLib.UI.Windows.Interfaces;
using UnityEngine;

namespace Multiplayer.UI
{
    internal sealed class MainMenu : BaseMultiplayerWindow
    {
        private ForumObject MyProfileButton;
        private ForumObject AvatarButton;
        private ForumObject BioButton;
        private ForumObject LobbiesButton;
        private ForumObject CompetitiveButton;
        private ForumObject SettingsButton;
        private ForumObject CreditsButton;

        internal InputWindow CodeWindow;
        private InputWindow BioWindow;
        private static PnlHead PnlHead => GameObject.Find("UI/Forward/Tips/PnlHead").GetComponent<PnlHead>();
        private static bool PnlHeadWasOpened = false;

        private static LocalString MainDescription => Localization.Get("MainMenu", "Description");
        private static LocalString Credits;

        internal MainMenu() : base(Localization.Get("MainMenu", "Title"), null, "MainMenu.png")
        {
            CodeWindow = new();
            CodeWindow.AutoReset = true;
            CodeWindow.OnCompletion += (BaseWindow _) => OnCodeEntered();

            BioWindow = new(Localization.Get("MainMenu", "BioDescription"));
            BioWindow.AutoReset = true;
            BioWindow.OnCompletion += OnBioCompletion;

            PnlHead.onClose += (Action)OnPnlHeadClose;

            Credits = new(string.Format("———| DEVELOPMENT |———\n\n<color=f542adff>taypexx</color> — Muse Dash mod development\n<color=f542adff>7OU</color> — Backend development\n<color=1eff00ff>PBalint817</color> — Additional libraries\n<color=fff700ff>???</color> — Traditional Chinese translation\n<color=fff700ff>???</color> — Simplified Chinese translation\n<color=fff700ff>???</color> — Korean translation\n<color=fff700ff>???</color> — Japanese translation\n\n———| TESTER TEAM |———\n\n{0}",Constants.Testers));
        }

        internal void CreateButtons()
        {
            MyProfileButton = AddButton(Localization.Get("MainMenu", "MyProfile"), null, MainDescription);
            AvatarButton = AddButton(Localization.Get("MainMenu", "Avatar"), PnlHead, MainDescription);
            BioButton = AddButton(Localization.Get("MainMenu", "Bio"), BioWindow, MainDescription);
            LobbiesButton = AddButton(Localization.Get("MainMenu","Lobbies"), null, MainDescription);
            CompetitiveButton = AddButton(Localization.Get("MainMenu", "Competitive"), null, MainDescription);
            SettingsButton = AddButton(Localization.Get("MainMenu", "Settings"), null, MainDescription);
            CreditsButton = AddButton(Localization.Get("MainMenu", "CreditsTitle"), null, Credits);
            AddReturnButton(MainDescription);
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
        internal void Open()
        {
            if (UIManager.Debounce) return;

            if (Client.Connected)
            {
                if (PlayerManager.LocalPlayer is null) return;
                if (PlayerManager.LocalPlayer.MultiplayerStats.Banned)
                {
                    UIManager.WarnNotification(Localization.Get("MainMenu", "LocalPlayerBanned"));
                } else
                {
                    if (!LobbyManager.IsInLobby) Window.Show();
                    else _ = UIManager.OpenLobbyWindow(LobbyManager.LocalLobby);
                }
            } else
            {
                if (Client.TriedConnecting)
                {
                    Client.Disconnect();
                    return;
                }

                _ = Client.Connect();
            }
        }

        private async Task OnCodeEntered()
        {
            if (CodeWindow.Result.IsNullOrWhitespace()) return;

            UIManager.Debounce = true;
            await Client.Connect(CodeWindow.Result.ToString());
            UIManager.Debounce = false;
        }

        /// <summary>
        /// Calls every time the bio window gets closed.
        /// </summary>
        private void OnBioCompletion(BaseWindow window)
        {
            Window.Show();
            if (BioWindow.Result.IsNullOrWhitespace()) return;

            if (BioWindow.Result.Length > Constants.BioCharactersMax)
            {
                PopupUtils.ShowInfo(String.Format(Localization.Get("MainMenu", "BioTooLong").ToString(),Constants.BioCharactersMax));
                return;
            }

            PlayerManager.LocalPlayer.MultiplayerStats.Bio = BioWindow.Result;
            PlayerManager.SyncProfile();
        }

        /// <summary>
        /// Calls every time <see cref="Il2CppAssets.Scripts.UI.Panels.PnlHead"/> gets closed.
        /// </summary>
        private void OnPnlHeadClose()
        {
            if (PlayerManager.LocalPlayer is null) return;

            string newAvatarName = "head_" + DataHelper.selectedHeadIndex.ToString();
            if (PlayerManager.LocalPlayer.MultiplayerStats.AvatarName != newAvatarName)
            {
                PlayerManager.LocalPlayer.MultiplayerStats.AvatarName = newAvatarName;
                PlayerManager.SyncProfile();
            }

            if (PnlHeadWasOpened)
            {
                PnlHeadWasOpened = false;
                Window.Show();
            }
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
            else if (button == AvatarButton)
            {
                PnlHeadWasOpened = true;
            } 
            else if (button == LobbiesButton)
            {
                if (LobbyManager.IsInLobby) 
                {
                    _ = UIManager.OpenLobbyWindow(LobbyManager.LocalLobby);
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
            else if (button == SettingsButton)
            {
                UIManager.SettingsWindow.Window.Show();
            }
        }
    }
}
