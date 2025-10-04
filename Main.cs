using MelonLoader;
using Multiplayer.Managers;
using CustomAlbums.Utilities;
using Il2CppArcadeController.UI.Panel.PnlHome;

namespace Multiplayer
{
    public class Main : MelonMod
    {
        internal static readonly Logger Logger = new("Multiplayer");

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            Settings.Load();
            AssetManager.Init();
            Localization.Init();

            Logger.Msg("Multiplayer was successfully initialized.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);

            if (sceneName == "UISystem_PC")
            {
                UIManager.Init();
                UIManager.InitUISystemMain();
            } else if (sceneName == "GameMain")
            {

            }
        }

        public override void OnDeinitializeMelon()
        {
            base.OnDeinitializeMelon();

            Settings.Save();
            Client.Disconnect();
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            AssetManager.CleanupDirectory();
        }
    }
}
