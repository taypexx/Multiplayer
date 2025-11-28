using Multiplayer.Data;
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
        internal void Update(Player player)
        {
            RemoveAllButtons(true);
            ButtonsFriends.Clear();

            foreach (Player friend in player.MultiplayerStats.Friends)
            {
                ForumObject button = AddButton(friend.MultiplayerStats.NameLocal);
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
