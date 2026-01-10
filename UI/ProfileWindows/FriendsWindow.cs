using LocalizeLib;
using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.ProfileWindows
{
    internal sealed class FriendsWindow : BaseMultiplayerWindow
    {
        private Dictionary<ForumObject, Player> ButtonsFriends;

        internal FriendsWindow() : base(Localization.Get("ProfileWindow", "Friends"), UIManager.ProfileWindow, "Friends.png")
        {
            ButtonsFriends = new();
            AddReturnButton();
        }

        /// <summary>
        /// Updates the window to show friends of a <see cref="Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> whose friends will show.</param>
        internal async Task Update(Player player)
        {
            RemoveAllButtons(true);
            ButtonsFriends.Clear();

            if (!player.MultiplayerStats.FriendsCached)
            {
                await player.MultiplayerStats.CacheFriends();
            }

            // Might crash, keep an eye on it
            foreach (string friendUid in player.MultiplayerStats.Friends)
            {
                Player friend = PlayerManager.GetCachedPlayer(friendUid);
                ForumObject button = AddButton((LocalString)friend.MultiplayerStats.Name);
                ButtonsFriends.Add(button, friend);
            }
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            if (ButtonsFriends.TryGetValue(Window.ForumObjects[objectIndex], out Player player))
            {
                _ = UIManager.OpenProfileWindow(player);
            }
        }
    }
}
