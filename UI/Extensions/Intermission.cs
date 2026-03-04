using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Multiplayer.Managers;
using Multiplayer.Static;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Multiplayer.Data.Lobbies;
using System.Net.Http.Json;
using System.Text.Json;
using Il2Cpp;

namespace Multiplayer.UI.Extensions
{
    internal static class Intermission
    {
        internal static bool Active { get; private set; } = false;
        internal static Stopwatch Stopwatch { get; private set; } = new Stopwatch();

        private static int CurrentTopGirlID = -1;
        private static int CurrentTopElfinID = -1;
        private static bool IsTopComboEquipped => CurrentTopGirlID == PlayerManager.LocalPlayer.MultiplayerStats.GirlIndex && CurrentTopElfinID == PlayerManager.LocalPlayer.MultiplayerStats.ElfinIndex;

        private static Tuple<string, Color, Color> ButtonPlayTuple = new(
            Localization.Get("Intermission", "ButtonPlay").ToString(),
            new(0f, 0.82f, 0.28f, 1f),
            new(0.536f, 1f, 0.05f, 1f)
        );

        private static Tuple<string, Color, Color> ButtonEquipTuple = new
        (
            Localization.Get("Intermission", "ButtonEquip").ToString(),
            new(1f, 0.6f, 0f, 1f),
            new(1f, 0.972f, 0f, 1f)
        );

        private static Tuple<string, Color, Color> ButtonEquippedTuple = new
        (
            Localization.Get("Intermission", "ButtonEquipped").ToString(),
            new(0.505f, 0.3f, 0.715f, 1f),
            new(0.59f, 0.429f, 0.75f, 1f)
        );

        /// <summary>
        /// Gets the top rank of a <paramref name="entry"/> from either <see href="https://api.musedash.moe"/> or <see href="https://api.mdmc.moe"/>.
        /// </summary>
        private static async Task UpdateTopCombo(PlaylistEntry entry)
        {
            try
            {
                if (entry.IsCustom)
                {
                    var response = await Client.GetAsync($"https://api.mdmc.moe/v3/sheets/{entry.EntryKey}/scores?limit=1", true, false);
                    if (response == null) return;

                    var body = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
                    if (body.Count == 0) return;

                    var scores = body["scores"].Deserialize<List<Dictionary<string, JsonElement>>>();
                    if (scores.Count == 0) return;

                    var topScore = scores.First();
                    CurrentTopGirlID = topScore["characterId"].GetInt32();
                    CurrentTopElfinID = topScore["elfinId"].GetInt32();
                }
                else
                {
                    var response = await Client.GetAsync($"https://api.musedash.moe/rank/{entry.MusicInfo.uid}/{entry.Difficulty - 1}/all", true, false);
                    if (response == null) return;

                    var scores = await response.Content.ReadFromJsonAsync<List<List<JsonElement>>>();
                    if (scores.Count == 0) return;

                    var topScore = scores.First();
                    CurrentTopGirlID = int.Parse(topScore[6].GetString());
                    CurrentTopElfinID = int.Parse(topScore[7].GetString());
                }
            }
            catch (Exception ex)
            {
                Main.Log(ex);
            }
        }

        /// <summary>
        /// Updates the top label.
        /// </summary>
        private static void UpdateNotification()
        {
            if (!Active || !LobbyManager.IsInLobby) return;

            var entry = LobbyManager.LocalLobby.CurrentPlaylistEntry;
            var chartTitle = ChartManager.GetNiceChartName(entry.MusicInfo, entry.Difficulty);
            var secondsLeft = (int)Math.Ceiling(Constants.IntermissionTimeMS / 1000 - Stopwatch.Elapsed.TotalSeconds);

            string topGirl, topElfin;
            if (CurrentTopGirlID >= 0 && CurrentTopElfinID >= 0)
            {
                topGirl = Utilities.GetGirl(CurrentTopGirlID);
                topElfin = Utilities.GetElfin(CurrentTopElfinID);
            }
            else
            {
                topGirl = Localization.Get("Global", "Loading").ToString();
                topElfin = topGirl;
            }

            SideNotification.Update(
                String.Format(
                    Localization.Get("Intermission", "Label").ToString(),
                    Constants.Yellow, chartTitle,
                    Constants.Pink, topGirl, topElfin,
                    Constants.Yellow, secondsLeft
                ),
                ButtonPlayTuple,
                IsTopComboEquipped ? ButtonEquippedTuple : ButtonEquipTuple
            );
        }

