using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels.PnlRole;
using Multiplayer.Managers;

namespace Multiplayer.Patches
{
    internal static class PnlRolePatch
    {
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(PnlRole), nameof(PnlRole.PlayCharacterBgm))]
        internal static class PnlRoleMusicPatch
        {
            private static bool Prefix(PnlRole __instance)
            {
                return __instance.isActiveAndEnabled || UIManager.PnlMenu.gameObject.active;
            }
        }
    }
}
