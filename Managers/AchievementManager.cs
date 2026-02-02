using Il2CppAssets.Scripts.PeroTools.Commons;
using Multiplayer.Data.Players;
using UnityEngine.UI;

namespace Multiplayer.Managers
{
    internal static class AchievementManager
    {
        internal static Dictionary<int,Achievement> Achievements { get; private set; }
        private static Queue<Achievement> QueuedAchievements;

        private static Il2CppAssets.Scripts.GameCore.Managers.AchievementManager PeroAchievementManager => Singleton<Il2CppAssets.Scripts.GameCore.Managers.AchievementManager>.instance;

        /// <summary>
        /// Performs an achievement check and rewards the local <see cref="Player"/> if the conditions are met.
        /// </summary>
        internal static void Check()
        {
            Player localPlayer = PlayerManager.LocalPlayer;
            if (localPlayer is null) return;

            if (localPlayer.MultiplayerStats.ELO >= Ranks.TopRankELO) Achieve(1);
        }

        /// <summary>
        /// Plays the vanilla achievement reward animation for every queued <see cref="Achievement"/>.
        /// </summary>
        internal static void PlayAchievementAnimation()
        {
            if (QueuedAchievements == null) return;

            foreach (var _ in QueuedAchievements)
            {
                PeroAchievementManager.Reward("1-1");
            }

            UIManager.PnlMessage.FinishMessage();

            var layout = UIManager.PnlMessage.layout.transform;
            Task.Run(async () => 
            {
                await Task.Delay(500);
                Main.Dispatcher.Enqueue(() =>
                {
                    for (int i = 0; i < layout.childCount; i++)
                    {
                        Achievement achievement = QueuedAchievements.Dequeue();
                        layout.GetChild(i).Find("TxtDescription").GetComponent<Text>().text =
                            $"<color=#ffca5fff>{achievement.Name}</color>    {achievement.Description}";
                    }
                    QueuedAchievements.Clear();
                });
            });
        }

        /// <summary>
        /// Adds an <see cref="Achievement"/> to the local <see cref="Player"/>'s profile and synchronizes with the server.
        /// </summary>
        /// <param name="achievementId">ID of an <see cref="Achievement"/>.</param>
        /// <param name="instantAnim">(Optional) Whether to play it instantly after getting or queue to play later.</param>
        /// <returns><see langword="true"/> if it was added successfully, otherwise <see langword="false"/>.</returns>
        internal static bool Achieve(int achievementId, bool instantAnim = false)
        {
            Player localPlayer = PlayerManager.LocalPlayer;
            if (localPlayer is null) return false;

            Achievement achievement = Achievements[achievementId];
            if (achievement is null) return false;

            if (localPlayer.MultiplayerStats.Achievements.ContainsValue(achievement)) return false;

            localPlayer.MultiplayerStats.Achievements.Add(DateTime.UtcNow,achievement);
            PlayerManager.SyncAchievements();

            QueuedAchievements.Enqueue(achievement);
            if (instantAnim) PlayAchievementAnimation();

            return true;
        }

        internal static void Init()
        {
            QueuedAchievements = new();

            Achievements = new()
            {
                [0] = new("Welcome!", AchievementDifficulty.Easy),
                [1] = new("Autoplay.dll", AchievementDifficulty.Hard)
            };

            Achieve(0, true);
        }
    }
}
