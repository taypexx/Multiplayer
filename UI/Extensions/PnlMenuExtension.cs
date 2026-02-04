using Il2CppAssets.Scripts.UI.Panels.PnlRole;
using Multiplayer.Managers;
using Multiplayer.Static;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Extensions
{
    internal static class PnlMenuExtension
    {
        private static GameObject ImgPnlRoleLocked;
        internal static bool ImgPnlRoleLockedEnabled => ImgPnlRoleLocked != null && ImgPnlRoleLocked.active;

        /// <summary>
        /// Locks/unlocks sleepwalker selection on the <see cref="PnlRole"/>.
        /// </summary>
        /// <param name="state">Whether to lock or unlock the select button.</param>
        internal static void ToggleSleepwalkerSelection(bool state)
        {
            var parent = ImgPnlRoleLocked.transform.parent;
            if (!state)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    parent.GetChild(i).gameObject.SetActive(false);
                }
            }

            parent.parent.gameObject.SetActive(!state);
            parent.gameObject.SetActive(!state);
            ImgPnlRoleLocked.SetActive(!state);
        }

        internal static void Create()
        {
            var pnlRoleLockedRef = UIManager.PnlRole.transform.Find("Properties/BtnApply/Locked/ImgLocked/ImgSkinPurchase").gameObject;
            ImgPnlRoleLocked = GameObject.Instantiate(
                pnlRoleLockedRef,
                pnlRoleLockedRef.transform.parent
            );
            ImgPnlRoleLocked.name = "ImgUnavailable";
            ImgPnlRoleLocked.SetActive(false);
            Component.Destroy(ImgPnlRoleLocked.GetComponent<Image>());

            var pnlRoleLockedText = ImgPnlRoleLocked.transform.Find("TxtPurchase").GetComponent<Text>();
            pnlRoleLockedText.gameObject.name = "TxtUnavailable";
            Component.Destroy(pnlRoleLockedText.GetComponent<Il2CppAssets.Scripts.PeroTools.GeneralLocalization.Localization>());
            pnlRoleLockedText.font = Utilities.NormalFont;
            pnlRoleLockedText.text = Localization.Get("Global", "Unavailable").ToString().ToUpper();
            pnlRoleLockedText.resizeTextMaxSize = 34;
            pnlRoleLockedText.color = new(0.5054f, 0.3f, 0.7144f, 1f);
        }
    }
}
