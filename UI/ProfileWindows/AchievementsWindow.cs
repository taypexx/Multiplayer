using LocalizeLib;
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
            Window.OnInternalShow += OnShow;
        }

        /// <summary>
        /// Updates the window to show the achievements of a <see cref="Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> whose achievements will show.</param>
        internal void Update(Player player)
        {
            RemoveAllButtons();

            if (player.MultiplayerStats.Achievements.Count > 0)
            {
                foreach ((DateTime date, Achievement achievement) in player.MultiplayerStats.Achievements)
                {
                    var color = Constants.AchievmentDifficultyColors[achievement.Difficulty];
                    AddButton((LocalString)$"<color={color}>{achievement.Name}</color>", null, new(
                        $"<color={color}>({achievement.Difficulty.ToString()})</color>\n" +
                        $"{achievement.Description}\n\n" +
                        $"[ {Localization.Get("Achievements", "AchievedOn")} ]: <color={Constants.Yellow}>{date.ToLocalTime()}</color>"
                    ));
                }
            }
            else AddEmptyButton(Localization.Get("ProfileWindow", "EmptyAchievements" + (player == PlayerManager.LocalPlayer ? "Local" : string.Empty)));
        }

        protected override void OnButtonClick(IListWindow _, int objectIndex)
        {
            return;
        }
    }
}
