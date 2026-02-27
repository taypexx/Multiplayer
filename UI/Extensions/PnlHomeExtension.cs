using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.UI;
using Il2CppAssets.Scripts.UI.Panels;
using Il2CppAssets.Scripts.UI.Panels.PnlRole;
using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.UI.Extensions
{
    internal static class PnlHomeExtension
    {
        private static GameObject PnlHome;
        private static PnlRole PnlRole;
        private static SelectableFancyPanel GirlFancyPanel;

        internal static bool Visible => Main.IsUIScene && Enabled && (PnlHome?.active ?? false);
        internal static bool Enabled { get; private set; } = false;

        private static List<List<Player>> Pages = new();
        private static int CurrentPageIndex;
        internal static List<Player> CurrentPage => Pages.Count > 0 ? Pages[CurrentPageIndex] : null;


        private static int LeftGirlIndex = -1;
        private static int RightGirlIndex = -1;

        private static GameObject OriginalMuseShow;
        private static GameObject OriginalElfinShow;
        private static GameObject OriginalButton;

        private static Vector3 OriginalMusePosition;
        private static Vector3 OriginalMuseScale;

        private static GameObject MiddleMuseShow;
        private static GameObject LeftMuseShow;
        private static GameObject RightMuseShow;

        private static float MuseShowGap = 6f;

        private static Vector3 MiddleMusePosition = new(2.3f, -0.9f, 100f);
        private static Vector3 LeftMusePosition = MiddleMusePosition + new Vector3(-MuseShowGap, -0.75f, 0f);
        private static Vector3 RightMusePosition = MiddleMusePosition + new Vector3(MuseShowGap, -0.75f, 0f);

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
        private static Vector3 LeftNamePosition = MiddleNamePosition + new Vector3(-MuseShowGap, -1f, 0f);
        private static Vector3 RightNamePosition = MiddleNamePosition + new Vector3(MuseShowGap, -1f, 0f);

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
        private static float ButtonSize = 0.6f;
        private static float ButtonGap = 8.8f;

        private static Vector3 LeftButtonPosition = new(-ButtonGap, 0f, 100f);
        private static Vector3 RightButtonPosition = new(ButtonGap, 0f, 100f);

        private static Vector3 LeftButtonScale = new(-ButtonSize, ButtonSize, ButtonSize);
        private static Vector3 RightButtonScale = new(ButtonSize, ButtonSize, ButtonSize);

        private static CancellationTokenSource[] PlayerSpeakCTS = new CancellationTokenSource[3];

        /// <summary>
        /// Turns off the speech bubble.
        /// </summary>
        internal static async Task PlayerEndSpeak(Il2CppAssets.Scripts.UI.Controls.DefaultTalkBubble bubble, int msgDurationMs, CancellationToken token)
        {
            try
            {
                await Task.Delay(msgDurationMs, token);
                token.ThrowIfCancellationRequested();

                Main.Dispatch(bubble.EndTalk);

                await Task.Delay(300, token);
                token.ThrowIfCancellationRequested();

                Main.Dispatch(() => bubble.gameObject.SetActive(false));
            }
            catch (OperationCanceledException) { }
        }

        /// <summary>
        /// Displays a nice bubble next to the character with the <paramref name="msg"/> and plays an <see cref="Expression"/> according to the said text.
        /// </summary>
        /// <param name="player">A <see cref="Player"/> who said the <paramref name="msg"/>.</param>
        /// <param name="msg">Yapping.</param>
        internal static void PlayerSpeak(Player player, string msg)
        {
            if (!Enabled) return;

            GameObject museShow = CurrentPage is null ? MiddleMuseShow : CurrentPage.IndexOf(player) switch
            {
                0 => LeftMuseShow,
                1 => RightMuseShow,
                _ when player == PlayerManager.LocalPlayer => MiddleMuseShow,
                _ => null
            };
            if (museShow == null) return;

            try
            {
                // Play an expression
                var charExpress = museShow.transform.Find("BtnInteraction").GetComponent<Il2CppAssets.Scripts.UI.Controls.CharacterExpression>();
                var express = ExpressionDeterminer.Determine(charExpress.expressionContainer, msg);
                if (express != null)
                {
                    charExpress.m_CharacterApply.PlayCharacterApply(express.animName, null);
                }
            }
            catch (Exception ex) { Main.Log(ex); }

            var bubble = museShow.transform.Find("FirstTwnTalkBubble").gameObject.GetComponent<Il2CppAssets.Scripts.UI.Controls.DefaultTalkBubble>();
            if (bubble == null) return;

            bubble.gameObject.SetActive(true);
            bubble.SetTalkTxt(msg);

            // Calculate duration of the message
            var msgDurationMs = Math.Clamp(
                msg.Split(" ").Length * 500 + 800
                + msg.Count(c => c == ',') * 200
                + msg.Count(c => c == '.') * 400
                + msg.Count(c => c == '!') * 200
                + msg.Count(c => c == '?') * 300,
                1200, 10000
            );

            var ctsIndex = player == PlayerManager.LocalPlayer ? 0 : CurrentPage.IndexOf(player) == 0 ? 1 : 2;

            var prevCts = PlayerSpeakCTS[ctsIndex];
            if (prevCts != null) prevCts.Cancel();

            var newCts = new CancellationTokenSource();
            PlayerSpeakCTS[ctsIndex] = newCts;

            _ = PlayerEndSpeak(bubble, msgDurationMs, newCts.Token);
        }

        /// <summary>
        /// Replaces the girl model.
        /// </summary>
        private static void ReplaceGirl(GameObject museShow, int girlIndex)
        {
            if (GirlFancyPanel is null || museShow == OriginalMuseShow || girlIndex < 0) return;

            var prefab = museShow.transform.Find("ShowLocalization/SpinePerfab_other").gameObject;
            var museShowComponent = prefab.GetComponent<MuseShow>();

            var orderIndex = PnlRole.m_ConfigCharacter.GetCharacterInfoByIndex(girlIndex).order - 1;
            var subControl = GirlFancyPanel.GetCellComponent<PnlRoleSubControl>(orderIndex);
            if (subControl is null) return;

            if (!subControl.m_Init) subControl.Init();

            var charApply = subControl.characterApply;
            if (charApply is null) return;

            for (int i = 0; i < prefab.transform.childCount; i++)
            {
                UnityEngine.Object.Destroy(prefab.transform.GetChild(i).gameObject);
            }

            var newShow = UnityEngine.Object.Instantiate(charApply.gameObject, prefab.transform);
            newShow.GetComponent<MeshRenderer>().sortingOrder = 0;
            museShowComponent.m_MuseShow = newShow;

            // Initialize expressions
            var charExpress = museShow.transform.Find("BtnInteraction").GetComponent<Il2CppAssets.Scripts.UI.Controls.CharacterExpression>();
            if (charExpress.expressionContainer.m_Expressions == null)
            {
                charExpress.expressionContainer.SetExpression(
                    Singleton<Il2CppAssets.Scripts.PeroTools.Managers.ConfigManager>.instance
                        .GetConfigObject<DBConfigCharacter>()
                        .GetCharacterInfoByIndex(museShow == LeftMuseShow ? LeftGirlIndex : RightGirlIndex)
                );
                charExpress.m_CharacterApply = charApply;
            }

            // Remove other visual stuff from characters

            switch (girlIndex)
            {
                case 31:
                    var bloodheirHandler = newShow.GetComponent<Il2CppAssets.Scripts.UI.Specials.BloodheirTransformGenerator>();
                    if (bloodheirHandler != null && bloodheirHandler.m_BloodheirTransformObj != null)
                        GameObject.Destroy(bloodheirHandler.m_BloodheirTransformObj);
                    break;
                case 33:
                    var diverHandler = newShow.GetComponent<Il2CppAssets.Scripts.UI.Specials.DiverBuroHandler>();
                    if (diverHandler != null && diverHandler.m_PnlDaveFishViewObj != null) 
                        GameObject.Destroy(diverHandler.m_PnlDaveFishViewObj);
                    break;
                default:
                    break;
            }
        }

        private static string GetName(Player player)
        {
            return $"{player.MultiplayerStats.Name} {(player.AreFriends(PlayerManager.LocalPlayer) ? $"<color=#{Constants.Red}>♥</color>" : $"<color=#{Constants.Green}>+</color>")}";
        }

        private static string GetInfo(Player player)
        {
            return InputManager.PingMode ? string.Format(PingInfoFormat, Utilities.GetPingColor(player.PingMS), player.PingMS) : string.Format(InfoFormat, player.MoeStats.RL);
        }

        /// <summary>
        /// Updates characters and elfins on the current page.
        /// </summary>
        /// <param name="infoOnly">Whether to only update the info on top of the character.</param>
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
                LeftName.text = GetName(player);

                var girlIndex = Settings.Config.FavGirlMode && player.MultiplayerStats.FavGirlIndex >= 0 ? player.MultiplayerStats.FavGirlIndex : player.MultiplayerStats.GirlIndex;
                if (girlIndex != LeftGirlIndex)
                {
                    LeftGirlIndex = girlIndex;
                    ReplaceGirl(LeftMuseShow, girlIndex);
                }
            }
            else LeftGirlIndex = -1;

            if (rightPresent)
            {
                Player player = CurrentPage[1];
                RightName.text = GetName(player);

                var girlIndex = Settings.Config.FavGirlMode && player.MultiplayerStats.FavGirlIndex >= 0 ? player.MultiplayerStats.FavGirlIndex : player.MultiplayerStats.GirlIndex;
                if (girlIndex != RightGirlIndex)
                {
                    RightGirlIndex = girlIndex;
                    ReplaceGirl(RightMuseShow, girlIndex);
                }
            }
            else RightGirlIndex = -1;
        }

        /// <summary>
        /// Recalculates players in the pages and updates the current page.
        /// </summary>
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

                    if (i % 2 == 0) Pages.Add(new()); // Create new page if the player index is even
                    Pages[Pages.Count - 1].Add(player);
                    i++;
                }
            }
            UpdateCurrentPage();
        }

        private static void LeftButtonClick()
        {
            CurrentPageIndex--;
            SoundManager.PlaySwitch();
            UpdateCurrentPage();
        }

        private static void RightButtonClick()
        {
            CurrentPageIndex++;
            SoundManager.PlaySwitch();
            UpdateCurrentPage();
        }

        /// <summary>
        /// Toggles the left character and elfin.
        /// </summary>
        private static void ToggleLeft(bool enabled)
        {
            if (!Enabled) return;

            LeftMuseShow.SetActive(enabled);
            LeftNameLabel.SetActive(enabled);
            LeftInfoLabel.SetActive(enabled);
        }

        /// <summary>
        /// Toggles the right character and elfin.
        /// </summary>
        private static void ToggleRight(bool enabled)
        {
            if (!Enabled) return;

            RightMuseShow.SetActive(enabled);
            RightNameLabel.SetActive(enabled);
            RightInfoLabel.SetActive(enabled);
        }

        /// <summary>
        /// Enables the extension.
        /// </summary>
        internal static void Create()
        {
            if (!Main.IsUIScene) return;

            PnlHome = GameObject.Find("UI/Standerd/PnlHome");
            PnlRole = GameObject.Find("UI/Standerd/PnlMenu/Panels/PnlRole").GetComponent<PnlRole>();
            GirlFancyPanel = PnlRole.fancyPanel;

            var bgSwitchBtn = PnlHome.transform.Find("PnlBgSwitchFsv");
            if (bgSwitchBtn != null) bgSwitchBtn.gameObject.SetActive(false);

            OriginalMuseShow = GameObject.Find("UI/Standerd/PnlHome/MuseShow");
            OriginalElfinShow = GameObject.Find("UI/Standerd/PnlHome/ElfinShow");
            OriginalButton = GameObject.Find("UI/Standerd/PnlMenu/Panels/PnlRole/MainShow/FancyScrollView/BtnPrevious");

            if (OriginalMuseShow is null || OriginalButton is null) return;
            if (!PnlRole.m_IsInit)
            {
                PnlRole.Init();
            }
            for (int i = 0; i < GirlFancyPanel.m_FancyScrollView.itemCount; i++) //GlobalDataBase.dbConfig.m_ConfigDic["character"].count
            {
                GirlFancyPanel.m_FancyScrollView.ScrollToDataIndex(i, 0, true);
            }
            GirlFancyPanel.m_FancyScrollView.ScrollToDataIndex(PnlRole.m_ConfigCharacter.GetCharacterInfoByIndex(DataHelper.selectedRoleIndex).order - 1, 0, true);

            OriginalMuseShow.transform.Find("BtnInteraction").gameObject.SetActive(false);
            OriginalMuseShow.transform.Find("FirstTwnTalkBubble").gameObject.SetActive(false);

            OriginalMusePosition = OriginalMuseShow.transform.position;
            OriginalMuseScale = OriginalMuseShow.transform.localScale;

            OriginalElfinShow.SetActive(false);

            Pages = new();
            CurrentPageIndex = 0;

            MiddleMuseShow = OriginalMuseShow;
            LeftMuseShow = UnityEngine.Object.Instantiate(OriginalMuseShow, OriginalMuseShow.transform.parent);
            RightMuseShow = UnityEngine.Object.Instantiate(OriginalMuseShow, OriginalMuseShow.transform.parent);

            LeftMuseShow.SetActive(true);
            RightMuseShow.SetActive(true);

            var leftComp = LeftMuseShow.transform.Find("ShowLocalization/SpinePerfab_other").gameObject.GetComponent<MuseShow>();
            var rightComp = RightMuseShow.transform.Find("ShowLocalization/SpinePerfab_other").gameObject.GetComponent<MuseShow>();

            leftComp.gameObject.SetActive(true);
            rightComp.gameObject.SetActive(true);

            leftComp.m_MuseShow = LeftMuseShow;
            rightComp.m_MuseShow = RightMuseShow;

            LeftMuseShow.name = "LeftMuseShow";
            RightMuseShow.name = "RightMuseShow";

            MiddleMuseShow.transform.position = MiddleMusePosition;
            LeftMuseShow.transform.position = LeftMusePosition;
            RightMuseShow.transform.position = RightMusePosition;

            MiddleMuseShow.transform.localScale = MiddleMuseScale;
            LeftMuseShow.transform.localScale = SideMuseScale;
            RightMuseShow.transform.localScale = SideMuseScale;

            MiddleNameLabel = Utilities.CreateText(MiddleMuseShow.transform.parent, "NameMiddle", true);
            LeftNameLabel = Utilities.CreateText(LeftMuseShow.transform.parent, "NameLeft", true);
            RightNameLabel = Utilities.CreateText(RightMuseShow.transform.parent, "NameRight", true);

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

            MiddleInfoLabel = Utilities.CreateText(MiddleNameLabel.transform.parent, "InfoMiddle", true);
            LeftInfoLabel = Utilities.CreateText(LeftNameLabel.transform.parent, "InfoLeft", true);
            RightInfoLabel = Utilities.CreateText(RightNameLabel.transform.parent, "InfoRight", true);

            MiddleInfoLabel.transform.position = MiddleInfoPosition;
            LeftInfoLabel.transform.position = LeftInfoPosition;
            RightInfoLabel.transform.position = RightInfoPosition;

            MiddleInfoLabel.AddComponent<Button>().onClick.AddListener((UnityAction)new Action(() => UIManager.OpenProfilePage(PlayerManager.LocalPlayerUid)));
            LeftInfoLabel.AddComponent<Button>().onClick.AddListener((UnityAction)new Action(() => UIManager.OpenProfilePage(CurrentPage[0].Uid)));
            RightInfoLabel.AddComponent<Button>().onClick.AddListener((UnityAction)new Action(() => UIManager.OpenProfilePage(CurrentPage[1].Uid)));

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

            LeftButton = UnityEngine.Object.Instantiate(OriginalButton, OriginalMuseShow.transform.parent);
            RightButton = UnityEngine.Object.Instantiate(OriginalButton, OriginalMuseShow.transform.parent);

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
            MiddleInfo.text = string.Format(InfoFormat, PlayerManager.LocalPlayer.MoeStats.RL);

            Enabled = true;
            UpdateAllPages();
            ReplaceGirl(LeftMuseShow, LeftGirlIndex);
            ReplaceGirl(RightMuseShow, RightGirlIndex);
        }

        /// <summary>
        /// Disables the extension.
        /// </summary>
        internal static void Destroy()
        {
            Enabled = false;
            try
            {
                var bgSwitchBtn = PnlHome.transform.Find("PnlBgSwitchFsv");
                if (bgSwitchBtn != null) bgSwitchBtn.gameObject.SetActive(true);

                OriginalElfinShow.SetActive(true);
                OriginalMuseShow.transform.Find("BtnInteraction").gameObject.SetActive(true);
            }
            catch { }
            try
            {
                Pages.Clear();

                OriginalMuseShow.transform.position = OriginalMusePosition;
                OriginalMuseShow.transform.localScale = OriginalMuseScale;

                MiddleMuseShow = null;
                UnityEngine.Object.Destroy(LeftMuseShow);
                UnityEngine.Object.Destroy(RightMuseShow);

                UnityEngine.Object.Destroy(MiddleNameLabel);
                UnityEngine.Object.Destroy(LeftNameLabel);
                UnityEngine.Object.Destroy(RightNameLabel);

                UnityEngine.Object.Destroy(MiddleInfoLabel);
                UnityEngine.Object.Destroy(LeftInfoLabel);
                UnityEngine.Object.Destroy(RightInfoLabel);

                UnityEngine.Object.Destroy(LeftButton);
                UnityEngine.Object.Destroy(RightButton);
            }
            catch { }
        }
    }
}
