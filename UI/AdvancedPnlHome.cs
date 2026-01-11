using Il2CppAssets.Scripts.PeroTools.Nice.Events;
using Il2CppAssets.Scripts.UI;
using Il2CppAssets.Scripts.UI.Controls;
using Il2CppAssets.Scripts.UI.Panels;
using Il2CppAssets.Scripts.UI.Panels.PnlRole;
using Il2CppAssets.Scripts.UI.Specials;
using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.UI
{
    internal static class AdvancedPnlHome
    {
        private static GameObject PnlHome;
        private static GameObject PnlRole;
        private static SelectableFancyPanel SelectableFancyPanel => PnlRole?.GetComponent<SelectableFancyPanel>();

        internal static bool Visible => Main.IsUIScene && Enabled && (PnlHome?.active ?? false);
        internal static bool Enabled { get; private set; } = false;

        private static List<List<Player>> Pages = new();
        private static int CurrentPageIndex;
        internal static List<Player> CurrentPage => Pages.Count > 0 ? Pages[CurrentPageIndex] : null;


        private static int LeftGirlIndex = -1;
        private static int RightGirlIndex = -1;

        private static int LeftElfinIndex = -3;
        private static int RightElfinIndex = -3;

        private static GameObject OriginalMuseShow;
        private static GameObject OriginalElfinShow;
        private static GameObject OriginalButton;

        private static Vector3 OriginalMusePosition;
        private static Vector3 OriginalMuseScale;

        private static Vector3 OriginalElfinPosition;
        private static Vector3 OriginalElfinScale;

        private static GameObject MiddleMuseShow;
        private static GameObject LeftMuseShow;
        private static GameObject RightMuseShow;

        private static float MuseShowGap = 6f;

        private static Vector3 MiddleMusePosition = new(2f, -0.9f, 100f);
        private static Vector3 LeftMusePosition = MiddleMusePosition + new Vector3(-MuseShowGap, -0.35f, 0f);
        private static Vector3 RightMusePosition = MiddleMusePosition + new Vector3(MuseShowGap, -0.35f, 0f);

        private static Vector3 MiddleMuseScale = new(0.75f, 0.75f, 0.75f);
        private static Vector3 SideMuseScale = new(0.6f, 0.6f, 0.6f);

        private static GameObject MiddleNameLabel;
        private static GameObject LeftNameLabel;
        private static GameObject RightNameLabel;

        private static Text MiddleName;
        private static Text LeftName;
        private static Text RightName;

        private static Vector2 NameSizeDelta = new(250f, 125f);
        private static int MiddleNameFontSize = 54;
        private static int SideNameFontSize = 52;

        private static Vector3 MiddleNamePosition = new(0f, 3.1f, 100f);
        private static Vector3 LeftNamePosition = MiddleNamePosition + new Vector3(-MuseShowGap, -0.65f, 0f);
        private static Vector3 RightNamePosition = MiddleNamePosition + new Vector3(MuseShowGap, -0.65f, 0f);

        private static GameObject MiddleInfoLabel;
        private static GameObject LeftInfoLabel;
        private static GameObject RightInfoLabel;

        private static Text MiddleInfo;
        private static Text LeftInfo;
        private static Text RightInfo;

        private const string InfoFormat = "<color=#" + Constants.Yellow + ">[ RL {0} ]</color>";
        private const string PingInfoFormat = "[ <color=#{0}>{1}ms</color> ]";
        private static Vector3 InfoTextGap = new(0f, 0.3f, 0f);
        private static int MiddleInfoFontSize = 36;
        private static int SideInfoFontSize = 34;

        private static Vector3 MiddleInfoPosition = MiddleNamePosition + InfoTextGap;
        private static Vector3 LeftInfoPosition = LeftNamePosition + InfoTextGap;
        private static Vector3 RightInfoPosition = RightNamePosition + InfoTextGap;

        private static GameObject LeftButton;
        private static GameObject RightButton;

        private static Color ButtonColor = new(0.8f, 0.2f, 0.8f, 1f);
        private static float ButtonSize = 0.7f;
        private static float ButtonGap = 8.8f;

        private static Vector3 LeftButtonPosition = new(-ButtonGap, 0f, 100f);
        private static Vector3 RightButtonPosition = new(ButtonGap, 0f, 100f);

        private static Vector3 LeftButtonScale = new(-ButtonSize, ButtonSize, ButtonSize);
        private static Vector3 RightButtonScale = new(ButtonSize, ButtonSize, ButtonSize);

        private static GameObject MiddleElfinShow;
        private static GameObject LeftElfinShow;
        private static GameObject RightElfinShow;

        private static Vector3 MiddleElfinPosition = new(2.5f, 1.5f, 100f);
        private static Vector3 LeftElfinPosition = MiddleElfinPosition + new Vector3(-1 * (MuseShowGap + 0.5f), -0.3f, 0f);
        private static Vector3 RightElfinPosition = MiddleElfinPosition + new Vector3(MuseShowGap - 0.5f, -0.3f, 0f);

        private static Vector3 MiddleElfinScale = new(-1.5f, 1.5f, 1.5f);
        private static Vector3 SideElfinScale = new(-1.2f, 1.2f, 1.2f);

        private static async System.Threading.Tasks.Task EndPlayerSpeak(DefaultTalkBubble bubble)
        {
            await System.Threading.Tasks.Task.Delay(Constants.PlayerSpeechBubbleDurationMS);
            Main.Dispatcher.Enqueue(bubble.EndTalk);
        }

        internal static void PlayerSpeak(Player player, string msg)
        {
            if (!Enabled) return;

            GameObject museShow = CurrentPage.IndexOf(player) switch
            {
                0 => LeftMuseShow,
                1 => RightMuseShow,
                _ when player == PlayerManager.LocalPlayer => MiddleMuseShow,
                _ => null
            };
            if (museShow == null) return;

            var bubble = museShow.transform.Find("FirstTwnTalkBubble").gameObject.GetComponent<DefaultTalkBubble>();
            if (bubble == null) return;

            bubble.SetTalkTxt(msg);
            _ = EndPlayerSpeak(bubble);
        }

        private static string GetName(Player player)
        {
            return $"{player.MultiplayerStats.Name} {(player.AreFriends(PlayerManager.LocalPlayer) ? $"<color=#{Constants.Red}>♥</color>" : $"<color=#{Constants.Green}>+</color>")}";
        }

        private static string GetInfo(Player player)
        {
            return InputManager.PingMode ? String.Format(PingInfoFormat, Utilities.GetPingColor(player.PingMS), player.PingMS) : String.Format(InfoFormat, player.MoeStats.RL);
        }

        private static void ReplaceGirl(GameObject museShow, int girlIndex)
        {
            if (SelectableFancyPanel is null) return;

            var prefab = museShow.transform.Find("ShowLocalization/SpinePerfab_other").gameObject;
            var museShowComponent = prefab.GetComponent<MuseShow>();

            var subControl = SelectableFancyPanel.GetCellComponent<PnlRoleSubControl>(girlIndex);
            if (subControl is null) return;
            var charApply = subControl.characterApply;
            if (charApply is null) return;

            for (int i = 0; i < prefab.transform.childCount; i++)
            {
                GameObject.Destroy(prefab.transform.GetChild(i).gameObject);
            }

            var newShow = GameObject.Instantiate(charApply.gameObject, prefab.transform);
            newShow.GetComponent<MeshRenderer>().sortingOrder = 0;
            museShowComponent.m_MuseShow = newShow;
        }

        private static void ReplaceElfin(GameObject elfinShow, int elfinIndex)
        {
            if (elfinIndex < 0)
            {
                elfinShow.SetActive(false);
                return;
            }


        }

        internal static void UpdateCurrentPage(bool infoOnly = false)
        {
            if (!Enabled || !Main.IsUIScene || !LobbyManager.IsInLobby) return;

            if (CurrentPageIndex > Pages.Count - 1) CurrentPageIndex = 0;
            else if (CurrentPageIndex < 0) CurrentPageIndex = Pages.Count - 1;

            bool leftPresent = Pages.Count > 0 && CurrentPage.Count >= 1;
            bool rightPresent = Pages.Count > 0 && CurrentPage.Count == 2;

            if (leftPresent)
            {
                LeftInfo.text = GetInfo(CurrentPage[0]);
            }
            if (rightPresent)
            {
                RightInfo.text = GetInfo(CurrentPage[1]);
            }

            MiddleInfo.text = GetInfo(PlayerManager.LocalPlayer);

            if (infoOnly) return;

            bool buttonsEnabled = Pages.Count > 1;
            LeftButton.SetActive(buttonsEnabled);
            RightButton.SetActive(buttonsEnabled);

            ToggleLeft(leftPresent);
            ToggleRight(rightPresent);

            if (leftPresent)
            {
                Player player = CurrentPage[0];

                var girlIndex = Settings.Config.FavGirlMode && player.MultiplayerStats.FavGirlIndex >= 0 ? player.MultiplayerStats.FavGirlIndex : player.MultiplayerStats.GirlIndex;
                if (girlIndex != LeftGirlIndex)
                {
                    ReplaceGirl(LeftMuseShow, girlIndex);
                    LeftGirlIndex = girlIndex;
                }

                var elfinIndex = Settings.Config.FavGirlMode && player.MultiplayerStats.FavElfinIndex >= 0 ? player.MultiplayerStats.FavElfinIndex : player.MultiplayerStats.ElfinIndex;
                if (elfinIndex != LeftElfinIndex && Settings.Config.ShowOtherElfins)
                {
                    ReplaceElfin(LeftElfinShow, elfinIndex);
                    LeftElfinIndex = elfinIndex;
                }

                LeftName.text = GetName(player);
            }

            if (rightPresent)
            {
                Player player = CurrentPage[1];

                var girlIndex = Settings.Config.FavGirlMode && player.MultiplayerStats.FavGirlIndex >= 0 ? player.MultiplayerStats.FavGirlIndex : player.MultiplayerStats.GirlIndex;
                if (girlIndex != RightGirlIndex)
                {
                    ReplaceGirl(RightMuseShow, girlIndex);
                    RightGirlIndex = girlIndex;
                }

                var elfinIndex = Settings.Config.FavGirlMode && player.MultiplayerStats.FavElfinIndex >= 0 ? player.MultiplayerStats.FavElfinIndex : player.MultiplayerStats.ElfinIndex;
                if (elfinIndex != RightElfinIndex && Settings.Config.ShowOtherElfins)
                {
                    ReplaceElfin(RightElfinShow, elfinIndex);
                    RightElfinIndex = elfinIndex;
                }

                RightName.text = GetName(player);
            }
        }

        internal static void UpdateAllPages()
        {
            if (!Enabled || !Main.IsUIScene || !LobbyManager.IsInLobby) return;

            Pages.Clear();
            var lobby = LobbyManager.LocalLobby;

            if (lobby.Players.Count > 1)
            {
                int i = 0;
                foreach (string playerUid in lobby.Players)
                {
                    Player player = PlayerManager.GetCachedPlayer(playerUid);
                    if (player != null && player == PlayerManager.LocalPlayer) continue;

                    if (i % 2 == 0) Pages.Add(new());
                    Pages[Pages.Count - 1].Add(player);
                    i++;
                }
            }
            UpdateCurrentPage();
        }

        private static void LeftButtonClick()
        {
            CurrentPageIndex--;
            UpdateCurrentPage();
        }

        private static void RightButtonClick()
        {
            CurrentPageIndex++;
            UpdateCurrentPage();
        }

        private static void ToggleLeft(bool enabled)
        {
            if (!Enabled) return;

            LeftMuseShow.SetActive(enabled);
            LeftNameLabel.SetActive(enabled);
            LeftInfoLabel.SetActive(enabled);

            LeftElfinShow.SetActive(enabled && Settings.Config.ShowOtherElfins);
        }

        private static void ToggleRight(bool enabled)
        {
            if (!Enabled) return;

            RightMuseShow.SetActive(enabled);
            RightNameLabel.SetActive(enabled);
            RightInfoLabel.SetActive(enabled);

            RightElfinShow.SetActive(enabled && Settings.Config.ShowOtherElfins);
        }

        internal static void Enable()
        {
            if (!Main.IsUIScene) return;

            PnlHome = GameObject.Find("UI/Standerd/PnlHome");
            PnlRole = GameObject.Find("UI/Standerd/PnlMenu/Panels/PnlRole");
            OriginalMuseShow = GameObject.Find("UI/Standerd/PnlHome/MuseShow");
            OriginalElfinShow = GameObject.Find("UI/Standerd/PnlHome/ElfinShow");
            OriginalButton = GameObject.Find("UI/Standerd/PnlMenu/Panels/PnlRole/MainShow/FancyScrollView/BtnPrevious");

            if (OriginalMuseShow is null || OriginalButton is null) return;

            OriginalMusePosition = OriginalMuseShow.transform.position;
            OriginalMuseScale = OriginalMuseShow.transform.localScale;

            OriginalElfinPosition = OriginalElfinShow.transform.position;
            OriginalElfinScale = OriginalElfinShow.transform.localScale;

            Pages = new();
            CurrentPageIndex = 0;

            MiddleMuseShow = OriginalMuseShow;
            LeftMuseShow = GameObject.Instantiate(MiddleMuseShow, MiddleMuseShow.transform.parent);
            RightMuseShow = GameObject.Instantiate(MiddleMuseShow, MiddleMuseShow.transform.parent);

            var leftComp = LeftMuseShow.transform.Find("ShowLocalization/SpinePerfab_other").gameObject.GetComponent<MuseShow>();
            var rightComp = RightMuseShow.transform.Find("ShowLocalization/SpinePerfab_other").gameObject.GetComponent<MuseShow>();

            leftComp.m_MuseShow = LeftMuseShow;
            rightComp.m_MuseShow= RightMuseShow;

            LeftMuseShow.name = "LeftMuseShow";
            RightMuseShow.name = "RightMuseShow";

            MiddleMuseShow.transform.position = MiddleMusePosition;
            LeftMuseShow.transform.position = LeftMusePosition;
            RightMuseShow.transform.position = RightMusePosition;

            MiddleMuseShow.transform.localScale = MiddleMuseScale;
            LeftMuseShow.transform.localScale = SideMuseScale;
            RightMuseShow.transform.localScale = SideMuseScale;

            MiddleNameLabel = Utilities.CreateText(MiddleMuseShow.transform.parent, "NameMiddle");
            LeftNameLabel = Utilities.CreateText(LeftMuseShow.transform.parent, "NameLeft");
            RightNameLabel = Utilities.CreateText(RightMuseShow.transform.parent, "NameRight");

            MiddleNameLabel.transform.position = MiddleNamePosition;
            LeftNameLabel.transform.position = LeftNamePosition;
            RightNameLabel.transform.position = RightNamePosition;

            MiddleNameLabel.GetComponent<RectTransform>().sizeDelta = NameSizeDelta;
            LeftNameLabel.GetComponent<RectTransform>().sizeDelta = NameSizeDelta;
            RightNameLabel.GetComponent<RectTransform>().sizeDelta = NameSizeDelta;

            MiddleNameLabel.AddComponent<Button>().onClick.AddListener((UnityAction)new Action(() => _ = UIManager.OpenProfileWindow(PlayerManager.LocalPlayer, false)));
            LeftNameLabel.AddComponent<Button>().onClick.AddListener((UnityAction)new Action(() => _ = UIManager.OpenProfileWindow(CurrentPage[0])));
            RightNameLabel.AddComponent<Button>().onClick.AddListener((UnityAction)new Action(() => _ = UIManager.OpenProfileWindow(CurrentPage[1])));

            MiddleName = MiddleNameLabel.GetComponent<Text>();
            LeftName = LeftNameLabel.GetComponent<Text>();
            RightName = RightNameLabel.GetComponent<Text>();

            MiddleName.fontSize = MiddleNameFontSize;
            LeftName.fontSize = SideNameFontSize;
            RightName.fontSize = SideNameFontSize;

            MiddleName.horizontalOverflow = HorizontalWrapMode.Overflow;
            LeftName.horizontalOverflow = HorizontalWrapMode.Overflow;
            RightName.horizontalOverflow = HorizontalWrapMode.Overflow;

            MiddleName.alignment = TextAnchor.LowerCenter;
            LeftName.alignment = TextAnchor.LowerCenter;
            RightName.alignment = TextAnchor.LowerCenter;

            MiddleInfoLabel = Utilities.CreateText(MiddleNameLabel.transform.parent, "InfoMiddle");
            LeftInfoLabel = Utilities.CreateText(LeftNameLabel.transform.parent, "InfoLeft");
            RightInfoLabel = Utilities.CreateText(RightNameLabel.transform.parent, "InfoRight");

            MiddleInfoLabel.transform.position = MiddleInfoPosition;
            LeftInfoLabel.transform.position = LeftInfoPosition;
            RightInfoLabel.transform.position = RightInfoPosition;

            MiddleInfo = MiddleInfoLabel.GetComponent<Text>();
            LeftInfo = LeftInfoLabel.GetComponent<Text>();
            RightInfo = RightInfoLabel.GetComponent<Text>();

            MiddleInfo.fontSize = MiddleInfoFontSize;
            LeftInfo.fontSize = SideInfoFontSize;
            RightInfo.fontSize = SideInfoFontSize;

            MiddleInfo.horizontalOverflow = HorizontalWrapMode.Overflow;
            LeftInfo.horizontalOverflow = HorizontalWrapMode.Overflow;
            RightInfo.horizontalOverflow = HorizontalWrapMode.Overflow;

            MiddleInfo.alignment = TextAnchor.MiddleCenter;
            LeftInfo.alignment = TextAnchor.MiddleCenter;
            RightInfo.alignment = TextAnchor.MiddleCenter;

            MiddleElfinShow = OriginalElfinShow;
            LeftElfinShow = GameObject.Instantiate(MiddleElfinShow, MiddleElfinShow.transform.parent);
            RightElfinShow = GameObject.Instantiate(MiddleElfinShow, MiddleElfinShow.transform.parent);

            Component.Destroy(LeftElfinShow.GetComponent<OnActivate>());
            Component.Destroy(RightElfinShow.GetComponent<OnActivate>());

            Component.Destroy(LeftElfinShow.GetComponent<DesElfinIfLessThanZero>());
            Component.Destroy(RightElfinShow.GetComponent<DesElfinIfLessThanZero>());

            LeftElfinShow.name = "LeftElfinShow";
            RightElfinShow.name = "RightElfinShow";

            MiddleElfinShow.transform.localScale = MiddleElfinScale;
            LeftElfinShow.transform.localScale = SideElfinScale;
            RightElfinShow.transform.localScale = SideElfinScale;

            MiddleElfinShow.transform.position = MiddleElfinPosition;
            LeftElfinShow.transform.position = LeftElfinPosition;
            RightElfinShow.transform.position = RightElfinPosition;

            LeftButton = GameObject.Instantiate(OriginalButton, OriginalMuseShow.transform.parent);
            RightButton = GameObject.Instantiate(OriginalButton, OriginalMuseShow.transform.parent);

            LeftButton.name = "LeftButton";
            RightButton.name = "RightButton";

            LeftButton.transform.position = LeftButtonPosition;
            RightButton.transform.position = RightButtonPosition;

            LeftButton.transform.localScale = LeftButtonScale;
            RightButton.transform.localScale = RightButtonScale;

            LeftButton.GetComponent<Image>().color = ButtonColor;
            RightButton.GetComponent<Image>().color = ButtonColor;

            var buttonLeft = LeftButton.GetComponent<Button>();
            var buttonRight = RightButton.GetComponent<Button>();

            buttonLeft.onClick.RemoveAllListeners();
            buttonRight.onClick.RemoveAllListeners();

            buttonLeft.onClick.AddListener((UnityAction)new Action(LeftButtonClick));
            buttonRight.onClick.AddListener((UnityAction)new Action(RightButtonClick));

            MiddleName.text = PlayerManager.LocalPlayer.MultiplayerStats.Name;
            MiddleInfo.text = String.Format(InfoFormat, PlayerManager.LocalPlayer.MoeStats.RL);

            Enabled = true;
            UpdateAllPages();
        }

        internal static void Disable()
        {
            Enabled = false;
            try
            {
                Pages.Clear();
                OriginalElfinShow.SetActive(true);

                OriginalMuseShow.transform.position = OriginalMusePosition;
                OriginalMuseShow.transform.localScale = OriginalMuseScale;

                OriginalElfinShow.transform.position = OriginalElfinPosition;
                OriginalElfinShow.transform.localScale = OriginalElfinScale;

                MiddleMuseShow = null;
                GameObject.Destroy(LeftMuseShow);
                GameObject.Destroy(RightMuseShow);

                GameObject.Destroy(MiddleNameLabel);
                GameObject.Destroy(LeftNameLabel);
                GameObject.Destroy(RightNameLabel);

                GameObject.Destroy(MiddleInfoLabel);
                GameObject.Destroy(LeftInfoLabel);
                GameObject.Destroy(RightInfoLabel);

                MiddleElfinShow = null;
                GameObject.Destroy(LeftElfinShow);
                GameObject.Destroy(RightElfinShow);

                GameObject.Destroy(LeftButton);
                GameObject.Destroy(RightButton);

            } catch (Exception ex)
            {
                Main.Logger.Error(ex);
            }
        }
    }
}
