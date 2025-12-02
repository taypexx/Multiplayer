using Tomlet;
using Tomlet.Attributes;

namespace Multiplayer.Static
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
                File.WriteAllText(Path.Combine("UserData", "Multiplayer.cfg"), TomletMain.TomlStringFrom(Config));
                Main.Logger.Msg("Successfully saved the settings!");
            }
            catch (Exception ex)
            {
                Main.Logger.Error("Failed to save the settings: " + ex);
            }
        }
    }

    public class Config
    {
        [TomlPrecedingComment("Server IP")]
        internal string ServerIP { get; set; } = Constants.ServerIP;

        [TomlPrecedingComment("HTTP port of the server")]
        internal int PortHTTP { get; set; } = Constants.PortHTTP;

        [TomlPrecedingComment("UDP port of the server")]
        internal int PortUdp { get; set; } = Constants.PortUDP;

        [TomlPrecedingComment("Allow the invites from friends")]
        internal bool AllowFriendInvites { get; set; } = true;

        [TomlPrecedingComment("Show friend invites in battle")]
        internal bool ShowBattleInvites { get; set; } = false;
    }
}