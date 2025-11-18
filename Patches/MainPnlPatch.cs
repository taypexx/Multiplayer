using HarmonyLib;
using Il2Cpp;
using Multiplayer.Managers;

namespace Multiplayer.Patches
{
    internal static class MainPnlPatch
    {
        [HarmonyPatch(typeof(PnlPreparation), nameof(PnlPreparation.OnEnable))]
        internal static class PnlPreparationLock
        {
            private static void Postfix()
            {
                UIManager.UpdatePnlPreparation();
            }
        }
    }
}
