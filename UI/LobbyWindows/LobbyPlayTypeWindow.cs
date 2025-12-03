using LocalizeLib;
using Multiplayer.Data.Lobbies;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class LobbyPlayTypeWindow : BaseMultiplayerWindow
    {
        internal LobbyPlayType Value { get; private set; } = LobbyPlayType.VanillaOnly;

        private ForumObject AllButton;
        private ForumObject VanillaOnlyButton;
        private ForumObject CustomOnlyButton;

        private Dictionary<ForumObject, LobbyPlayType> PlayTypeValues;
        private LocalString MainDescription => Localization.Get("LobbyCreation", "PlayTypeDescription");

        internal LobbyPlayTypeWindow() : base(Localization.Get("LobbyCreation", "PlayType"), UIManager.LobbyCreationWindow, "Lobbies.png")
        {
            AddReturnButton(MainDescription);
            AllButton = AddButton(Localization.Get("Lobby", "All"), null, MainDescription);
            VanillaOnlyButton = AddButton(Localization.Get("Lobby", "VanillaOnly"), null, MainDescription);
            CustomOnlyButton = AddButton(Localization.Get("Lobby", "CustomOnly"), null, MainDescription);

            PlayTypeValues = new()
            {
                [AllButton] = LobbyPlayType.All,
                [VanillaOnlyButton] = LobbyPlayType.VanillaOnly,
                [CustomOnlyButton] = LobbyPlayType.CustomOnly
            };
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];
            if (button == ReturnButton) return;

            // REMOVE LATER
            if (button != VanillaOnlyButton)
            {
                PopupUtils.ShowInfo(Localization.Get("Global", "ComingSoon"));
                Window.Show();
                return;
            }

            Value = PlayTypeValues[button];

            UIManager.LobbyCreationWindow.UpdateDescription();
            UIManager.LobbyCreationWindow.Window.Show();
        }
    }
}
