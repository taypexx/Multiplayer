using LocalizeLib;
using Multiplayer.Managers;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI
{
    internal sealed class LobbiesWindow : BaseMultiplayerWindow
    {
        private ForumObject PublicLobbiesButton;
        private ForumObject PrivateLobbyButton;
        private ForumObject CreateLobbyButton;

        private static LocalString MainDescription => Localization.Get("Lobbies", "Description");

        internal LobbiesWindow() : base(Localization.Get("Lobbies", "Title"), UIManager.MainMenu, "Lobbies.png")
        {
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

        internal override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];
            if (button == PublicLobbiesButton)
            {
                OpenPublicLobbies();
            }
        }
    }
}