        private static void EnableNotification()
        {
            var entry = LobbyManager.LocalLobby.CurrentPlaylistEntry;
            try
            {
                UIManager.PnlStage.SelectAllTagAndJumpToAssginIndex(entry.MusicInfo.uid);
            }
            catch { }

            var buttonMain = new Tuple<string, Color, Color, Action>
            (
                Localization.Get("Intermission", "ButtonPlay").ToString(),
                new(), new(),
                new(() => 
                {
                    SoundManager.PlayClick();
                    if (!Active) return;

                    Active = false;
                })
            );
            var buttonSecondary = new Tuple<string, Color, Color, Action>
            (
                Localization.Get("Intermission", "ButtonEquip").ToString(),
                new(), new(),
                new(() =>
                {
                    SoundManager.PlayClick();
                    if (IsTopComboEquipped || !Active) return;

                    DataHelper.selectedRoleIndex = CurrentTopGirlID;
                    DataHelper.selectedElfinIndex = CurrentTopElfinID;

                    UpdateNotification();
                })
            );

            SideNotification.Popup(string.Empty, buttonMain, buttonSecondary);
        }

        /// <summary>
        /// Resets everything before entering the chart.
        /// </summary>
        private static void DisableNotification()
        {
            CurrentTopGirlID = -1;
            CurrentTopElfinID = -1;

            SideNotification.Close();
            UIManager.MainLobbyDisplay.Destroy();
            UIManager.ChatLobbyDisplay.Destroy();
            PnlHomeExtension.Destroy();
        }

        /// <summary>
        /// Launches the current chart.
        /// </summary>
        private static void StartBattle()
        {
            if (!LobbyManager.IsPlaylistChartComingUp) return;

            var entry = LobbyManager.LocalLobby.CurrentPlaylistEntry;
            var specialSongManager = Singleton<SpecialSongManager>.instance;
            var currentHiddenUnlocked = specialSongManager.IsInvokeHideBms(entry.MusicInfo.uid);

            // If the current chart's hidden is not unlocked and entry diff is 4
            if (entry.Difficulty == 4 && !currentHiddenUnlocked)
            {
                specialSongManager.InvokeHideBms(entry.MusicInfo, true);
            }
            // If the current chart's hidden is unlocked and entry diff is 3
            else if (entry.Difficulty == 3 && currentHiddenUnlocked)
            {
                // TODO: figure this out
            }

            UIManager.JumpToChart(entry.MusicInfo.uid);
            GlobalDataBase.dbMusicTag.selectedDiffTglIndex = entry.Difficulty == 4 ? 3 : entry.Difficulty;
            GlobalDataBase.dbMusicTag.pnlSelectMusicUid = entry.MusicInfo.uid;

            // DB music tag will trick the CurMusicInfo to return the current playlist entry
            BattleHelper.GameBattleStart(new Il2CppSystem.Object());
        }

        /// <summary>
        /// Starts the intermission.
        /// </summary>
        internal static async Task Start()
        {
            if (Active || !UIManager.Initialized || !LobbyManager.IsInLobby) return;
            Active = true;

            UIManager.Debounce = true;
            LobbyManager.LocalLobby.SyncPlaylistEntry();
            _ = UpdateTopCombo(LobbyManager.LocalLobby.CurrentPlaylistEntry);

            Stopwatch.Restart();
            Main.Dispatch(EnableNotification);

            while (Stopwatch.ElapsedMilliseconds < Constants.IntermissionTimeMS && Active)
            {
                Main.Dispatch(UpdateNotification);
                await Task.Delay(500);
            }
            Stopwatch.Stop();

            UIManager.Debounce = false;
            Main.Dispatch(StartBattle);
            Main.Dispatch(DisableNotification);
            Active = false;
        }
    }
}
