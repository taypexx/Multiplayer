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
            PublicLobbiesButton = AddButton(Localization.Get("Lobbies", "PublicButton"), UIManager.PublicLobbiesWindow, MainDescription);
            PrivateLobbyButton = AddButton(Localization.Get("Lobbies", "PrivateButton"), null, MainDescription);
            CreateLobbyButton = AddButton(Localization.Get("Lobbies", "CreateButton"), null, MainDescription);
            AddReturnButton(MainDescription);
        }

        internal override void OnButtonClick(IListWindow window, int objectIndex)
        {
            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == PublicLobbiesButton)
            {
                UIManager.PublicLobbiesWindow.Update().ContinueWith(t =>
                {
                    Main.Dispatcher.Enqueue(() =>
                    {
                        base.OnButtonClick(window, objectIndex);
                    });
                });
            } else
            {
                base.OnButtonClick(window, objectIndex);
            }
        }
    }
}