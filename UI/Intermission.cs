using Il2CppAccount;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Newtonsoft.Json.Linq;
using Multiplayer.Managers;
using Multiplayer.Static;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Multiplayer.Data.Lobbies;
using System.Net.Http.Json;
using System.Text.Json;

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

        internal static string GetElfin(int elfinId)
        {
            if (elfinId < 0) return string.Empty;

            return Singleton<Il2CppAssets.Scripts.PeroTools.Managers.ConfigManager>.instance.GetJson("elfin", true)[elfinId]["name"].ToString();
        }

        private static void Update()
        {
            if (!Active || TimerText == null || !LobbyManager.IsInLobby) return;

            var entry = LobbyManager.LocalLobby.CurrentPlaylistEntry;
            var chartLabel = ChartManager.GetNiceChartName(entry.MusicInfo, entry.Difficulty);
            var topGirl = GetGirl(CurrentTopGirlID);
            var topElfin = GetElfin(CurrentTopElfinID);
            var secondsLeft = Constants.IntermissionTimeMS/1000 - Stopwatch.Elapsed.TotalSeconds;

            TimerText.text = String.Format(
                Localization.Get("Lobby", "IntermissionLabel").ToString(),
                Constants.Yellow, chartLabel, 
                Constants.Blue, topGirl, topElfin,
                Constants.Yellow, (int)secondsLeft
            );
        }

        private static async Task GetTopRank(PlaylistEntry entry)
        {
            var response = await Client.GetAsync($"https://api.musedash.moe/rank/{entry.MusicInfo.uid}/{entry.Difficulty-1}/all", true, false);
            if (response == null) return;

            var scores = await response.Content.ReadFromJsonAsync<List<List<object>>>();
            var topScore = scores.First();
            CurrentTopGirlID = (int)topScore[6];
            CurrentTopElfinID = (int)topScore[7];
        }

        private static void SetPnlMenu()
        {
            var pnlMenu = UIManager.PnlMenu.gameObject;
            BtnBack = pnlMenu.transform.Find("MenuNavigation/BtnBack").gameObject;
            BtnBack.SetActive(false);

            var timerLabel = Utilities.CreateText(pnlMenu.transform.Find("MenuNavigation"), "TimerLabel");
            timerLabel.GetComponent<RectTransform>().pivot = Vector2.one;
            timerLabel.transform.position = BtnBack.transform.position + BtnLabelOffset;

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

            // Get ranks from the peropero server and update top combo
            _ = GetTopRank(LobbyManager.LocalLobby.CurrentPlaylistEntry);
        }

        private static void UnsetPnlMenu()
        {
            GameObject.Destroy(TimerText.gameObject);
            UIManager.MainLobbyDisplay.Destroy();
            UIManager.ChatLobbyDisplay.Destroy();
            PnlHomeExtension.Disable();

            BtnBack.SetActive(true);
            UIManager.PnlPreparation.gameObject.SetActive(true);
            UIManager.PnlPreparation.OnBattleStart();
        }

        internal static async Task Start()
        {
            if (Active || !UIManager.Initialized || !LobbyManager.IsInLobby) return;
            Active = true;

            UIManager.Debounce = true;
            Stopwatch.Restart();
            Main.Dispatcher.Enqueue(SetPnlMenu);

            while (Stopwatch.ElapsedMilliseconds < Constants.IntermissionTimeMS)
            {
                Update();
                await Task.Delay(1000);
            }
            Stopwatch.Stop();

            UIManager.Debounce = false;
            Main.Dispatcher.Enqueue(UnsetPnlMenu);
            Active = false;
        }
    }
}
