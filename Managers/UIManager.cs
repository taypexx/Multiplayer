using Multiplayer.UI;
using Multiplayer.UI.Displays;
using Multiplayer.UI.LobbyWindows;
using Multiplayer.UI.ProfileWindows;
using Multiplayer.Static;
using UnityEngine;
using UnityEngine.UI;
using Il2Cpp;
using Il2CppAssets.Scripts.UI;
using Il2CppAssets.Scripts.UI.Panels;
using Il2CppArcadeController.UI.Panel.PnlHome;
using PopupLib.UI;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using LocalizeLib;
using Multiplayer.Data.Lobbies;
using Multiplayer.Data.Players;
using Multiplayer.UI.NavigationButtons;
using UnityEngine.Events;
using Il2CppAssets.Scripts.UI.Panels.PnlRole;
using UnityEngine.EventSystems;
using Multiplayer.UI.Extensions;

namespace Multiplayer.Managers
{
    internal static class UIManager
    {
        internal static EventSystem EventSystem { get; private set; }

        internal static bool Initialized { get; private set; } = false;
        internal static bool Debounce = false;

        internal static GameObject MainFrame;
        internal static PnlMessage PnlMessage;
        internal static PageHome PageHome;
        internal static PnlMenu PnlMenu;
        internal static PnlNavigation PnlNavigation;
        internal static PnlPreparation PnlPreparation;
        internal static PnlStage PnlStage;
        internal static PnlHead PnlHead;
        internal static AchvSelect PnlAchvOther;
        internal static PnlRole PnlRole;
        internal static PnlElfin PnlElfin;
        internal static PnlRank PnlRank;
        internal static GameObject PnlCloudMessage;

        internal static PnlVictory PnlVictory;

        internal static PromptWindow PlayConfirmPrompt;
        internal static bool LobbyWindowQueued = false;

        internal static MessageWindow Warning;
        internal static PromptWindow WarningChoose;
        internal static Action<bool> WarningChooseAction;

        internal static MainMenu MainMenu { get; private set; }
        internal static SettingsWindow SettingsWindow { get; private set; }

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
        internal static ChatLobbyDisplay ChatLobbyDisplay { get; private set; }

        internal static MainButton MainNavButton { get; private set; }
        internal static LobbyButton LobbyNavButton { get; private set; }
        internal static PlayButton PlayNavButton { get; private set; }
        internal static PlaylistButton PlaylistNavButton { get; private set; }


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

        /// <summary>
        /// Invokes the current WarningChooseAction.
        /// </summary>
        private static void WarningChooseCompletion(BaseWindow _)
        {
            if (WarningChooseAction is null) return;

            WarningChooseAction.Invoke(WarningChoose.Result ?? false);
            WarningChooseAction = null;
        }

        /// <summary>
        /// Opens the <see cref="ProfileWindow"/> and displays information of the <see cref="Player"/> of the given <paramref name="uid"/>.
        /// </summary>
        /// <param name="uid">UID of a <see cref="Player"/>.</param>
        internal static async Task OpenProfileWindow(string uid)
        {
            Debounce = true;

            Player player = await PlayerManager.GetPlayer(uid);
            await ProfileWindow.Update(player);

            Main.Dispatch(() =>
            {
                Debounce = false;
                ProfileWindow.Window.Show();
            });
        }

        /// <summary>
        /// Opens the <see cref="ProfileWindow"/> and displays the information of the <see cref="Player"/>.
        /// </summary>
        /// <param name="player">(Optional) <see cref="Player"/> whose profile will be shown.</param>
        /// <param name="updatePlayer">(Optional) Whether to update the <see cref="Player"/> before opening.</param>
        internal static async Task OpenProfileWindow(Player player = null, bool updatePlayer = true)
        {
            if (player is null) player = PlayerManager.LocalPlayer;
            Debounce = true;

            await ProfileWindow.Update(player, updatePlayer);

            Main.Dispatch(() =>
            {
                Debounce = false;
                ProfileWindow.Window.Show();
            });
        }

        /// <summary>
        /// Opens the <see cref="LobbyWindow"/> and displays information and members of the <see cref="Lobby"/>.
        /// </summary>
        /// <param name="lobby">(Optional) <see cref="Lobby"/> which will be displayed.</param>
        internal static async Task OpenLobbyWindow(Lobby lobby = null)
        {
            if (Intermission.Active) return;
            if (lobby is null) lobby = LobbyManager.LocalLobby;
            Debounce = true;

            await LobbyWindow.Update(lobby, lobby != LobbyManager.LocalLobby);

            Main.Dispatch(() =>
            {
                Debounce = false;
                LobbyWindow.Window.Show();
            });
        }

