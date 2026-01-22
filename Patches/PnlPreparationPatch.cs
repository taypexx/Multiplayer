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
                PnlPreparationExtension.UpdatePnlPreparation();
            }
        }

        /// <summary>
        /// Jumps to the current playlist entry of the chart and continues to battle start.
        /// </summary>
        [HarmonyPatch(typeof(PnlPreparation), nameof(PnlPreparation.OnBattleStart))]
        internal static class PnlPreparationOnBattleStart
        {
            private static bool Prefix()
            {
                if (!LobbyManager.IsInLobby) return true;

                bool startCondition = LobbyManager.IsInLobby && LobbyManager.LocalLobby.Locked;
                if (!startCondition) return startCondition;

                PnlPreparationExtension.PrepareBeforeBattle();

                return startCondition;
            }
        }
    }
}
