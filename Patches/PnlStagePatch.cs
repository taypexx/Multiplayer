using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Managers;
using Multiplayer.UI;

namespace Multiplayer.Patches
{
    [HarmonyPatch(typeof(PnlStage),nameof(PnlStage.Awake))]
    internal static class PnlStagePatch
    {
        private static void Postfix()
        {
            if (LobbyManager.IsInLobby && LobbyManager.LocalLobby.Locked)
            {
                _ = Intermission.Start();
            } 
            else AchievementManager.PlayAchievementAnimation();
        }
    }
}
