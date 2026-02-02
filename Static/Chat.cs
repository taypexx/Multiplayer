using Multiplayer.Data.Players;
using Multiplayer.Data.Chat;
using Multiplayer.Managers;
using Multiplayer.UI;

namespace Multiplayer.Static
{
    internal static class Chat
    {
        internal static HashSet<string> MutedPlayerUids;
        internal static List<string> MessageHistory;
        internal static Dictionary<string, ChatCommand> TotalCommands;

        /// <summary>
        /// Registers a new <see cref="ChatMessage"/> and adds it to the chat.
        /// </summary>
        internal static void Recieve(ChatMessage chatMessage)
        {
            if (!LobbyManager.IsInLobby || !Settings.Config.EnableChat) return;

            chatMessage.Init();

            if (!chatMessage.IsSystemMessage)
            {
                var player = PlayerManager.GetCachedPlayer(chatMessage.AuthorUid);
                if (player is null) return;

                var isMuted = MutedPlayerUids.Contains(chatMessage.AuthorUid);
                PnlHomeExtension.PlayerSpeak(player, isMuted ? string.Empty.PadRight(chatMessage.Message.Length, '*') : chatMessage.Message);

                if (isMuted) return;
                if (UIManager.PageHome.gameObject.active) SoundManager.PlayBack();
            } 
            UIManager.ChatLobbyDisplay.AddMessage(chatMessage);
        }

        /// <summary>
        /// Wraps the <paramref name="msg"/> into a <see cref="ChatMessage"/> and either runs a <see cref="ChatCommand"/> or sends to the server.
        /// Also saves the message to the history.
        /// </summary>
        /// <param name="msg">Any junk you could possibly type in the chat.</param>
        internal static void Send(string msg) 
        {
            if (!LobbyManager.IsInLobby) return;

            var message = new
            {
                Type = "Chat",
                Body = new ChatMessage
                {
                    Message = msg,
                    AuthorName = PlayerManager.LocalPlayer.MultiplayerStats.Name,
                    AuthorUid = PlayerManager.LocalPlayerUid
                }
            };

            message.Body.Init();
            if (message.Body.IsCommand)
            {
                message.Body.Command.Run(message.Body.Arguments);
            } 
            else _ = Client.WebsocketSend(message);

            if (MessageHistory.Count == Constants.ChatMessageHistorySize)
            {
                MessageHistory.RemoveAt(0);
            }
            MessageHistory.Add(msg);
        }

        /// <summary>
        /// Looks for a cached <see cref="Player"/> whose name matches the <paramref name="query"/> the most.
        /// </summary>
        private static Player FindPlayer(string query)
        {
            foreach (var playerUid in LobbyManager.LocalLobby.Players)
            {
                var player = PlayerManager.GetCachedPlayer(playerUid);
                if (player is null) continue;

                if (
                    (query.Length > 3 && player.MultiplayerStats.Name.Contains(query))
                    || (query.Length <= 3 && player.MultiplayerStats.Name.StartsWith(query))
                    || query == playerUid
                )
                {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// Mutes/unmutes a <see cref="Player"/> on the client.
        /// </summary>
        /// <param name="args"><see cref="ChatCommand"/> arguments.</param>
        /// <param name="mute">Whether to mute or unmute.</param>
        private static void MuteToggle(string[] args, bool doMute)
        {
            if (!LobbyManager.IsInLobby) return;

            var query = args.Length == 0 ? null : args[1];
            Player target = null;

            if (query != null) target = FindPlayer(query);

            string msg;
            if (query != null)
            {
                if (target != null)
                {
                    var wasMuted = MutedPlayerUids.Contains(target.Uid);
                    if (doMute && wasMuted)
                    {
                        msg = String.Format(Localization.Get("SystemChatMessages", "PlayerAlreadyMuted").ToString(), target.MultiplayerStats.Name);
                    }
                    else if (!doMute && !wasMuted)
                    {
                        msg = String.Format(Localization.Get("SystemChatMessages", "PlayerNotMuted").ToString(), target.MultiplayerStats.Name);
                    }
                    else if (target == PlayerManager.LocalPlayer)
                    {
                        msg = Localization.Get("SystemChatMessages", "PlayerMuteSelf").ToString();
                    } 
                    else
                    {
                        if (doMute) MutedPlayerUids.Add(target.Uid);
                        else MutedPlayerUids.Remove(target.Uid);
                        msg = String.Format(Localization.Get("SystemChatMessages", $"Player{(doMute ? "M" : "Unm")}uted").ToString(), target.MultiplayerStats.Name);
                    }
                }
                else msg = String.Format(Localization.Get("SystemChatMessages", "PlayerNotFound").ToString(), query);
            }
            else msg = Localization.Get("SystemChatMessages", "NoArguments").ToString();

            UIManager.ChatLobbyDisplay.AddMessage(new()
            {
                Message = $"<i>{msg}</i>",
                AuthorName = "system"
            });
        }

        /// <summary>
        /// Initializes every <see cref="ChatCommand"/>.
        /// </summary>
        internal static void Init()
        {
            MessageHistory = new();
            MutedPlayerUids = new();

            Action<string[]> helpAction = new((_) =>
            {
                string helpText = string.Empty;
                foreach ((string commandName, var chatCommand) in TotalCommands)
                {
                    if (chatCommand.Hidden) continue;
                    if (helpText != string.Empty) helpText += "\n";
                    helpText = helpText + $"/{commandName} — {chatCommand.Description}";
                }

                UIManager.ChatLobbyDisplay.AddMessage(new()
                {
                    Message = helpText,
                    AuthorName = "system"
                });
            });

            TotalCommands = new() 
            {
                ["."] = new(".", new((args) => {
                    UIManager.ChatLobbyDisplay.AddMessage(new()
                    {
                        Message = String.Format(Localization.Get("SystemChatMessages", "UnknownCommand").ToString(), args[0]),
                        AuthorName = "system"
                    });
                }), true),

                ["help"] = new("help",helpAction),
                ["?"] = new("help", helpAction, true),

                ["clear"] = new("clear", new((_) =>
                {
                    UIManager.ChatLobbyDisplay.ClearText(true);
                    UIManager.ChatLobbyDisplay.Update();
                })),

                ["mute"] = new("mute", new((args) => 
                {
                    MuteToggle(args, true);
                })),

                ["unmute"] = new("unmute", new((args) =>
                {
                    MuteToggle(args, false);
                })),

                ["discord"] = new("discord", new((_) =>
                {
                    Utilities.OpenBrowserLink($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/discord");
                })),
            };
        }
    }
}
