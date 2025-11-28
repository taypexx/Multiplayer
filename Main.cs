using MelonLoader;
using Multiplayer.Managers;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Static;
using Multiplayer.Patches;

namespace Multiplayer
{
    public class Main : MelonMod
    {
        internal static Dispatcher Dispatcher { get; private set; }
        internal static MelonLogger.Instance Logger { get; private set; }
        internal static string CurrentScene { get; private set; }

        public override void OnEarlyInitializeMelon()
        {
            base.OnEarlyInitializeMelon();
            Dispatcher = new();
        }

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            Logger = new(Constants.ModName);

            InitGlobal();

            Logger.Msg(Constants.ModName + " was successfully initialized.");
        }

        /// <summary>
        /// Initializes before everything else.
        /// </summary>
        private static void InitGlobal()
        {
            Settings.Load();
            AssetManager.Init();
            Localization.Init();
            BattleManager.Init();
            //DiscordManager.Init();
        }

        /// <summary>
        /// Initializes after connecting to the server.
        /// </summary>
        internal static void InitConnect()
        {
            AchievementManager.Init();
            PlayerManager.Init();
            LobbyManager.Init();
            ChartManager.Init();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            CurrentScene = sceneName;

            if (sceneName == "UISystem_PC")
            {
                UIManager.Init();
                UIManager.InitUISystemMain();

                PlayerManager.LocalPlayerLVL = DataHelper.Level;
                AchievementManager.Check();
                PlayerManager.SyncProfile();
                PlayerManager.SyncHiddens();
            } else if (sceneName == "GameMain")
            {
                UIManager.InitGameMain();

                BattlePatch.SceneLoaded();
            } else if (LobbyManager.IsInLobby)
            {
                UIManager.MainLobbyDisplay.Destroy();
                UIManager.BattleLobbyDisplay.Destroy();
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            Dispatcher.Update();
        }

        public override void OnDeinitializeMelon()
        {
            Settings.Save();
            DiscordManager.Dispose();
            LobbyManager.LeaveLobby().ContinueWith(t => Client.Disconnect());

            base.OnDeinitializeMelon();
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }
    }
}
