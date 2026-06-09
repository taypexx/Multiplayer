using HarmonyLib;
using Multiplayer.UI.Extensions;
using UnityEngine.UI;

namespace Multiplayer.Patches
{
    [HarmonyPatch(typeof(Toggle), "OnPointerClick")]
    internal class ToggleGhostClickPatch
    {
        private static void Prefix(Toggle __instance, out Toggle.ToggleEvent __state)
        {
            __state = null;
            // If we are in a Multiplayer window (which means PopupLib is using PnlBulletinNew)
            if (BulletinExtension.CurrentWindow != null)
            {
                // We check if this toggle is part of the Bulletin list
                if (__instance.gameObject.name.Contains("Bulletin") || __instance.transform.parent.name == "Content")
                {
                    // Swap the persistent onValueChanged event with a temporary empty one
                    __state = __instance.onValueChanged;
                    __instance.onValueChanged = new Toggle.ToggleEvent();
                }
            }
        }

        private static void Postfix(Toggle __instance, Toggle.ToggleEvent __state)
        {
            if (__state != null)
            {
                // Restore the original event so the vanilla game doesn't break later
                __instance.onValueChanged = __state;
            }
        }
    }
}
