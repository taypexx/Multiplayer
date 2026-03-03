using Multiplayer.Data.Lobbies;
using Multiplayer.Data.Players;
using UnityEngine;

namespace Multiplayer.Static
{
    public static class Constants
    {
        public const string ModName = "Multiplayer";
        public const string ModDescription = "Multiplayer client mod";
        public const string Authors = "taypexx & 7OU";
        public const string Version = "0.1.2";
        public static readonly Version Version_ = new Version(Version);

        public const string Credits = "[ DEVELOPMENT ]\n\n" +
            "<color=f542adff>taypexx</color> — Mod & backend development\n" +
            "<color=f542adff>7OU</color> — Backend development\n" +
            "<color=1eff00ff>AshtonMemer</color> — Custom Albums & Headquarters\n" +
            "<color=1eff00ff>PBalint817</color> — Additional libraries\n" +
            "<color=fff700ff>Bubu</color> — Traditional Chinese translation\n" +
            "<color=fff700ff>???</color> — Simplified Chinese translation\n" +
            "<color=fff700ff>???</color> — Korean translation\n" +
            "<color=fff700ff>???</color> — Japanese translation\n\n" +
            "[ TESTER TEAM ]\n\n" +
            "Slawter, Medeyah, PBalint817, WallKitty,\n" +
            "Nickicchi, Bubu, Yarshmellow, Fran艾林, ame,\n" +
            "Rohan, shezza, wylh, kariya_07, Fang, MADGUY,\n" +
            "المطربة الشعبية, free_muffin, Renbo, AutoGamer\n" +
            "IgnisclowVT";

        internal const int PortHTTP = 9095;
        internal const int PortWebsocket = 443;

        internal const int WebsocketTryReconnectTimes = 5;
        internal const int WebsocketReconnectAfterMS = 2000;

        internal const string MDMCAPIEndpoint = "https://api.mdmc.moe/v3/";
        internal static readonly string DiscordAuthURL = $"https://discord.com/oauth2/authorize?client_id=1436371970206728301&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A{PortHTTP}&scope=identify";
        internal const string ServerHTTPScheme = "https";
        internal const string ServerAddress = "mdmp.online";

        public const KeyCode PingKeyCode = KeyCode.LeftShift;
        public const KeyCode ChatFocusKeyCode = KeyCode.Slash;
        public const KeyCode ChatSendKeyCode = KeyCode.Return;

        public static readonly int TokenCipherShift = 17;

        public static readonly HashSet<string> UnsupportedChartUids = new() 
        {
            "93-0", // sushi class
            "84-0", // buro warrior
            "41-0" // super battleworn insomniac
        };

        public const int ModUnlockLevel = 100;
        public const int SleepwalkerRoleIndex = 2;
        public const int IntermissionTimeMS = 30000;

        public const int ChatMessageCharactersMax = 256;
        public const int ChatMessageHistorySize = 48;
        public const int BioCharactersMax = 48;
        public const int PlayersMin = 2;
        public const int PlayersMax = 10;
        public const int NameCharactersMin = 3;
        public const int NameCharactersMax = 16;
        public const int PasswordCharactersMin = 4;
        public const int PasswordCharactersMax = 16;
        public const int PlaylistSizeMin = 2;
        public const int PlaylistSizeMax = 32;

        public const int LobbyUpdateIntervalMinMS = 1000;
        public const int LobbyUpdateIntervalMaxMS = 8000;

        public const int BattleUpdateIntervalMinMS = 200;
        public const int BattleUpdateIntervalMaxMS = 2000;
        public const int BattleUpdateTimeoutMS = 2000;
        public const int AwaitBattleIntervalMS = 500;

        public const int CacheCheckIntervalMS = 5000;
        public static readonly TimeSpan PlayerCacheExpiration = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan LobbyCacheExpiration = TimeSpan.FromMinutes(10);

        public const string Red = "f5428aff";
        public const string Orange = "ff6f00ff";
        public const string Yellow = "fff700ff";
        public const string Green = "1eff00ff";
        public const string Blue = "4564ffff";
        public const string Pink = "fb24ffff";

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

        public static SortedDictionary<ushort, string> PingColors = new()
        {
            [100] = Green,
            [250] = Yellow,
            [500] = Orange,
            [ushort.MaxValue] = Red
        };
    }
}
