using MelonLoader;
using Multiplayer.Managers;
using CustomAlbums.Utilities;
using Il2CppAssets.Scripts.Database;

namespace Multiplayer
{
    public class Main : MelonMod
    {
        public const string Name = "Multiplayer";
        public const string Version = "1.0.0";
        public const string Testers = "UntrustedURL, ???, ???";

        internal static Dispatcher Dispatcher { get; private set; }
        internal static Logger Logger { get; private set; }

        public override void OnEarlyInitializeMelon()
        {
            base.OnEarlyInitializeMelon();
            Dispatcher = new();
        }

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            Logger = new(Name);

            InitGlobal();

            Logger.Msg(Name + " was successfully initialized.");
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
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            if (BattleManager.CancellationTokenSource != null && sceneName != "GameMain")
            {
                BattleManager.BattleSyncStop();
            }

            if (sceneName == "UISystem_PC")
            {
                UIManager.Init();
                UIManager.InitUISystemMain();

                PlayerManager.LocalPlayerLVL = DataHelper.Level;
                AchievementManager.Check();
                PlayerManager.SyncLocalPlayer();
            } else if (sceneName == "GameMain")
            {

            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            Dispatcher.Update();
        }

        public override void OnDeinitializeMelon()
        {
            base.OnDeinitializeMelon();

            Settings.Save();
            Client.Disconnect();
            DiscordManager.Dispose();
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }
    }
}
