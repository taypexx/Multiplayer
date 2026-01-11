using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.UI.Panels.PnlRole;
using Multiplayer.Managers;

namespace Multiplayer.Patches
{
    /// <summary>
    /// Synchronizes character/elfin combo with the server
    /// </summary>
    internal static class CombinationUpdatePatch
    {
        [HarmonyPatch(typeof(PnlRole), nameof(PnlRole.OnApplyClicked))]
        internal static class GirlUpdatePatch
        {
            private static void Postfix()
            {
                PlayerManager.SyncProfile();
            }
        }

        [HarmonyPatch(typeof(PnlElfin), nameof(PnlElfin.OnApplyClicked))]
        internal static class ElfinUpdatePatch
        {
            private static void Postfix()
            {
                PlayerManager.SyncProfile();
            }
        }
    }
}
