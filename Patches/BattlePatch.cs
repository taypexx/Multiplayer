using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Data.LobbyEnums;
using Multiplayer.Managers;
using Multiplayer.Static;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Patches
{
    internal static class BattlePatch
    {
        private static bool AwaitingForOthers = false;
        private static bool CanExit = false;

        private static PnlVictory PnlVictory => GameObject.Find("UI_3D/PnlVictory").GetComponent<PnlVictory>();

        /// <summary>
        /// Runs a loop and waits until everyone loads.
        /// </summary>
        private static async Task AwaitForOthers()
        {
            if (AwaitingForOthers) return;
            AwaitingForOthers = true;

            while (LobbyManager.IsInLobby && !LobbyManager.LocalLobby.EveryoneReady)
            {
                await Task.Delay(LobbyManager.AutoUpdateInterval);
            }

            AwaitingForOthers = false;
            Main.Dispatcher.Enqueue(() => 
            {
                SingletonMonoBehaviour<PnlBattle>.instance.GameStart();
            });
        }

        /// <summary>
        /// Runs when the GameMain scene loads.
        /// </summary>
        internal static void SceneLoaded()
        {
            CanExit = false;
            
            if (LobbyManager.IsInLobby)
            {
                _ = AwaitForOthers();
                _ = LobbyManager.SetReady(true);
            }
        }

        /// <summary>
        /// Yields the game start until everyone loads up.
        /// </summary>
        [HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.GameStart))]
        [HarmonyPriority(Priority.First)]
        internal static class BattleStartPatch
        {
            /// <summary>
            /// Skips the method execution if awaiting for other players.
            /// </summary>
            private static bool Prefix()
            {
                return !((LobbyManager.IsInLobby && AwaitingForOthers));
            }

            /// <summary>
            /// Removes the awaiting panel and starts synchronizing with the server.
            /// </summary>
            private static void Postfix()
            {
                if (LobbyManager.IsInLobby && !AwaitingForOthers)
                {
                    UIManager.PnlAwait.Destroy();
                    BattleManager.BattleSyncStart();

                    GameObject.Find("UI_2D/Standard/PnlBattle/PnlBattleUI/PnlBattleOthers/Up/BtnPause").SetActive(false);
                }
            }
        }

        /// <summary>
        /// Removes the restart button from the fail screen.
        /// </summary>
        [HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.OnFail))]
        internal static class BattleFailPatch
        {
            private static void Postfix()
            {
                if (!LobbyManager.IsInLobby) return;

                UIManager.BattleLobbyDisplay.Destroy();

                var restartButton = GameObject.Find("UI_2D/Standard/PnlFail/ImgBgDown/BtnRestart");
                restartButton.SetActive(false);

                var returnButton = GameObject.Find("UI_2D/Standard/PnlFail/ImgBgDown/BtnReturn");
                returnButton.transform.localPosition = restartButton.transform.localPosition;

                CanExit = true;
            }
        }

        [HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.OnShowVictory), typeof(Il2CppSystem.Object), typeof(Il2CppSystem.Object), typeof(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>))]
        internal static class BattleVictoryPatch
        {
            private static void Postfix()
            {
                if (!LobbyManager.IsInLobby) return;

                UIManager.BattleLobbyDisplay.Destroy();
                PnlVictory.m_CurControls.btnReset.gameObject.SetActive(false);
                
                if (LobbyManager.LocalLobby.ChartSelection != LobbyChartSelection.HostPlaylist)
                {
                    PnlVictory.m_CurControls.btnContinue.transform.Find("TxtContinue").gameObject.GetComponent<Text>().text = Localization.Get("Global", "Next").ToString();
                }

                // Display the fake achievements as the winners

                CanExit = true;
            }
        }

        /// <summary>
        /// Disables the pause method.
        /// </summary>
        [HarmonyPatch(typeof(PnlBattle), nameof(PnlBattle.Pause))]
        [HarmonyPriority(Priority.First)]
        internal static class BattlePausePatch
        {
            private static bool Prefix() 
            {
                return !LobbyManager.IsInLobby;
            }
        }

        /// <summary>
        /// Doesn't let you restart the chart if you are playing multiplayer.
        /// </summary>
        [HarmonyPatch(typeof(BattleHelper), nameof(BattleHelper.GameRestart))]
        [HarmonyPriority(Priority.First)]
        internal static class BattleHelperRestartPatch
        {
            private static bool Prefix()
            {
                return !LobbyManager.IsInLobby;// || CanExit;
            }

            private static void Postfix()
            {
                AwaitingForOthers = false;
            }
        }

        /// <summary>
        /// Doesn't let you exit the chart if you are playing multiplayer.
        /// </summary>
        [HarmonyPatch(typeof(BattleHelper), nameof(BattleHelper.GameFinish))]
        [HarmonyPriority(Priority.First)]
        internal static class BattleHelperFinishPatch
        {
            private static bool Prefix()
            {
                return !LobbyManager.IsInLobby || CanExit;
            }

            private static void Postfix()
            {
                AwaitingForOthers = false;
            }
        }
    }
}