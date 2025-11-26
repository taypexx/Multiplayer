using Multiplayer.Data.LobbyEnums;

namespace Multiplayer.Static
{
    public static class Constants
    {
        public const string ModName = "Multiplayer";
        public const string Authors = "taypexx & 7OU";
        public const string Version = "1.0.0";
        public const string Testers = "UntrustedURL, ame, Medeyah, kataclysmx, Crits, IgnisclowVT, MADGUY";

        public const string Red = "f5428aff";
        public const string Yellow = "fff700ff";
        public const string Green = "1eff00ff";
        public const string Blue = "4564ffff";
        public const string Pink = "bc42f5ff";

        public static Dictionary<LobbyGoal, string> GoalColors = new()
        {
            [LobbyGoal.Accuracy] = Yellow,
            [LobbyGoal.Score] = Pink,
            [LobbyGoal.Custom] = Blue,
        };
        public static Dictionary<LobbyPlayType, string> PlayTypeColors = new()
        {
            [LobbyPlayType.All] = Yellow,
            [LobbyPlayType.VanillaOnly] = Blue,
            [LobbyPlayType.CustomOnly] = Pink,
        };
        public static Dictionary<LobbyChartSelection, string> ChartSelectionColors = new()
        {
            [LobbyChartSelection.HostPlaylist] = Yellow,
            [LobbyChartSelection.Playlist] = Pink,
            [LobbyChartSelection.Random] = Blue,
        };
    }
}
