using LocalizeLib;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.ProfileWindows
{
    internal sealed class FriendRequestsWindow : BaseMultiplayerWindow
    {
        private Dictionary<ForumObject, string> ButtonsUids;

        internal FriendRequestsWindow() : base(Localization.Get("ProfileWindow", "FriendRequests"), UIManager.ProfileWindow, "Friends.png")
        {
            ButtonsUids = new();
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
                ForumObject button = AddButton((LocalString)playerName);
                ButtonsUids.Add(button, playerUid);
            }
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            if (ButtonsUids.TryGetValue(Window.ForumObjects[objectIndex], out string playerUid))
            {
                _ = UIManager.OpenProfileWindow(playerUid);
            }
        }
    }
}
