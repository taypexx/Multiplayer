using MelonLoader;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.Patches;
using System.Reflection;
using System.Drawing;

namespace Multiplayer
{
    public class Main : MelonMod
    {
        internal static Dispatcher Dispatcher { get; private set; }
        internal static MelonLogger.Instance Logger { get; private set; }
        internal static string CurrentScene { get; private set; }
        internal static bool IsUIScene => CurrentScene == "UISystem_PC";

        private static readonly string[] Dependencies = { "CustomAlbums", "PopupLib", "LocalizeLib" };
        private static readonly string[] AdditionalDependencies = { "FavGirl" };
        private static Dictionary<string, Assembly> AdditionalDependenciesInstalled = new();

        internal static Assembly GetDependency(string dependencyName)
        {
            if (!AdditionalDependenciesInstalled.TryGetValue(dependencyName, out var asm)) return null;
            return asm;
        }

        public override void OnEarlyInitializeMelon()
        {
            base.OnEarlyInitializeMelon();
            Dispatcher = new();
        }

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            foreach (var dependencyName in Dependencies)
            {
                if (!RegisteredMelons.Any(m => m.Info.Name == dependencyName))
                {
                    Logger.Error($"Failed to initialize {Constants.ModName}: {dependencyName} is missing!");
                    return;
                } 
            }

            foreach (var dependencyName in AdditionalDependencies)
            {
                if (!RegisteredMelons.Any(m => m.Info.Name == dependencyName)) continue;
                AdditionalDependenciesInstalled.Add(dependencyName, AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == dependencyName));
            }

            Logger = new(Constants.ModName, Color.Magenta);

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
            Chat.Init();
            BattleManager.Init();
            Client.Init();
        }

        /// <summary>
        /// Initializes after connecting to the server.
        /// </summary>
        internal static async Task InitConnect()
        {
            AchievementManager.Init();
            await PlayerManager.Init();
            await LobbyManager.Init();
            ChartManager.Init();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            CurrentScene = sceneName;

            if (sceneName == "UISystem_PC")
            {
                UIManager.UpdateVanillaPanels();
                UIManager.Init();
                UIManager.InitUISystemMain();

                AchievementManager.Check();
                PlayerManager.SyncProfile();
                PlayerManager.SyncHiddens();
            } else if (sceneName == "GameMain")
            {
                UIManager.InitGameMain();

                BattlePatch.SceneLoaded();
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            Dispatcher.Update();
            InputManager.Update();
        }

        public override void OnDeinitializeMelon()
        {
            Settings.Save();
            base.OnDeinitializeMelon();
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }
    }
}
