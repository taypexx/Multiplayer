using MelonLoader.Utils;
using Multiplayer.Data.Lobbies;
using Multiplayer.Data.Players;

namespace Multiplayer.Static
{
    public static class Constants
    {
        public const string ModName = "Multiplayer";
        public const string Authors = "taypexx & 7OU";
        public const string Version = "0.1.0";
        public const string Testers = "ame, MADGUY, IgnisclowVT, PBalint817";

        internal const string DiscordAuthURL = "https://discord.com/oauth2/authorize?client_id=1436371970206728301&response_type=code&redirect_uri=http%3A%2F%2F127.0.0.1%3A9095%2Fauth&scope=identify";
        internal const string ServerIP = "127.0.0.1";
        internal const int PortHTTP = 9095;
        internal const int PortUDP = 9096;

        public static readonly string TempPath = Path.Combine(MelonEnvironment.UserDataDirectory, "Multiplayer");

        public const int BioCharactersMax = 32;

        public const int PlayersMin = 2;
        public const int PlayersMax = 8;
        public const int NameCharactersMin = 3;
        public const int NameCharactersMax = 16;
        public const int PasswordCharactersMin = 4;
        public const int PasswordCharactersMax = 16;
        public const int PlaylistSizeMin = 2;
        public const int PlaylistSizeMax = 32;

        public static readonly TimeSpan BattleSyncInterval = TimeSpan.FromMilliseconds(300);
        public static readonly TimeSpan AwaitBattleInterval = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan CacheCheckInterval = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan PlayerCacheExpiration = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan LobbyCacheExpiration = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan LobbyUpdateInterval = TimeSpan.FromSeconds(3);

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

        public static Dictionary<AchievementDifficulty, string> AchievmentDifficultyColors = new()
        {
            [AchievementDifficulty.Easy] = Green,
            [AchievementDifficulty.Medium] = Yellow,
            [AchievementDifficulty.Hard] = Red,
            [AchievementDifficulty.Secret] = Pink,
        };
    }
}
