using Multiplayer.Managers;
using Multiplayer.UI.Extensions;
using Tomlet;

namespace Multiplayer.Static
{
    public class Config
    {
        internal bool ShowNavigationButtons { 
            get; set 
            {
                UIManager.ToggleNavigationButtons(value);
                field = value;
            } 
        } 
        = true;

        internal bool EnableChat {
            get; set
            {
                if (UIManager.ChatLobbyDisplay != null && UIManager.ChatLobbyDisplay.Frame != null) UIManager.ChatLobbyDisplay.Frame.SetActive(value);
                field = value;
            }
        } 
        = true;

        internal bool EnableShortcuts { 
            get; set 
            {
                if (UIManager.ChatLobbyDisplay != null) UIManager.ChatLobbyDisplay.UpdatePlaceholder(value);
                field = value;
            }
        } 
        = false;

        internal bool FavGirlMode { 
            get; set 
            {
                PnlHomeExtension.UpdateCurrentPage();
                field = value;
            } 
        } 
        = false;

        internal bool ShowOtherElfins { get; set; } = true;

        internal bool DisplayLobbyStatus { get; set; } = true;

        internal bool ShowBattlePopups { get; set; } = true;

        internal bool EnableLogging { get; set; } = true;

        internal int LobbyUpdateIntervalMS { 
            get; set 
            {
                field = Math.Clamp(value, Constants.LobbyUpdateIntervalMinMS, Constants.LobbyUpdateIntervalMaxMS);
            } 
        } 
        = 2000;

        internal int BattleUpdateIntervalMS { 
            get; set 
            {
                field = Math.Clamp(value, Constants.BattleUpdateIntervalMinMS, Constants.BattleUpdateIntervalMaxMS);
            } 
        } 
        = 200;
    }

    internal static class Settings
    {
        internal static string ConfigFilePath = Path.Combine("UserData", "Multiplayer.cfg");
        internal static Config Config = new();

        internal static void Load()
        {
            Main.Log("Loading the settings...");
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    var defaultConfig = TomletMain.TomlStringFrom(Config);
                    File.WriteAllText(ConfigFilePath, defaultConfig);
                }

                Config = TomletMain.To<Config>(File.ReadAllText(ConfigFilePath));

                Main.Log("Successfully loaded the settings!", Main.LogType.Success);
            }
            catch (Exception ex)
            {
                Main.Log("Failed to load settings: " + ex, Main.LogType.Error);
            }
        }

        internal static void Save()
        {
            Main.Log("Saving the settings...");
            try
            {
                File.WriteAllText(ConfigFilePath, TomletMain.TomlStringFrom(Config));
                Main.Log("Successfully saved the settings!", Main.LogType.Success);
            }
            catch (Exception ex)
            {
                Main.Log("Failed to save the settings: " + ex, Main.LogType.Error);
            }
        }
    }
}