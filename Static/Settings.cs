using MelonLoader;
using Multiplayer.Data.Settings;
using Multiplayer.Managers;
using Multiplayer.UI.Extensions;

namespace Multiplayer.Static
{
    internal static class Settings
    {
        internal static readonly MelonPreferences_Category MelonCategory;
        internal static readonly HashSet<ISetting> Config;
        internal static readonly string ConfigPath = Path.Combine("UserData", Constants.ModName + ".cfg");

        internal static T Get<T>(string settingName)
        {
            var setting = Config.First(s => s.Name == settingName) as Setting<T>;
            return setting.Value;
        }

        /// <summary>
        /// Loads the <see cref="Settings"/> from the config file.
        /// </summary>
        internal static void Load()
        {
            MelonCategory.LoadFromFile();
            foreach (var setting in Config)
            {
                setting.Load();
            }
        }

        static Settings()
        {
            MelonCategory = MelonPreferences.CreateCategory(Constants.ModName);
            MelonCategory.SetFilePath(ConfigPath, false);

            // Default config
            Config = new()
            {
                // Global

                new Setting<bool>("UseMDMCName", SettingCategory.Global, false, null, _ => PlayerManager.SyncProfile()),

                new Setting<bool>("ShowNavigationButtons", SettingCategory.Global, true, null, UIManager.ToggleNavigationButtons),

                new Setting<bool>("EnableLogging", SettingCategory.Global, true),

                new Setting<bool>("JailbreakMode", SettingCategory.Global, false),

                // Chat

                new Setting<bool>("EnableChat", SettingCategory.Chat, true, null,
                (bool value) =>
                {
                    if (UIManager.ChatLobbyDisplay == null || UIManager.ChatLobbyDisplay.Frame == null) return;
                    UIManager.ChatLobbyDisplay.Frame.SetActive(value);
                }),

                new Setting<bool>("FilterChatMessages", SettingCategory.Chat, true),

                new Setting<string>("NameColor", SettingCategory.Chat, "ffffff", c => c.Remove(0,6), _ => PlayerManager.SyncProfile()),

                // Lobby

                new Setting<bool>("FavGirlMode", SettingCategory.Lobby, true, null, (_) => PnlHomeExtension.UpdateCurrentPage()),

                new Setting<bool>("DisplayLobbyStatus", SettingCategory.Lobby, true),

                new Setting<int>("LobbyUpdateIntervalMS", SettingCategory.Lobby, 2000, ms => Math.Clamp(ms, Constants.LobbyUpdateIntervalMinMS, Constants.LobbyUpdateIntervalMaxMS)),

                // Battle

                new Setting<bool>("ShowBattlePopups", SettingCategory.Battle, true),

                new Setting<bool>("DisplayAvatars", SettingCategory.Battle, true),

                new Setting<int>("BattleUpdateIntervalMS", SettingCategory.Battle, 200, ms => Math.Clamp(ms, Constants.BattleUpdateIntervalMinMS, Constants.BattleUpdateIntervalMaxMS))
            };
        }
    }
}