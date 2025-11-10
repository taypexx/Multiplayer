using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Data;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace Multiplayer.Managers
{
    internal static class AchievementManager
    {
        internal static List<Achievement> Achievements { get; private set; }
        private static List<Achievement> QueuedAchievements;
        private static Color AchievementTitleColor = new(1, 0.792f, 0.373f);

        private static PnlMessage PnlMessage => GameObject.Find("CommonManagers/MessagesManager/UI/PnlMessage").GetComponent<PnlMessage>();
        private static Il2CppAssets.Scripts.GameCore.Managers.AchievementManager PeroAchievementManager => Singleton<Il2CppAssets.Scripts.GameCore.Managers.AchievementManager>.instance;

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

            PnlMessage.FinishMessage();

            // FIX THIS SHIT PLEASE

            int i = 0;
            foreach (Transform cell in PnlMessage.layout.transform)
            {
                Console.WriteLine(i);
                Achievement achievement = QueuedAchievements[i];
                cell.Find("TxtDescription").GetComponent<Text>().text = $"<color=#{ColorUtility.ToHtmlStringRGB(AchievementTitleColor)}>{achievement.Name}</color>    {achievement.Description}";
                i++;
            }

            QueuedAchievements.Clear();
        }

        /// <summary>
        /// Adds an <see cref="Achievement"/> to the local <see cref="Player"/>'s profile.
        /// </summary>
        /// <param name="achievementId">ID of an <see cref="Achievement"/>.</param>
        /// <param name="instantAnim">Whether to play it instantly after getting or queue to play later.</param>
        /// <returns><see langword="true"/> if it was added successfully, otherwise <see langword="false"/>.</returns>
        internal static bool Achieve(int achievementId, bool instantAnim = true)
        {
            Player localPlayer = PlayerManager.LocalPlayer;
            if (localPlayer == null) return false;

            Achievement achievement = Achievements[achievementId];
            if (achievement == null) return false;

            if (localPlayer.MultiplayerStats.Achievements.ContainsValue(achievement)) return false;

            localPlayer.MultiplayerStats.Achievements.Add(DateTime.UtcNow,achievement);
            PlayerManager.SyncLocalPlayer();

            QueuedAchievements.Add(achievement);
            if (instantAnim) PlayAchievementAnimation();

            return true;
        }

        internal static void Init()
        {
            QueuedAchievements = new();

            Achievements = new()
            {
                new("Welcome!",Localization.Get("Achievements","Welcome!"))
            };
        }
    }
}
