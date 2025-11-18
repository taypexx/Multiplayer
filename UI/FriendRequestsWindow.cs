using LocalizeLib;
using Multiplayer.Managers;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI
{
    internal sealed class FriendRequestsWindow : BaseMultiplayerWindow
    {
        private Dictionary<ForumObject, string> ButtonsUids;
        private Dictionary<string, LocalString> CachedNames;

        internal FriendRequestsWindow() : base(Localization.Get("MainMenu", "FriendRequests"), UIManager.MainMenu, "MainMenu.png")
        {
            ButtonsUids = new();
            CachedNames = new();
            AddReturnButton();
        }

        /// <summary>
        /// Updates the window to show friend requests of a local player.
        /// </summary>
        internal void Update()
        {
            RemoveAllButtons(true);
            ButtonsUids.Clear();

            foreach ((string playerUid, string playerName) in PlayerManager.LocalPlayer.MultiplayerStats.FriendRequests)
            {
                LocalString localString;
                if (!CachedNames.TryGetValue(playerName, out localString))
                {
                    localString = new(playerName);
                    CachedNames.Add(playerName, localString);
                }

                ForumObject button = AddButton(localString);
                ButtonsUids.Add(button, playerUid);
            }
        }

        internal override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            if (ButtonsUids.TryGetValue(Window.ForumObjects[objectIndex], out string playerUid))
            {
                OpenProfileWindow(playerUid);
            }
        }
    }
}
