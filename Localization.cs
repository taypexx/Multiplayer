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
                    ["Title"] = new()
                    {
                        English = "Multiplayer"
                    },
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
                    ["Title"] = new()
                    {
                        English = "Profile"
                    },
                    ["TitleMyProfile"] = new()
                    {
                        English = "My Profile"
                    },
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
                        English = "MuseDash.moe"
                    },
                    ["AddFriend"] = new()
                    {
                        English = "Send a friend request"
                    },
                    ["RemoveFriend"] = new()
                    {
                        English = "Unfriend"
                    },
                    ["DecideFriendRequest"] = new()
                    {
                        English = "Accept/reject the friend request"
                    },
                    ["CancelFriendRequest"] = new()
                    {
                        English = "Cancel friend request"
                    },
                    ["DecideFriendRequestPrompt"] = new()
                    {
                        English = "Accept the friend request?"
                    },
                    ["DecideUnfriendPrompt"] = new()
                    {
                        English = "Are you sure you want to unfriend this person?"
                    },
                    ["AddFriendSuccess"] = new()
                    {
                        English = "Request sent"
                    },
                    ["RemoveFriendSuccess"] = new()
                    {
                        English = "Unfriended!"
                    },
                    ["CancelFriendRequestSuccess"] = new()
                    {
                        English = "Cancelled friend request"
                    },
                    ["AddedFriend"] = new()
                    {
                        English = "New friend added!"
                    },
                },
                ["Achievements"] = new()
                {
                    ["Title"] = new()
                    {
                        English = "Achievements"
                    },
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
