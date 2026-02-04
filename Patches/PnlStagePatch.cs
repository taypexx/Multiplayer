using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Managers;
using Multiplayer.UI.Extensions;

namespace Multiplayer.Patches
{
    /// <summary>
    /// Starts the next intermission when PnlStage awakes or plays the achievement animation.
    /// </summary>
    [HarmonyPatch(typeof(PnlStage),nameof(PnlStage.Awake))]
    internal static class PnlStagePatch
    {
        private static void Postfix()
        {
            if (LobbyManager.IsInLobby && LobbyManager.LocalLobby.Locked)
            {
                _ = Intermission.Start();
            } 
            else if (UIManager.Initialized) AchievementManager.PlayAchievementAnimation();
        }
    }
}
