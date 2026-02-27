using Il2CppAssets.Scripts.Database;
using MelonLoader;
using Multiplayer.Managers;
using Multiplayer.Patches;
using Multiplayer.Static;
using System.Drawing;
using System.Reflection;
using static Multiplayer.Static.Dispatcher;

namespace Multiplayer
{
    public class Main : MelonMod
    {
        private static Dispatcher Dispatcher;
        private static MelonLogger.Instance Logger;
        internal enum LogType : byte { Error, Warning, Info, Success }

        internal static string CurrentScene { get; private set; }
        internal static bool IsUIScene => CurrentScene == "UISystem_PC";

        internal static Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
        private static readonly string[] Dependencies = { "CustomAlbums", "PopupLib", "LocalizeLib" };
        private static readonly string[] AdditionalDependencies = { "FavGirl" };
        private static Dictionary<string, Assembly> AdditionalDependenciesInstalled = new();

        internal static Assembly GetDependency(string dependencyName)
        {
            if (!AdditionalDependenciesInstalled.TryGetValue(dependencyName, out var asm)) return null;
            return asm;
        }

        /// <summary>
        /// Logs the passed object.
        /// </summary>
        /// <param name="msg">Message to log.</param>
        /// <param name="logType">Type of the log.</param>
        internal static void Log(object msg, LogType logType = LogType.Info)
        {
            if (msg is Exception || logType == LogType.Error)
            {
                Logger.Error(msg);
            }
            else if (Settings.Config.EnableLogging)
            {
                switch (logType)
                {
                    case LogType.Info:
                        Logger.Msg(msg); break;
                    case LogType.Success:
                        Logger.Msg(System.ConsoleColor.Green, msg); break;
                    case LogType.Warning:
                        Logger.Warning(msg); break;
                }
            }
        }

        internal static void Dispatch(DispatcherCallbackDelegate del)
        {
            Dispatcher.ThreadQueue.Enqueue(del);
        }

        public override void OnEarlyInitializeMelon()
        {
            Dispatcher = new();
            Logger = new(Constants.ModName, Color.Magenta);
        }

        public override void OnInitializeMelon()
        {
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

            InitGlobal();

            Log(Constants.ModName + " was successfully initialized.", LogType.Success);
        }

        /// <summary>
        /// Initializes before everything else.
        /// </summary>
        private static void InitGlobal()
        {
            Settings.Load();
            AssetManager.Init();
            Localization.Init();
            Client.Init();
            Chat.Init();
        }

        /// <summary>
        /// Initializes after connecting to the server.
        /// </summary>
        internal static async Task InitConnect()
        {
            await PlayerManager.Init();
            await LobbyManager.Init();
            Dispatch(() => 
            {
                AchievementManager.Init();
                UIManager.MainMenu.Open();
            });
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            CurrentScene = sceneName;

            if (IsUIScene)
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
            Dispatcher.Update();
            InputManager.Update();
        }

        public override void OnDeinitializeMelon()
        {
            Settings.Save();
            Client.Disconnect();
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }
    }
}
