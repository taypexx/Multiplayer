using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.ProfileWindows
{
    internal sealed class AchievementsWindow : BaseMultiplayerWindow
    {
        internal AchievementsWindow() : base(Localization.Get("Achievements", "Title"), UIManager.ProfileWindow, "Achievements.png")
        {
            AddReturnButton();

            Window.OnInternalShow += OnShow;
        }

        /// <summary>
        /// Updates the window to show the achievements of a <see cref="Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> whose achievements will show.</param>
        internal void Update(Player player)
        {
            RemoveAllButtons(true);

            foreach ((DateTime date, Achievement achievement) in player.MultiplayerStats.Achievements)
            {
                AddButton(achievement.Name, null, new(
                    $"{achievement.Description}\n\n" +
                    $"[ {Localization.Get("Achievements", "AchievedOn")} ]: <color=fff700ff>{date.ToLocalTime()}</color>"
                ));
            }
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            if (Window.ForumObjects[objectIndex] == ReturnButton) base.OnButtonClick(window, objectIndex);
        }
    }
}
