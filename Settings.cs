using Tomlet;
using Tomlet.Attributes;

namespace Multiplayer
{
    internal static class Settings
    {
        internal static Config Config = new();

        internal static void Load()
        {
            Main.Logger.Msg("Loading the settings...");
            try
            {
                if (!File.Exists(Path.Combine("UserData", "Multiplayer.cfg")))
                {
                    var defaultConfig = TomletMain.TomlStringFrom(Config);
                    File.WriteAllText(Path.Combine("UserData", "Multiplayer.cfg"), defaultConfig);
                }

                var data = File.ReadAllText(Path.Combine("UserData", "Multiplayer.cfg"));
                Config = TomletMain.To<Config>(data);
                Main.Logger.Success("Successfully loaded the settings!");
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
                File.WriteAllText(Path.Combine("UserData", "Multiplayer.cfg"), TomletMain.TomlStringFrom(Config));
                Main.Logger.Success("Successfully saved the settings!");
            }
            catch (Exception ex)
            {
                Main.Logger.Error("Failed to save the settings: " + ex);
            }
        }
    }

    public class Config
    {
        [TomlPrecedingComment("Server address")]
        internal string MultiplayerAPI { get; set; } = "http://localhost:9095/api/";

        [TomlPrecedingComment("Timeout in milliseconds")]
        internal int RequestTimeout { get; set; } = 3000;

        [TomlPrecedingComment("Allow the invites from friends")]
        internal bool AllowFriendInvites { get; set; } = true;

        [TomlPrecedingComment("Show friend invites in battle")]
        internal bool ShowBattleInvites { get; set; } = false;
    }
}