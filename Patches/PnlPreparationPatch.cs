using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Data;
using Multiplayer.Managers;
using Multiplayer.UI.Extensions;

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
            private static async Task SetPnlRetrieving(CustomChartData customData)
            {
                await customData.Update();
                PnlPreparationExtension.IsRetrieving = false;
                Main.Dispatch(PnlPreparationExtension.UpdatePnlPreparation);
            }

            private static void Postfix()
            {
                if (!LobbyManager.IsInLobby) return;

                var customData = ChartManager.GetCustomChartData(GlobalDataBase.dbMusicTag.CurMusicInfo().uid);
                if (customData != null && customData.IsOnWebsite is null) 
                {
                    PnlPreparationExtension.IsRetrieving = true;
                    _ = SetPnlRetrieving(customData);
                }
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
