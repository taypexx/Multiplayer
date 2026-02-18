using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels.PnlAccount;
using Multiplayer.Managers;
using Multiplayer.Static;
using PopupLib.UI;

namespace Multiplayer.Patches
{
    internal static class AccountPatch
    {
        [HarmonyPatch(typeof(PnlLogOutAsk), nameof(PnlLogOutAsk.OnYesClicked))]
        internal static class AccountLogOutPatch
        {
            /// <summary>
            /// Doesn't let you log out of your account if the local player is in lobby.
            /// </summary>
            private static bool Prefix()
            {
                return !LobbyManager.IsInLobby;
            }

            /// <summary>
            /// Disconnects from the server in case local player logged out of their account.
            /// </summary>
            private static void Postfix()
            {
                Client.Disconnect();
            }
        }

        [HarmonyPatch(typeof(PnlAccountSystem), nameof(PnlAccountSystem.OnLogoutClicked))]
        internal static class AccountLogOutTryPatch
        {
            /// <summary>
            /// Doesn't let you click the log out button if the local player is in lobby.
            /// </summary>
            private static bool Prefix()
            {
                if (LobbyManager.IsInLobby) PopupUtils.ShowInfo(Localization.Get("Warning", "LogoutUnavailable"));
                return !LobbyManager.IsInLobby;
            }
        }
    }
}
