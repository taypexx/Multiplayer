using HarmonyLib;
using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Managers;

namespace Multiplayer.Patches
{
    /// <summary>
    /// Unlocks the lobby if the playlist was completed, otherwise starts the next chart.
    /// </summary>
    [HarmonyPatch(typeof(PnlStage),nameof(PnlStage.Awake))]
    internal static class PnlStagePatch
    {
        private static async Task YieldBattleStart()
        {
            await Task.Delay(1000);
            Main.Dispatcher.Enqueue(() => 
            {
                UIManager.PnlPreparation.OnBattleStart();
            });
        }

        private static void Postfix()
        {
            if (LobbyManager.IsInLobby && LobbyManager.LocalLobby.Locked && LobbyManager.LocalLobby.Host == PlayerManager.LocalPlayer)
            {
                if (LobbyManager.LocalLobby.Playlist.Count == 0)
                {
                    _ = LobbyManager.LockLobby(false);
                }
                else _ = YieldBattleStart();
            }

            AchievementManager.PlayAchievementAnimation();
        }
    }
}
