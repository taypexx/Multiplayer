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

namespace Multiplayer.UI
{
    internal static class Intermission
    {
        internal static bool Active { get; private set; } = false;
        internal static Stopwatch Stopwatch { get; private set; } = new Stopwatch();
        private static int CurrentTopGirlID = -1;
        private static int CurrentTopElfinID = -1;

        private static Text TimerText;
        private static GameObject BtnBack;
        private static Vector3 BtnLabelOffset = new Vector3(1.4f, -0.2f, 0f);

        /// <summary>
        /// Gets the name of a girl with the given <paramref name="girlId"/>.
        /// </summary>
        internal static string GetGirl(int girlId)
        {
            if (girlId < 0) return string.Empty;

            var configManager = Singleton<Il2CppAssets.Scripts.PeroTools.Managers.ConfigManager>.instance;
            var character = configManager.GetJson("character", true)[girlId];

            var characterType = configManager.GetConfigObject<DBConfigCharacter>()
                .GetCharacterInfoByIndex(girlId)
                .characterType;

            return string.Equals(characterType, "Special")
                ? character["characterName"].ToString()
                : character["cosName"].ToString();
        }

        /// <summary>
        /// Gets the name of a elfin with the given <paramref name="elfinId"/>.
        /// </summary>
        internal static string GetElfin(int elfinId)
        {
            if (elfinId < 0) return string.Empty;

            return Singleton<Il2CppAssets.Scripts.PeroTools.Managers.ConfigManager>.instance.GetJson("elfin", true)[elfinId]["name"].ToString();
        }

        /// <summary>
        /// Updates the top label.
        /// </summary>
        private static void Update()
        {
            if (!Active || TimerText == null || !LobbyManager.IsInLobby) return;

            var entry = LobbyManager.LocalLobby.CurrentPlaylistEntry;
            var chartLabel = ChartManager.GetNiceChartName(entry.MusicInfo, entry.Difficulty);
            var secondsLeft = Constants.IntermissionTimeMS/1000 - Stopwatch.Elapsed.TotalSeconds;

            string topGirl, topElfin;
            if (CurrentTopGirlID >= 0 && CurrentTopElfinID >= 0)
            {
                topGirl = GetGirl(CurrentTopGirlID);
                topElfin = GetElfin(CurrentTopElfinID);
            }
            else
            {
                topGirl = Localization.Get("Global", "Loading").ToString();
                topElfin = topGirl;
            }

            TimerText.text = String.Format(
                Localization.Get("Lobby", "IntermissionLabel").ToString(),
                Constants.Yellow, chartLabel, 
                Constants.Blue, topGirl, topElfin,
                Constants.Yellow, (int)secondsLeft
            );
        }

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

                    var scores = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(body["scores"]);
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
        /// Transforms the PnlMenu for the intermission.
        /// </summary>
        private static void SetPnlMenu()
        {
            var entry = LobbyManager.LocalLobby.CurrentPlaylistEntry;
            UIManager.PnlStage.SelectAllTagAndJumpToAssginIndex(entry.MusicInfo.uid);

            var pnlMenu = UIManager.PnlMenu.gameObject;
            BtnBack = pnlMenu.transform.Find("MenuNavigation/BtnBack").gameObject;
            BtnBack.SetActive(false);

            var timerLabel = Utilities.CreateText(pnlMenu.transform.Find("MenuNavigation"), "TimerLabel");
            var timerRect = timerLabel.GetComponent<RectTransform>();
            timerRect.pivot = new(0f, 1f);
            timerRect.anchorMin = timerRect.pivot;
            timerRect.anchorMax = timerRect.pivot;
            timerRect.anchoredPosition = new(32f, -16f);

            TimerText = timerLabel.GetComponent<Text>();
            TimerText.text = string.Empty;
            TimerText.alignment = TextAnchor.UpperLeft;
            TimerText.horizontalOverflow = HorizontalWrapMode.Overflow;
            TimerText.fontSize = 28;

            // Open PnlMenu
            if (!pnlMenu.active) UIManager.PnlNavigation.OnOptionClicked();

            // Open the character tab
            var menuSelect = UIManager.PnlMenu.menuSelect;
            menuSelect.SetOn(Il2CppAssets.Scripts.UI.MenuType.Role);

            // Turn off buttons besides girl and elfin selection
            foreach (var toggle in menuSelect.m_PcOptions)
            {
                if (toggle.name != "TglRole" && toggle.name != "TglElfin")
                {
                    toggle.isOn = false;
                    toggle.enabled = false;
                    toggle.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Resets everything before entering the chart.
        /// </summary>
        private static void UnsetPnlMenu()
        {
            CurrentTopGirlID = -1;
            CurrentTopElfinID = -1;

            GameObject.Destroy(TimerText.gameObject);
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

            UIManager.PnlStage.SelectAllTagAndJumpToAssginIndex(entry.MusicInfo.uid);
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
            Main.Dispatch(SetPnlMenu);

            while (Stopwatch.ElapsedMilliseconds < Constants.IntermissionTimeMS)
            {
                Main.Dispatch(Update);
                await Task.Delay(1000);
            }
            Stopwatch.Stop();

            UIManager.Debounce = false;
            Main.Dispatch(UnsetPnlMenu);
            Main.Dispatch(StartBattle);
            Active = false;
        }
    }
}
