using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels.PnlRole;

namespace Multiplayer.Patches
{
    internal static class PnlRolePatch
    {
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(PnlRole), nameof(PnlRole.PlayCharacterBgm))]
        internal static class PnlRoleMusicPatch
        {
            // Makes sure the girl bgm doesn't play when PnlRole initializes (probably doesn't work)
            private static bool Prefix(PnlRole __instance)
            {
                return __instance.isActiveAndEnabled;
            }
        }
    }
}
