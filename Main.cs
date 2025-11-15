using MelonLoader;
using Multiplayer.Managers;
using CustomAlbums.Utilities;
using Multiplayer.Data;

namespace Multiplayer
{
    public class Main : MelonMod
    {
        public const string Name = "Multiplayer";
        public const string Version = "1.0.0";

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

        private static void InitGlobal()
        {
            Settings.Load();
            AssetManager.Init();
            Localization.Init();
            BattleManager.Init();
            //DiscordManager.Init();
        }

        internal static void InitConnect()
        {
            AchievementManager.Init();
            PlayerManager.Init();
            LobbyManager.Init();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            if (BattleManager.Synchronizing && sceneName != "GameMain")
            {
                BattleManager.BattleSyncStop();
            }

            if (sceneName == "UISystem_PC")
            {
                DiscordManager.SetRPC(LobbyManager.LocalLobby is null ? RPCState.Idle : LobbyManager.LocalLobby.IsPrivate ? RPCState.InPrivateLobby : RPCState.InLobby);
                UIManager.Init();
                UIManager.InitUISystemMain();

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
