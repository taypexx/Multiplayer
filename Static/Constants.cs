using MelonLoader.Utils;
using Multiplayer.Data.Lobbies;
using Multiplayer.Data.Players;
using UnityEngine;

namespace Multiplayer.Static
{
    public static class Constants
    {
        public const string ModName = "Multiplayer";
        public const string Authors = "taypexx & 7OU";
        public const string Version = "0.1.0";
        public const string Testers = "ame, MADGUY, IgnisclowVT, PBalint817, WallKitty, Medeyah";

        internal const string MDMCAPIEndpoint = "https://api.mdmc.moe/v3/";
        internal const string ServerHTTPScheme = "https";
        internal const string ServerAddress = "mdmp.online";
        internal const int PortHTTP = 9095;
        internal const int PortUDP = 9096;
        internal static readonly string DiscordAuthURL = $"https://discord.com/oauth2/authorize?client_id=1436371970206728301&response_type=code&redirect_uri={ServerHTTPScheme}%3A%2F%2F{ServerAddress}%2Fauth&scope=identify";

        public const KeyCode BattleDisplayKeyCode = KeyCode.LeftShift;
        public const KeyCode MainMenuOpenKeyCode = KeyCode.M;
        public const KeyCode LobbyOpenKeyCode = KeyCode.L;
        public const KeyCode PlaylistOpenKeyCode = KeyCode.P;
        public const KeyCode ChatOpenKeyCode = KeyCode.Slash;
        public const KeyCode ChatSendKeyCode = KeyCode.Return;

        public static readonly string TempPath = Path.Combine(MelonEnvironment.UserDataDirectory, "Multiplayer");

        public const int ChatMessageCharactersMax = 256;
        public const int BioCharactersMax = 48;
        public const int PlayersMin = 2;
        public const int PlayersMax = 8;
        public const int NameCharactersMin = 3;
        public const int NameCharactersMax = 16;
        public const int PasswordCharactersMin = 4;
        public const int PasswordCharactersMax = 16;
        public const int PlaylistSizeMin = 2;
        public const int PlaylistSizeMax = 32;

        public const int PlayerSpeechBubbleDurationMS = 5000;

        public const int LobbyUpdateIntervalMinMS = 1000;
        public const int LobbyUpdateIntervalMaxMS = 8000;

        public const int BattleUpdateIntervalMinMS = 200;
        public const int BattleUpdateIntervalMaxMS = 2000;

        public static readonly TimeSpan AwaitBattleInterval = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan CacheCheckInterval = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan PlayerCacheExpiration = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan LobbyCacheExpiration = TimeSpan.FromMinutes(5);

        public const string Red = "f5428aff";
        public const string Orange = "ff6f00ff";
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

        public static Dictionary<ushort, string> PingColors = new()
        {
            [100] = Green,
            [250] = Yellow,
            [500] = Orange,
            [ushort.MaxValue] = Red
        };
    }
}
