using HarmonyLib;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Managers;

namespace Multiplayer.Patches
{
    internal static class BattlePatch
    {
        private static bool AwaitingForOthers = false;

        /// <summary>
        /// Runs a loop and waits until everyone loads.
        /// </summary>
        private static async Task AwaitForOthers()
        {
            if (AwaitingForOthers) return;
            AwaitingForOthers = true;

            while (LobbyManager.LocalLobby != null && !LobbyManager.LocalLobby.EveryoneReady)
            {
                await Task.Delay(LobbyManager.AutoUpdateInterval);
            }

            AwaitingForOthers = false;

            Singleton<PnlBattle>.instance.GameStart();
        }

        //[HarmonyPatch(typeof(SceneObjectController), nameof(SceneObjectController.Run))]
        [HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
        [HarmonyPriority(Priority.First)]
        internal static class BattleStartPatch
        {
            private static bool Prefix()
            {
                return !AwaitingForOthers; // Game won't start unless everyone is ready
            }

            private static void Postfix()
            {
                BattleManager.BattleSyncStart();
            }
        }

        /// <summary>
        /// Prepares the battle for the multiplayer shenanigans.
        /// </summary>
        internal static void BattleSceneLoaded()
        {
            if (PlayerManager.LocalPlayer is null || LobbyManager.LocalLobby is null) return;

            _ = LobbyManager.SetReady(true);
            _ = AwaitForOthers();
        }
    }
}
