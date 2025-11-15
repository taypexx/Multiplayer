using HarmonyLib;
using Il2Cpp;
using Multiplayer.Managers;

namespace Multiplayer.Patches
{
    internal static class BattlePatch
    {
        [HarmonyPatch(typeof(SceneObjectController),nameof(SceneObjectController.Run))]
        internal static class BattleStartPatch
        {
            private static void Postfix()
            {
                if (PlayerManager.LocalPlayer is null || LobbyManager.LocalLobby is null) return;

                BattleManager.BattleSyncStart();
            }
        }
    }
}
