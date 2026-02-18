using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore.Managers;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI;
using Multiplayer.UI.Extensions;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.Patches
{
    internal static class BattlePatch
    {
        private static bool AwaitingForReady = false;
        private static bool HasFailed = false;
        private static bool Started = false;

        /// <summary>
        /// Runs when the GameMain scene loads.
        /// </summary>
        internal static void SceneLoaded()
        {
            HasFailed = false;
            Started = false;

            if (!LobbyManager.IsInLobby) return;

            // Toggle the pause button
            GameObject.Find("UI_2D/Standard/PnlBattle/PnlBattleUI/PnlBattleOthers/Up/BtnPause").SetActive(false);
        }

        /// <summary>
        /// Runs a loop and waits until everyone loads.
        /// </summary>
        private static async Task AwaitForReady()
        {
            if (AwaitingForReady) return;
            AwaitingForReady = true;

            while (!LobbyManager.LocalLobby.EveryoneReady)
            {
                if (!LobbyManager.IsInLobby) 
                {
                    AwaitingForReady = false; 
                    return; 
                }
                await Task.Delay(Constants.AwaitBattleInterval);
            }

            AwaitingForReady = false;
            Main.Dispatch(SingletonMonoBehaviour<PnlBattle>.instance.GameStart);
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
                    _ = LobbyManager.SetReady(true);

                    PnlAwait.Create();
                    _ = AwaitForReady();
                }
                return !AwaitingForReady;
            }

            /// <summary>
            /// Removes the awaiting panel and starts synchronizing with the server.
            /// </summary>
            private static void Postfix()
            {
                if (!LobbyManager.IsInLobby || AwaitingForReady) return;

                PnlAwait.Destroy();
                _ = BattleManager.SyncStart();

                // Fail screen adjustments
                var failRestartButtonGo = GameObject.Find("UI_2D/Standard/PnlFail/ImgBgDown/BtnRestart");
                failRestartButtonGo.SetActive(false);

                var returnButtonGo = GameObject.Find("UI_2D/Standard/PnlFail/ImgBgDown/BtnReturn");
                returnButtonGo.transform.localPosition = failRestartButtonGo.transform.localPosition;

                // Toggle info+ label
                var infoPlusLabel = GameObject.Find("InfoPlus_TextLowerLeft");
                if (infoPlusLabel != null)
                {
                    infoPlusLabel.SetActive(false);
                }
            }
        }

        ////////////////////////////////////////////////////////////////

        private static HashSet<string> DisplayedPlayers = new();

        /// <summary>
        /// Tells the server that you are done with the chart.
        /// </summary>
        private static void FinishCurrentChart()
        {
            UIManager.BattleLobbyDisplay.Destroy();
            _ = LobbyManager.SetReady(false);
        }

        /// <summary>
        /// Shows all player results by displaying them as achievements.
        /// </summary>
        private static void ShowPlayResults()
        {
            var localLobby = LobbyManager.LocalLobby;
            if (!localLobby.EveryoneFinished) return;

            DisplayedPlayers.Clear();
            PnlMessageExtension.Enable();

            var gradeObjects = GameObject.Find("UI_3D/PnlVictory/Default/PnlVictory/PnlVictory_2D/Info/Grade").GetComponent<GameMainGrade>().gradeObjects;

            var sprites = new Sprite[gradeObjects.Count];
            for (int i = 0; i < gradeObjects.Count; i++)
            {
                sprites[i] = gradeObjects[i].GetComponent<Image>().sprite;
            }

            var positionList = localLobby.Players.ToList();
            positionList.Sort(localLobby.GoalComparison);

            Task.Run(async () =>
            {
                foreach (var playerUid in positionList)
                {
                    if (!DisplayedPlayers.Contains(playerUid) && !localLobby.ReadyPlayers.Contains(playerUid))
                    {
                        var player = PlayerManager.GetCachedPlayer(playerUid);
                        if (player is null) continue;

                        DisplayedPlayers.Add(playerUid);
                        await PnlMessageExtension.AddOne($"{positionList.Count - positionList.IndexOf(playerUid)}) {player.MultiplayerStats.Name} — {localLobby.GetBattleInfo(player)}", false, sprites[(int)player.BattleStats.Grade]);
                    }
                }
                Main.Dispatch(() =>
                {
                    var nextIndex = localLobby.CurrentPlaylistEntryIndex + 1;
                    if (nextIndex >= localLobby.Playlist.Count) return;

                    var nextEntry = localLobby.Playlist[nextIndex];
                    var chartName = ChartManager.GetNiceChartName(nextEntry.MusicInfo, nextEntry.Difficulty);
                    if (localLobby.CurrentPlaylistEntryIndex + 1 < localLobby.Playlist.Count)
                    {
                        _ = PnlMessageExtension.AddOne(
                            $"{Localization.Get("Battle", "Next")}: {chartName}", false
                        );
                    }
                });
            });
        }

        /// <summary>
        /// Toggles the buttons based on whether everyone has finished playing.
        /// </summary>
        private static void SetVictoryButtons()
        {
            var finished = LobbyManager.LocalLobby.EveryoneFinished;

            var btnContinue = UIManager.PnlVictory.m_CurControls.btnContinue;
            btnContinue.transform.Find("TxtContinue/ImgBtnA").gameObject.SetActive(finished);
            btnContinue.transform.Find("TxtContinue").GetComponent<Text>().text = Localization.Get("Battle", finished ? "Continue" : "Awaiting").ToString();

            var restartButton = UIManager.PnlVictory.m_CurControls.btnReset;
            restartButton.gameObject.SetActive(finished);

            if (finished)
            {
                restartButton.transform.Find("TxtRestart").GetComponent<Text>().text = Localization.Get("Battle", "Results").ToString();
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener((UnityAction)new Action(ShowPlayResults));
            }
        }

        /// <summary>
        /// Waits for everyone to finish the chart and continues.
        /// </summary>
        private static async Task AwaitForFinish()
        {
            while (LobbyManager.IsInLobby && !LobbyManager.LocalLobby.EveryoneFinished)
            {
                await Task.Delay(Constants.AwaitBattleInterval);
            }

            Main.Dispatch(SetVictoryButtons);

            BattleManager.SyncStop();

            var messageManager = SingletonMonoBehaviour<MessageManager>.instance;
            while (messageManager.messages.Count > 0 || PnlMessageExtension.Visible)
            {
                await Task.Delay(Constants.AwaitBattleInterval);
            }
            Main.Dispatch(ShowPlayResults);
        }

        /// <summary>
        /// Patches the visuals of PnlVictory.
        /// </summary>
        [HarmonyPatch]
        internal static class BattleVictoryPatch
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(PnlVictory).GetMethods().Where(m => m.Name == nameof(PnlVictory.OnVictory));
            }

            private static void Postfix()
            {
                if (!LobbyManager.IsInLobby) return;

                SetVictoryButtons();
                FinishCurrentChart();

                _ = AwaitForFinish();
                _ = LobbyManager.PlaylistContinue();
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

                BattleManager.SyncStop();

                FinishCurrentChart();
                HasFailed = true;
            }
        }

        ////////////////////////////////////////////////////////////////

        /// <summary>
        /// Keeps the application focused and disables the pause if local player is in the lobby.
        /// </summary>
        [HarmonyPriority(Priority.First)]
        internal static class BattlePausePatch
        {
            private static MethodBase[] Methods = { 
                typeof(PnlBattle).GetMethod(nameof(PnlBattle.Pause)),
                typeof(Il2CppAssets.Scripts.PeroTools.Managers.UnityGameManager).GetMethod(nameof(Il2CppAssets.Scripts.PeroTools.Managers.UnityGameManager.OnApplicationFocus))
            };
            private static IEnumerable<MethodBase> TargetMethods() => Methods;

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
                return !LobbyManager.IsInLobby;// || HasFailed;
            }

            private static void Postfix()
            {
                AwaitingForReady = false;
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
                return !LobbyManager.IsInLobby || LobbyManager.LocalLobby.EveryoneFinished || HasFailed;
            }

            private static void Postfix()
            {
                if (!LobbyManager.IsInLobby) return;

                // Resetting stats on exit
                foreach (string playerUid in LobbyManager.LocalLobby.Players)
                {
                    Player player = PlayerManager.GetCachedPlayer(playerUid);
                    if (player == null) continue;
                    player.BattleStats.Reset();
                }

                AwaitingForReady = false;
            }
        }
    }
}