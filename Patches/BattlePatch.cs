using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Managers;
using Multiplayer.Static;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Patches
{
    internal static class BattlePatch
    {
        private static bool AwaitingForOthers = false;
        private static bool CanExit = false;
        private static bool Started = false;

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
                await Task.Delay(Constants.AwaitBattleInterval);
            }

            AwaitingForOthers = false;
            Main.Dispatcher.Enqueue(() => 
            {
                SingletonMonoBehaviour<PnlBattle>.instance.GameStart();
            });
        }

        /// <summary>
        /// Resets everything before moving further.
        /// </summary>
        private static void FinishCurrentChart()
        {
            if (BattleManager.Synchronizing)
            {
                BattleManager.BattleSyncStop();
            }
            UIManager.BattleLobbyDisplay.Destroy();
            _ = LobbyManager.SetReady(false);
        }

        /// <summary>
        /// Runs when the GameMain scene loads.
        /// </summary>
        internal static void SceneLoaded()
        {
            CanExit = false;
            Started = false;
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
                if (!LobbyManager.IsInLobby) return true;

                if (!Started)
                {
                    Started = true;
                    _ = AwaitForOthers();
                    _ = LobbyManager.SetReady(true);
                }
                return !AwaitingForOthers;
            }

            /// <summary>
            /// Removes the awaiting panel and starts synchronizing with the server.
            /// </summary>
            private static void Postfix()
            {
                if (!LobbyManager.IsInLobby || AwaitingForOthers) return;

                UIManager.PnlAwait.Destroy();
                BattleManager.BattleSyncStart();
                GameObject.Find("UI_2D/Standard/PnlBattle/PnlBattleUI/PnlBattleOthers/Up/BtnPause").SetActive(false);

                var infoPlusLabel = GameObject.Find("InfoPlus_TextLowerLeft");
                if (infoPlusLabel != null)
                {
                    infoPlusLabel.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Patches the end screen to display winners and move forward to the next chart.
        /// </summary>
        [HarmonyPatch]
        internal static class BattleVictoryPatch
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(PnlBattle).GetMethods().Where(m => m.Name == nameof(PnlBattle.OnShowVictory));
            }
        }

        /// <summary>
        /// Patches the visuals of PnlVictory.
        /// </summary>
        [HarmonyPatch]
        internal static class BattleVictoryPanelPatch
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(PnlVictory).GetMethods().Where(m => m.Name == nameof(PnlVictory.OnVictory));
            }

            private static async Task Continue()
            {
                await LobbyManager.PlaylistContinue();
                CanExit = true;
            }

            private static void Postfix()
            {
                if (!LobbyManager.IsInLobby) return;

                FinishCurrentChart();

                // Display the fake achievements as the winners (await for thr retrieval)

                _ = Continue();

                PnlVictory.m_CurControls.btnReset.gameObject.SetActive(false);
                
                if (LobbyManager.LocalLobby.Playlist.Count > 1)
                {
                    PnlVictory.m_CurControls.btnContinue.transform.Find("TxtContinue").gameObject.GetComponent<Text>().text =
                    Localization.Get("Global", "Next").ToString();
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

                FinishCurrentChart();

                var restartButton = GameObject.Find("UI_2D/Standard/PnlFail/ImgBgDown/BtnRestart");
                restartButton.SetActive(false);

                var returnButton = GameObject.Find("UI_2D/Standard/PnlFail/ImgBgDown/BtnReturn");
                returnButton.transform.localPosition = restartButton.transform.localPosition;
                CanExit = true;
            }
        }

        /// <summary>
        /// Disables the pause method. TODO: fix :sob:
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
                if (!LobbyManager.IsInLobby) return;

                AwaitingForOthers = false;
            }
        }
    }
}