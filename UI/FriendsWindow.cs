using Multiplayer.Data;
using Multiplayer.Managers;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI
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
                ForumObject button = AddButton(friend.MultiplayerStats.NameLocal, UIManager.ProfileWindow);
                ButtonsFriends.Add(button, friend);
            }
        }

        internal override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            // Should come after the base method or else it desyncs and breaks :bleh:
            if (ButtonsFriends.TryGetValue(Window.ForumObjects[objectIndex], out Player player))
            {
                UIManager.ProfileWindow.Update(player);
            }
        }
    }
}