        /// <summary>
        /// Opens <see cref="Player"/>'s profile in the browser.
        /// </summary>
        /// <param name="uid">UID of the <see cref="Player"/>.</param>
        internal static void OpenProfilePage(string uid)
        {
            Utilities.OpenBrowserLink($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/player/{uid}");
        }

        /// <summary>
        /// Opens the <see cref="LobbyPlaylistWindow"/>.
        /// </summary>
        internal static void OnPlaylistButtonClick()
        {
            if (!LobbyManager.IsInLobby)
            {
                PopupUtils.ShowInfo(Localization.Get("Lobby", "NoLobby"));
                return;
            }

            LobbyPlaylistWindow.Update(LobbyManager.LocalLobby);
            LobbyPlaylistWindow.Window.Show();
        }

        /// <summary>
        /// Performs required checks, locks the <see cref="Lobby"/> and proceeds to the <see cref="Intermission"/>.
        /// </summary>
        private static async Task OnPlayConfirm()
        {
            if (!LobbyManager.IsInLobby)
            {
                PopupUtils.ShowInfo(Localization.Get("Lobby", "NoLobby"));
                return;
            }

            if (PlayConfirmPrompt.Result != true)
            {
                if (LobbyWindowQueued) LobbyWindow.Window.Show();
                LobbyWindowQueued = false;
                return;
            }
            else LobbyWindowQueued = false;

            // Host and playlist checks
            var localLobby = LobbyManager.LocalLobby;
            if (localLobby.Host != PlayerManager.LocalPlayer) return;
            if (localLobby.Playlist.Count == 0)
            {
                PopupUtils.ShowInfo(Localization.Get("Lobby", "PlaylistEmpty"));
                LobbyWindow.Window.Show();
                return;
            }

            // Sleepwalker check
            foreach (string playerUid in localLobby.Players)
            {
                var player = PlayerManager.GetCachedPlayer(playerUid);
                if (player is null) continue;

                if (player.MultiplayerStats.GirlIndex == Constants.SleepwalkerRoleIndex)
                {
                    PopupUtils.ShowInfo((LocalString)String.Format(Localization.Get("Lobby", "SleepwalkerUsed").ToString(), player.MultiplayerStats.Name));
                    return;
                }
            }

            Debounce = true;
            await LobbyManager.LockLobby(true);
            Debounce = false;

            _ = Intermission.Start();
        }

        /// <summary>
        /// Opens <see cref="PnlStage"/> by exiting any other panels and jumps to a chart.
        /// </summary>
        /// <param name="uid">UID of a chart.</param>
        internal static void JumpToChart(string uid)
        {
            if (PnlMenu.gameObject.active)
            {
                PnlMenu.backBtn.onClick.Invoke();
            }
            if (PageHome.gameObject.active)
            {
                PageHome.transform.Find("Bottom/Btn").GetComponent<Button>().onClick.Invoke();
            }
            if (PnlPreparation.gameObject.active)
            {
                PnlNavigation.transform.Find("Top/BtnNavigationBack").GetComponent<Button>().onClick.Invoke();
            }
            PnlStage.SelectAllTagAndJumpToAssginIndex(uid);
        }

        /// <summary>
        /// Enables/disables navigation buttons.
        /// </summary>
        /// <param name="state">Whether to enable or disable the buttons.</param>
        internal static void ToggleNavigationButtons(bool state)
        {
            if (!Initialized) return;
            MainNavButton.Toggle(state);
            LobbyNavButton.Toggle(state);
            PlayNavButton.Toggle(state);
            PlaylistNavButton.Toggle(state);
        }

        /// <summary>
        /// Finds the <see cref="GameObject"/> for each vanilla panel and sets the fields.
        /// </summary>
        internal static void UpdateVanillaPanels()
        {
            MainFrame = GameObject.Find("UI/Forward/Tips/PnlBulletinNew");
            PnlMessage = GameObject.Find("CommonManagers/MessagesManager/UI/PnlMessage").GetComponent<PnlMessage>();
            PageHome = GameObject.Find("UI/Standerd/PnlHome").GetComponent<PageHome>();
            PnlMenu = GameObject.Find("UI/Standerd/PnlMenu").GetComponent<PnlMenu>();
            PnlNavigation = GameObject.Find("UI/Standerd/PnlNavigation").GetComponent<PnlNavigation>();
            PnlPreparation = GameObject.Find("UI/Standerd/PnlPreparation").GetComponent<PnlPreparation>();
            PnlStage = GameObject.Find("UI/Standerd/PnlStage").GetComponent<PnlStage>();
            PnlAchvOther = GameObject.Find("UI/Standerd/PnlMenu/Panels/PnlAchvLocalization/PnlAchv_other").GetComponent<AchvSelect>();
            PnlHead = GameObject.Find("UI/Forward/Tips/PnlHead").GetComponent<PnlHead>();
            PnlRole = GameObject.Find("UI/Standerd/PnlMenu/Panels/PnlRole").GetComponent<PnlRole>();
            PnlElfin = GameObject.Find("UI/Standerd/PnlMenu/Panels/PnlElfin").GetComponent<PnlElfin>();
            PnlRank = GameObject.Find("UI/Standerd/PnlPreparation/Panels/PnlRankLocalization/Pc/PnlRank").GetComponent<PnlRank>();
            PnlCloudMessage = GameObject.Find("UI/Standerd/PnlCloudMessage");
        }

        /// <summary>
        /// Initializes every time a UISystem_PC scene gets loaded.
        /// </summary>
        internal static void InitUISystemMain()
        {
            EventSystem = GameObject.Find("UI/EventSystem").GetComponent<EventSystem>();

            // Navigation

            MainNavButton.Create();
            LobbyNavButton.Create();
            PlayNavButton.Create();
            PlaylistNavButton.Create();

            // Displays and extensions

            BulletinExtension.Create();
            SideNotification.Create();
            PnlMenuExtension.Create();
            PnlCloudExtension.Create();
            PnlMessageExtension.Create();

            if (LobbyManager.IsInLobby && LobbyManager.LocalLobby.Playlist.Count == 0)
            {
                MainLobbyDisplay.Create(LobbyManager.LocalLobby);
                ChatLobbyDisplay.Create(LobbyManager.LocalLobby, true);
                PnlHomeExtension.Create();
            }

            var updateLobbyDisplayAction = (UnityAction)new Action(MainLobbyDisplay.UpdateTexts);
            var navBackButton = GameObject.Find("UI/Standerd/PnlNavigation/Top/BtnNavigationBack").GetComponent<Button>();
            var homeStartButton = GameObject.Find("UI/Standerd/PnlHome/Bottom/Btn").GetComponent<Button>();

            navBackButton.onClick.AddListener(updateLobbyDisplayAction);
            homeStartButton.onClick.AddListener(updateLobbyDisplayAction);

            // Other

            ProfileWindow.CreateAvatarBox();
            PnlHead.onClose += (Action)ProfileWindow.OnPnlHeadClose;
            PnlPreparationExtension.BindCustomPnlPreparationClick(PnlPreparation);
        }

        /// <summary>
        /// Initializes every time a GameMain scene gets loaded.
        /// </summary>
        internal static void InitGameMain()
        {
            PnlVictory = GameObject.Find("UI_3D/PnlVictory").GetComponent<PnlVictory>();

            if (LobbyManager.IsInLobby)
            {
                BattleLobbyDisplay.Create(LobbyManager.LocalLobby, false);
            }
        }

        internal static void Init()
        {
            if (Initialized) return;

            Warning = new(new(), Localization.Get("Warning", "Title"));
            Warning.AutoReset = true;

            WarningChoose = new(new(), Localization.Get("Warning", "Title"));
            WarningChoose.AutoReset = true;

            WarningChoose.OnCompletion += WarningChooseCompletion;

            PlayConfirmPrompt = new(Localization.Get("Lobby", "PlayConfirm"));
            PlayConfirmPrompt.AutoReset = true;
            PlayConfirmPrompt.OnCompletion += (BaseWindow w) => _ = OnPlayConfirm();

            MainMenu = new();
            SettingsWindow = new();

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
            ChatLobbyDisplay = new();
            BattleLobbyDisplay = new();

            MainNavButton = new();
            LobbyNavButton = new();
            PlayNavButton = new();
            PlaylistNavButton = new();

            SettingsWindow.CreateButtons();
            ProfileWindow.CreateButtons();
            LobbiesWindow.CreateButtons();
            LobbyCreationWindow.CreateButtons();

            MainMenu.CreateButtons();

            Initialized = true;
        }
    }
}
