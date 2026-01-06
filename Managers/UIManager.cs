using Multiplayer.UI;
using Multiplayer.UI.Displays;
using Multiplayer.UI.LobbyWindows;
using Multiplayer.UI.ProfileWindows;
using Multiplayer.Static;
using UnityEngine;
using UnityEngine.UI;
using Il2CppAssets.Scripts.Database;
using Il2Cpp;
using Il2CppAssets.Scripts.UI;
using Il2CppAssets.Scripts.UI.Panels;
using Il2CppAssets.Scripts.PeroTools.UI;
using Il2CppArcadeController.UI.Panel.PnlHome;
using PopupLib.UI;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using LocalizeLib;
using Multiplayer.Data.Lobbies;
using Multiplayer.Data.Players;

namespace Multiplayer.Managers
{
    internal static class UIManager
    {
        internal static bool Debounce = false;
        internal static Text WindowTitle { get; private set; }
        private static TimeSpan ShowMsgDuration = TimeSpan.FromSeconds(1);

        internal static GameObject MainFrame => GameObject.Find("UI/Forward/Tips/PnlBulletinNew");
        internal static PageHome PageHome => GameObject.Find("UI/Standerd/PnlHome").GetComponent<PageHome>();
        internal static PnlMenu PnlMenu => GameObject.Find("UI/Standerd/PnlMenu").GetComponent<PnlMenu>();
        internal static PnlNavigation PnlNavigation => GameObject.Find("UI/Standerd/PnlNavigation").GetComponent<PnlNavigation>();
        internal static PnlPreparation PnlPreparation => GameObject.Find("UI/Standerd/PnlPreparation").GetComponent<PnlPreparation>();
        internal static PnlStage PnlStage => GameObject.Find("UI/Standerd/PnlStage").GetComponent<PnlStage>();

        internal static MessageWindow Warning;
        internal static PromptWindow WarningChoose;
        internal static Action<bool> WarningChooseAction;

        internal static MainMenu MainMenu { get; private set; }
        internal static SettingsWindow SettingsWindow { get; private set; }
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

            WarningChooseAction.Invoke(WarningChoose.Result ?? false);
            WarningChooseAction = null;
        }

        /// <summary>
        /// Opens the <see cref="ProfileWindow"/> and displays information of the <see cref="Player"/> of the given <paramref name="uid"/>.
        /// </summary>
        /// <param name="uid">Uid of a <see cref="Player"/>.</param>
        internal static async Task OpenProfileWindow(string uid)
        {
            Debounce = true;

            Player player = await PlayerManager.GetPlayer(uid);
            await ProfileWindow.Update(player);

            Main.Dispatcher.Enqueue(() =>
            {
                Debounce = false;
                ProfileWindow.Window.Show();
            });
        }

        /// <summary>
        /// Opens the <see cref="ProfileWindow"/> and displays information of the <see cref="Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> whose profile will show.</param>
        internal static async Task OpenProfileWindow(Player player, bool updatePlayer = true)
        {
            Debounce = true;

            await ProfileWindow.Update(player, updatePlayer);

            Main.Dispatcher.Enqueue(() =>
            {
                Debounce = false;
                ProfileWindow.Window.Show();
            });
        }

        /// <summary>
        /// Opens the <see cref="LobbyWindow"/> and displays information and members of the <see cref="Lobby"/>.
        /// </summary>
        /// <param name="lobby"><see cref="Lobby"/> which will be displayed.</param>
        internal static async Task OpenLobbyWindow(Lobby lobby)
        {
            Debounce = true;

            await LobbyWindow.Update(lobby, true);

            Main.Dispatcher.Enqueue(() =>
            {
                Debounce = false;
                LobbyWindow.Window.Show();
            });
        }

        /// <summary>
        /// Locks/unlocks PnlPreparation depending on the local lobby.
        /// </summary>
        internal static void UpdatePnlPreparation()
        {
            try
            {
                if (PnlPreparation is null) return;

                MusicInfo curMusicInfo = GlobalDataBase.dbMusicTag.CurMusicInfo();
                if (curMusicInfo is null) return;

                GameObject playObject = GameObject.Find("UI/Standerd/PnlPreparation/Start/BtnStart");
                GameObject imgObject = playObject.transform.Find("TxtStart/ImgBtnA").gameObject;
                Text playText = playObject.transform.Find("TxtStart").GetComponent<Text>();
                Button playButton = playObject.GetComponent<Button>();
                InputKeyBinding keyBinding = playObject.GetComponent<InputKeyBinding>();

                bool isRemoving = LobbyManager.LocalLobby.HasInPlaylist(ChartManager.GetEntry(curMusicInfo, ChartManager.CurrentDifficulty));
                bool isFull = LobbyManager.LocalLobby.IsPlaylistFull;

                playButton.enabled = (!LobbyManager.IsInLobby || LobbyManager.CanChangePlaylist) && (isRemoving || !isFull);
                keyBinding.enabled = playButton.enabled;
                imgObject.SetActive(playButton.enabled);

                if (!LobbyManager.IsInLobby)
                {
                    playText.text = "PLAY!";
                }
                else if (LobbyManager.CanChangePlaylist)
                {
                    playText.text = Localization.Get("PnlPreparation",
                        isRemoving
                        ? "PlaylistRemove"
                        : isFull ? "PlaylistFull" : "PlaylistAdd"
                    ).ToString();
                }
                else
                {
                    playText.text = Localization.Get("PnlPreparation", "Waiting").ToString();
                }
            } catch { return; }
        }

        /// <summary>
        /// Opens PnlStage by exiting any other panels and jumps to a chart.
        /// </summary>
        /// <param name="uid">UID of a chart.</param>
        internal static void JumpToChart(string uid)
        {
            if (PageHome.gameObject.active)
            {
                PageHome.transform.Find("Bottom/Btn").GetComponent<Button>().onClick.Invoke();
            }
            else if (PnlPreparation.gameObject.active)
            {
                PnlNavigation.transform.Find("Top/BtnNavigationBack").GetComponent<Button>().onClick.Invoke();
            }
            else if (PnlMenu.gameObject.active)
            {
                PnlMenu.transform.Find("MenuNavigation/BtnBack").GetComponent<Button>().onClick.Invoke();
            }
            PnlStage.SelectAllTagAndJumpToAssginIndex(uid);
        }

        /// <summary>
        /// Closes all panels and opens PnlPreparation, then launches the game.
        /// </summary>
        internal static async Task ShowInfoAndStartGame()
        {
            if (Debounce) return;
            Debounce = true;
            Main.Dispatcher.Enqueue(() => PopupUtils.ShowInfo(Localization.Get("Lobby", "Starting")));

            await Task.Delay(ShowMsgDuration);

            Main.Dispatcher.Enqueue(() => 
            {
                PnlPreparation.OnBattleStart();
                Debounce = false;
            });
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

            if (LobbyManager.IsInLobby) MainLobbyDisplay.Create(LobbyManager.LocalLobby);
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
            SettingsWindow = new();
            MainMenuOpenButton = new();

            ProfileWindow = new();
            FriendsWindow = new();
            FriendRequestsWindow = new();
            AchievementsWindow = new();

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

            SettingsWindow.CreateButtons();
            ProfileWindow.CreateButtons();
            LobbiesWindow.CreateButtons();
            LobbyCreationWindow.CreateButtons();

            MainMenu.CreateButtons();
        }
    }
}
