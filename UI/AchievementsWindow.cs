using Multiplayer.Data;
using Multiplayer.Managers;

namespace Multiplayer.UI
{
    internal sealed class AchievementsWindow : BaseMultiplayerWindow
    {
        internal AchievementsWindow() : base(Localization.Get("Achievements", "Title"),UIManager.ProfileWindow)
        {
            AddReturnButton();

            Window.OnInternalShow += OnShow;
        }

        internal override void OnShow(PopupLib.UI.Windows.Abstract.BaseWindow window)
        {
            base.OnShow(window);
            AchievementManager.Achieve(0);
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
    }
}
