using LocalizeLib;

namespace Multiplayer
{
    internal static class Localization
    {
        private static Dictionary<string, Dictionary<string, LocalString>> Strings;
        private static LocalString Empty = new();

        /// <summary>
        /// Returns a <see cref="LocalString"/> of the given <paramref name="category"/> and <paramref name="name"/> or an empty <see cref="LocalString"/> if not found.
        /// </summary>
        /// <returns>A <see cref="LocalString"/> reference.</returns>
        internal static LocalString Get(string category,string name)
        {
            if (!Strings.TryGetValue(category, out var dic)) { return Empty; }
            if (!dic.TryGetValue(name, out var localstr)) { return Empty; }
            return localstr;
        }

        internal static void Init()
        {
            Strings = new()
            {
                ["Warning"] = new() 
                {
                    ["Title"] = new()
                    {
                        English = "Warning"
                    },
                    ["NoAccount"] = new()
                    {
                        English = "You are not logged in! Please log in to play multiplayer."
                    },
                    ["Offline"] = new()
                    {
                        English = "Couldn't connect to the multiplayer server. Please make sure you are connected to the internet and restart the game."
                    }
                },
                ["Window"] = new()
                {
                    ["ReturnButton"] = new()
                    {
                        English = "Back"
                    },
                    ["ExitButton"] = new()
                    {
                        English = "Exit"
                    }
                },
                ["MainMenu"] = new()
                {
                    ["Open"] = new()
                    {
                        English = "Multiplayer"
                    },
                    ["Connecting"] = new()
                    {
                        English = "Connecting to the server..."
                    },
                    ["MyProfile"] = new()
                    {
                        English = "My Profile"
                    },
                    ["LocalPlayerBanned"] = new()
                    {
                        English = "You have been banned from multiplayer servers. If you wish to appeal contact @cvle. or @taypexx in discord."
                    }
                },
                ["ProfileWindow"] = new()
                {
                    ["Rank"] = new()
                    {
                        English = "Rank"
                    },
                    ["Avatar"] = new()
                    {
                        English = "Avatar"
                    },
                    ["Friends"] = new()
                    {
                        English = "Friends"
                    },
                    ["Achievements"] = new()
                    {
                        English = "Achievements"
                    },
                    ["HQStats"] = new()
                    {
                        English = "Headquarters Stats"
                    },
                    ["MoeStats"] = new()
                    {
                        English = "Vanilla Stats"
                    },
                },
                ["Achievements"] = new()
                {
                    ["AchievedOn"] = new()
                    {
                        English = "Achieved on"
                    },
                    ["Welcome!"] = new()
                    {
                        English = "Launch the multiplayer for the first time."
                    }
                }
            };
        }
    }
}
