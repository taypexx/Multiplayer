using Multiplayer.Data;
using Multiplayer.Managers;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI
{
    internal sealed class FriendsWindow : BaseMultiplayerWindow
    {
        private Dictionary<int, Player> FriendIndexes;

        internal FriendsWindow() : base(UIManager.ProfileWindow)
        {
            FriendIndexes = new();
            AddReturnButton();

            Window.OnInternalShow += OnShow;
        }

        private void OnShow(PopupLib.UI.Windows.Abstract.BaseWindow window)
        {
            RemoveAllButtons(true);
            FriendIndexes.Clear();

            var player = UIManager.ProfileWindow.Player;
            if (player == null) return;

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
