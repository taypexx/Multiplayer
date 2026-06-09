using LocalizeLib;
using Multiplayer.Data.Lobbies;
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
            IDPrompt.OnCompletion += (window) => _ = OnIDEnter(window);
        }

        internal void CreateButtons()
        {
            PublicLobbiesButton = AddButton(Localization.Get("Lobbies", "PublicButton"), null, MainDescription);
            PrivateLobbyButton = AddButton(Localization.Get("Lobbies", "PrivateButton"), null, MainDescription);
            CreateLobbyButton = AddButton(Localization.Get("Lobbies", "CreateButton"), null, MainDescription);
        }

        /// <summary>
        /// Opens <see cref="PublicLobbiesWindow"/> and displays refreshed public lobbies.
        /// </summary>
        private async Task OpenPublicLobbies()
        {
            UIManager.Debounce = true;

            await UIManager.PublicLobbiesWindow.Update();

            Main.Dispatch(() =>
            {
                UIManager.Debounce = false;
                Window.ForceClose();
                UIManager.PublicLobbiesWindow.Window.Show();
            });
        }

        private async Task OnIDEnter(BaseWindow window)
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

                Main.Dispatch(() =>
                {
                    UIManager.Debounce = false;
                    if (lobby is null)
                    {
                        PopupUtils.ShowInfo(Localization.Get("Lobbies", "IncorrectID"));
                        Window.Show();
                    }
                    else _ = UIManager.OpenLobbyWindow(lobby);
                });
            }
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            if (UIManager.Debounce) return;

            if (LobbyManager.IsInLobby)
            {
                base.OnButtonClick(window, objectIndex);
                _ = UIManager.OpenLobbyWindow();
                return;
            }

            if (objectIndex < 0 || objectIndex >= Window.ForumObjects.Count) return;
            ForumObject button = Window.ForumObjects[objectIndex];
            if (button == PublicLobbiesButton)
            {
                _ = OpenPublicLobbies();
            }
            else if (button == PrivateLobbyButton)
            {
                base.OnButtonClick(window, objectIndex);
                IDPrompt.Show();
            } 
            else if (button == CreateLobbyButton)
            {
                base.OnButtonClick(window, objectIndex);
                UIManager.LobbyCreationWindow.Window.Show();
            }
            else
            {
                base.OnButtonClick(window, objectIndex);
            }
        }
    }
}