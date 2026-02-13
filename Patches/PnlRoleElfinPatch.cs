using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.UI.Panels.PnlRole;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Extensions;

namespace Multiplayer.Patches
{
    internal static class PnlRoleElfinPatch
    {
        /// <summary>
        /// Prevents character BGM from accidently playing while initializing PnlRole.
        /// </summary>
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(PnlRole), nameof(PnlRole.PlayCharacterBgm))]
        internal static class PnlRoleMusicPatch
        {
            private static bool Prefix(PnlRole __instance)
            {
                return __instance.isActiveAndEnabled || (UIManager.Initialized && UIManager.PnlMenu.gameObject.active);
            }
        }

        /// <summary>
        /// Doesn't let you apply the current character if it's sleepwalker and syncs the combo when a character gets applied.
        /// </summary>
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(PnlRole), nameof(PnlRole.OnApplyClicked))]
        internal static class PnlRoleApplyPatch
        {
            private static bool Prefix()
            {
                if (!UIManager.Initialized) return true;
                return !(LobbyManager.IsInLobby && UIManager.PnlRole.m_CurrentCharacterInfo.listIndex == Constants.SleepwalkerRoleIndex);
            }

            private static void Postfix()
            {
                PlayerManager.SyncProfile();
            }
        }

        /// <summary>
        /// Syncs the combo when an elfin gets applied.
        /// </summary>
        [HarmonyPatch(typeof(PnlElfin), nameof(PnlElfin.OnApplyClicked))]
        internal static class ElfinUpdatePatch
        {
            private static void Postfix()
            {
                PlayerManager.SyncProfile();
            }
        }

        /// <summary>
        /// Locks/unlocks the PnlRole select button if the current character is sleepwalker.
        /// </summary>
        [HarmonyPatch(typeof(PnlRole), nameof(PnlRole.OnFsvIndexChanged))]
        internal static class PnlRoleIndexChangePatch
        {
            private static void Postfix(int index)
            {
                if (!LobbyManager.IsInLobby) return;

                bool sleepwalkerVisible = UIManager.PnlRole.m_ConfigCharacter.GetCharacterInfoByOrder(index + 1).listIndex == Constants.SleepwalkerRoleIndex;
                if (!PnlMenuExtension.ImgPnlRoleLockedEnabled && sleepwalkerVisible)
                {
                    PnlMenuExtension.ToggleSleepwalkerSelection(false);
                }
                else if (PnlMenuExtension.ImgPnlRoleLockedEnabled && !sleepwalkerVisible)
                {
                    PnlMenuExtension.ToggleSleepwalkerSelection(true);
                }
            }
        }
    }
}
