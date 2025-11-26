using Il2CppAssets.Scripts.PeroTools.UI;
using LocalizeLib;
using Multiplayer.Static;
using Multiplayer.UI;
using Multiplayer.UI.Displays;
using Multiplayer.UI.LobbyWindows;
using Multiplayer.UI.ProfileWindows;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using UnityEngine;
using UnityEngine.UI;
using Il2CppAssets.Scripts.Database;
using Il2Cpp;
using Il2CppAssets.Scripts.UI.Panels;

namespace Multiplayer.Managers
{
    internal static class UIManager
    {
        internal static bool Debounce = false;
        internal static Text WindowTitle { get; private set; }
        internal static GameObject MainFrame => GameObject.Find("UI/Forward/Tips/PnlBulletinNew");
        internal static PnlPreparation PnlPreparation => GameObject.Find("UI/Standerd/PnlPreparation").GetComponent<PnlPreparation>();
        internal static PnlStage PnlStage => GameObject.Find("UI/Standerd/PnlStage").GetComponent<PnlStage>();

        internal static MessageWindow Warning;
        internal static PromptWindow WarningChoose;
        internal static Action<bool?> WarningChooseAction;

        internal static MainMenu MainMenu { get; private set; }
        internal static MainMenuOpenButton MainMenuOpenButton { get; private set; }

        internal static ProfileWindow ProfileWindow { get; private set; }
        internal static FriendsWindow FriendsWindow { get; private set; }
        internal static AchievementsWindow AchievementsWindow { get; private set; }

        internal static FriendRequestsWindow FriendRequestsWindow { get; private set; }

        internal static LobbiesWindow LobbiesWindow { get; private set; }
        internal static LobbyWindow LobbyWindow { get; private set; }
        internal static PublicLobbiesWindow PublicLobbiesWindow { get; private set; }

        internal static LobbyCreationWindow LobbyCreationWindow { get; private set; }
        internal static LobbyGoalWindow LobbyGoalWindow { get; private set; }
        internal static LobbyPlayTypeWindow LobbyPlayTypeWindow { get; private set; }
        internal static LobbyChartSelectionWindow LobbyChartSelectionWindow { get; private set; }

        internal static LobbyPlaylistWindow LobbyPlaylistWindow { get; private set; }

        internal static MainLobbyDisplay MainLobbyDisplay { get; private set; }
        internal static BattleLobbyDisplay BattleLobbyDisplay { get; private set; }

        internal static PnlAwait PnlAwait { get; private set; }


        /// <summary>
        /// Displays a warning popup with the given <see cref="LocalString"/>.
        /// </summary>
        /// <param name="warning">Warning message.</param>
        internal static void WarnNotification(LocalString warning)
        {
            Warning.Text = warning;
            Warning.Show();
        }

        /// <summary>
        /// Displays a warning popup with the given <see cref="LocalString"/> and asks to continue or not.
        /// WarningChooseAction (<see cref="Action"/>) will be invoked once the local player clicks on either of options.
        /// </summary>
        /// <param name="warning">Warning message.</param>
        internal static void WarnChooseNotification(LocalString warning)
        {
            WarningChoose.Text = warning;
            WarningChoose.Show();
        }

        private static void WarningChooseCompletion(BaseWindow window)
        {
            if (WarningChooseAction is null) return;

            WarningChooseAction.Invoke(WarningChoose.Result);
            WarningChooseAction = null;
        }

        /// <summary>
        /// Locks/unlocks PnlPreparation depending on the local lobby.
        /// </summary>
        internal static void UpdatePnlPreparation()
        {
            MusicInfo curMusicInfo = GlobalDataBase.dbMusicTag.CurMusicInfo();
            if (curMusicInfo == null) return;

            GameObject playObject = GameObject.Find("UI/Standerd/PnlPreparation/Start/BtnStart");
            GameObject imgObject = playObject.transform.Find("TxtStart/ImgBtnA").gameObject;
            Text playText = playObject.transform.Find("TxtStart").GetComponent<Text>();
            Button playButton = playObject.GetComponent<Button>();
            InputKeyBinding keyBinding = playObject.GetComponent<InputKeyBinding>();
            
            playButton.enabled = !LobbyManager.IsInLobby || LobbyManager.CanChangePlaylist;
            keyBinding.enabled = playButton.enabled;
            imgObject.SetActive(playButton.enabled);

            if (!LobbyManager.IsInLobby)
            {
                playText.text = "PLAY!";
            } 
            else if (LobbyManager.CanChangePlaylist)
            {
                playText.text = Localization.Get("PnlPreparation",
                    LobbyManager.LocalLobby.HasInPlaylist(ChartManager.GetEntry(curMusicInfo, GlobalDataBase.dbMusicTag.selectedDiffTglIndex))
                    ? "PlaylistRemove"
                    : "PlaylistAdd"
                ).ToString();
            } 
            else
            {
                playText.text = Localization.Get("PnlPreparation", "Waiting").ToString();
            }
        }

        /// <summary>
        /// Initializes every time a UISystem_PC scene gets loaded.
        /// </summary>
        internal static void InitUISystemMain()
        {
            var windowTitleGo = GameObject.Instantiate(
                GameObject.Find("UI/Forward/Tips/PnlAchievementsTips/TxtTittle"),
                MainFrame.transform
            );
            windowTitleGo.transform.localPosition = new(0f, 330f, 0f);
            windowTitleGo.SetActive(false);
            WindowTitle = windowTitleGo.GetComponent<Text>();

            MainMenuOpenButton.Create();
            ProfileWindow.CreateAvatarBox();

            if (LobbyManager.IsInLobby)
            {
                MainLobbyDisplay.Create(LobbyManager.LocalLobby);

                if (LobbyManager.LocalLobby.Locked && LobbyManager.LocalLobby.Host == PlayerManager.LocalPlayer)
                {
                    _ = LobbyManager.LockLobby(false);
                }
            }
        }

        /// <summary>
        /// Initializes every time a GameMain scene gets loaded.
        /// </summary>
        internal static void InitGameMain()
        {
            if (LobbyManager.IsInLobby)
            {
                BattleLobbyDisplay.Create(LobbyManager.LocalLobby, false);
                PnlAwait.Create();
            }
        }

        internal static void Init()
        {
            if (MainMenu != null) return;

            Warning = new(new(), Localization.Get("Warning", "Title"));
            Warning.AutoReset = true;

            WarningChoose = new(new(), Localization.Get("Warning", "Title"));
            WarningChoose.AutoReset = true;

            WarningChoose.OnCompletion += WarningChooseCompletion;

            MainMenu = new();
            MainMenuOpenButton = new();

            ProfileWindow = new();
            FriendsWindow = new();
            AchievementsWindow = new();

            FriendRequestsWindow = new();

            LobbiesWindow = new();
            PublicLobbiesWindow = new();
            LobbyWindow = new();

            LobbyCreationWindow = new();
            LobbyGoalWindow = new();
            LobbyPlayTypeWindow = new();
            LobbyChartSelectionWindow = new();

            LobbyPlaylistWindow = new();

            MainLobbyDisplay = new();
            BattleLobbyDisplay = new();

            PnlAwait = new();

            ProfileWindow.CreateButtons();
            LobbiesWindow.CreateButtons();
            LobbyCreationWindow.CreateButtons();

            MainMenu.CreateButtons();
        }
    }
}
