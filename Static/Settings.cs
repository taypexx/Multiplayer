using Multiplayer.Managers;
using Multiplayer.UI;
using Tomlet;

namespace Multiplayer.Static
{
    public class Config
    {
        internal bool ShowNavigationButtons { 
            get;
            set {
                UIManager.ToggleNavigationButtons(value);
                field = value;
            } 
        } = true;
        internal bool EnableShortcuts { get; set; } = false;
        internal bool FavGirlMode { 
            get; 
            set {
                PnlHomeExtension.UpdateCurrentPage();
                field = value;
            } 
        } = false;
        internal bool ShowOtherElfins { get; set; } = true;
        internal bool DisplayLobbyStatus { get; set; } = true;
        internal bool ShowBattlePopups { get; set; } = true;
        internal int LobbyUpdateIntervalMS { 
            get; 
            set {
                field = Math.Clamp(value, Constants.LobbyUpdateIntervalMinMS, Constants.LobbyUpdateIntervalMaxMS);
            } 
        } = 3000;
        internal int BattleUpdateIntervalMS { 
            get; 
            set {
                field = Math.Clamp(value, Constants.BattleUpdateIntervalMinMS, Constants.BattleUpdateIntervalMaxMS);
            } 
        } = 300;
    }

    internal static class Settings
    {
        internal static string ConfigFilePath = Path.Combine("UserData", "Multiplayer.cfg");
        internal static Config Config = new();

        internal static void Load()
        {
            Main.Logger.Msg("Loading the settings...");
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    var defaultConfig = TomletMain.TomlStringFrom(Config);
                    File.WriteAllText(ConfigFilePath, defaultConfig);
                }

                Config = TomletMain.To<Config>(File.ReadAllText(ConfigFilePath));

                Main.Logger.Msg("Successfully loaded the settings!");
            }
            catch (Exception ex)
            {
                Main.Logger.Error("Failed to load settings: " + ex);
            }
        }

        internal static void Save()
        {
            Main.Logger.Msg("Saving the settings...");
            try
            {
                File.WriteAllText(ConfigFilePath, TomletMain.TomlStringFrom(Config));
                Main.Logger.Msg("Successfully saved the settings!");
            }
            catch (Exception ex)
            {
                Main.Logger.Error("Failed to save the settings: " + ex);
            }
        }
    }
}