using Multiplayer.Data;
using Multiplayer.Managers;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI
{
    internal sealed class FriendsWindow : BaseMultiplayerWindow
    {
        private Dictionary<int, Player> FriendIndexes;

        internal FriendsWindow() : base(Localization.Get("ProfileWindow", "Friends"), UIManager.ProfileWindow)
        {
            FriendIndexes = new();
            AddReturnButton();
        }

        /// <summary>
        /// Updates the window to show friends of a <see cref="Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> whose friends will show.</param>
        internal void Update(Player player)
        {
            RemoveAllButtons(true);
            FriendIndexes.Clear();

            foreach (Player friend in player.MultiplayerStats.Friends)
            {
                ForumObject button = AddButton(friend.MultiplayerStats.NameLocal, UIManager.ProfileWindow);
                FriendIndexes.Add(Window.ForumObjects.IndexOf(button), friend);
            }
        }

        internal override void OnButtonClick(IListWindow window, int objectIndex)
        {
            if (FriendIndexes.TryGetValue(objectIndex, out Player player))
            {
                UIManager.ProfileWindow.Update(player);
            }

            base.OnButtonClick(window, objectIndex);
        }
    }
}
