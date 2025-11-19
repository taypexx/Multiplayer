using Il2CppSirenix.Serialization.Utilities;
using LocalizeLib;
using Multiplayer.Data;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class LobbiesWindow : BaseMultiplayerWindow
    {
        private ForumObject PublicLobbiesButton;
        private ForumObject PrivateLobbyButton;
        private ForumObject CreateLobbyButton;

        private InputWindow IDPrompt;

        private static LocalString MainDescription => Localization.Get("Lobbies", "Description");

        internal LobbiesWindow() : base(Localization.Get("Lobbies", "Title"), UIManager.MainMenu, "Lobbies.png")
        {
            IDPrompt = new(Localization.Get("Lobbies", "IDPrompt"));
            IDPrompt.AutoReset = true;
            IDPrompt.OnCompletion += OnIDEnter;
        }

        internal void CreateButtons()
        {
            PublicLobbiesButton = AddButton(Localization.Get("Lobbies", "PublicButton"), null, MainDescription);
            PrivateLobbyButton = AddButton(Localization.Get("Lobbies", "PrivateButton"), null, MainDescription);
            CreateLobbyButton = AddButton(Localization.Get("Lobbies", "CreateButton"), null, MainDescription);
            AddReturnButton(MainDescription);
        }

        /// <summary>
        /// Opens <see cref="PublicLobbiesWindow"/> and displays refreshed public lobbies.
        /// </summary>
        private async void OpenPublicLobbies()
        {
            UIManager.Debounce = true;

            await UIManager.PublicLobbiesWindow.Update();

            Main.Dispatcher.Enqueue(() =>
            {
                UIManager.Debounce = false;
                UIManager.PublicLobbiesWindow.Window.Show();
            });
        }

        private async void OnIDEnter(BaseWindow window)
        {
            int? id = Utilities.GetValidNumber(IDPrompt.Result);
            if (id is null)
            {
                Window.Show();
            }
            else
            {
                UIManager.Debounce = true;

                Lobby lobby = await LobbyManager.GetLobby((int)id, true);

                Main.Dispatcher.Enqueue(() =>
                {
                    UIManager.Debounce = false;
                    if (lobby is null)
                    {
                        PopupUtils.ShowInfoAndLog(Localization.Get("Lobbies", "IncorrectID"));
                        Window.Show();
                    }
                    else OpenLobbyWindow(lobby);
                });
            }
        }

        internal override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);
            if (LobbyManager.LocalLobby != null)
            {
                OpenLobbyWindow(LobbyManager.LocalLobby);
                return;
            }

            ForumObject button = Window.ForumObjects[objectIndex];
            if (button == PublicLobbiesButton)
            {
                OpenPublicLobbies();
            }
            else if (button == PrivateLobbyButton)
            {
                IDPrompt.Show();
            } else if (button == CreateLobbyButton)
            {
                UIManager.LobbyCreationWindow.Window.Show();
            }
        }
    }
}