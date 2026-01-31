using HarmonyLib;
using Il2Cpp;
using Multiplayer.Managers;
using Multiplayer.UI;

namespace Multiplayer.Patches
{
    internal static class PnlPreparationPatch
    {
        /// <summary>
        /// Updates itself when difficulty changes.
        /// </summary>
        [HarmonyPatch(typeof(PnlPreparation), nameof(PnlPreparation.OnDiffTglChanged))]
        internal static class PnlPreparationDiffChanged
        {
            private static void Postfix()
            {
                if (!UIManager.Initialized) return;
                PnlPreparationExtension.UpdatePnlPreparation();
            }
        }

        /// <summary>
        /// Prevents the local player from entering the chart if they are in lobby.
        /// </summary>
        [HarmonyPatch(typeof(PnlPreparation), nameof(PnlPreparation.OnBattleStart))]
        internal static class PnlPreparationOnBattleStart
        {
            private static bool Prefix()
            {
                return !LobbyManager.IsInLobby;
            }
        }
    }
}
