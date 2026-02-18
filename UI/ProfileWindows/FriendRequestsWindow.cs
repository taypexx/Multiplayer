using LocalizeLib;
using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.ProfileWindows
{
    internal sealed class FriendRequestsWindow : BaseMultiplayerWindow
    {
        private Dictionary<ForumObject, Player> ButtonsPlayers;

        internal FriendRequestsWindow() : base(Localization.Get("ProfileWindow", "FriendRequests"), UIManager.ProfileWindow, "Friends.png")
        {
            ButtonsPlayers = new();
        }

        /// <summary>
        /// Updates the window to show friend requests of a local player.
        /// </summary>
        internal async Task Update()
        {
            var localStats = PlayerManager.LocalPlayer.MultiplayerStats;
            if (!localStats.FriendRequestsCached)
            {
                await localStats.CacheFriendRequests();
            }

            Main.Dispatch(() =>
            {
                RemoveAllButtons();
                ButtonsPlayers.Clear();

                if (localStats.FriendRequests.Count > 0)
                {
                    foreach (string playerUid in localStats.FriendRequests)
                    {
                        Player otherPlayer = PlayerManager.GetCachedPlayer(playerUid);
                        ForumObject button = AddButton((LocalString)otherPlayer.MultiplayerStats.Name);
                        ButtonsPlayers.Add(button, otherPlayer);
                    }
                }
                else AddEmptyButton(Localization.Get("ProfileWindow", "EmptyFriendRequests"));
            });
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            if (ButtonsPlayers.TryGetValue(Window.ForumObjects[objectIndex], out Player otherPlayer))
            {
                _ = UIManager.OpenProfileWindow(otherPlayer);
            }
        }
    }
}
