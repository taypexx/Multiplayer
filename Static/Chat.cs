using Multiplayer.Data.Players;
using Multiplayer.Data.Chat;
using Multiplayer.Managers;
using Multiplayer.UI;

namespace Multiplayer.Static
{
    internal static class Chat
    {
        internal static List<string> MutedPlayerUids;
        internal static List<string> MessageHistory;
        internal static Dictionary<string, ChatCommand> TotalCommands;

        internal static void Recieve(ChatMessage chatMessage)
        {
            if (!LobbyManager.IsInLobby) return;

            chatMessage.Init();

            if (!chatMessage.IsSystemMessage)
            {
                var player = PlayerManager.GetCachedPlayer(chatMessage.AuthorUid);
                if (player is null) return;

                var isMuted = MutedPlayerUids.Contains(chatMessage.AuthorUid);
                PnlHomeExtension.PlayerSpeak(player, isMuted ? string.Empty.PadRight(chatMessage.Message.Length, '*') : chatMessage.Message);
                if (isMuted) return;
                SoundManager.PlayBack();
            } 
            UIManager.ChatLobbyDisplay.AddMessage(chatMessage);
        }

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

        private static void MuteToggle(string[] args, bool mute)
        {
            if (!LobbyManager.IsInLobby) return;

            var input = args.Length == 0 ? null : args[1];
            Player target = null;

            if (input != null)
            {
                foreach (var playerUid in LobbyManager.LocalLobby.Players)
                {
                    var player = PlayerManager.GetCachedPlayer(playerUid);
                    if (player is null) continue;

                    if (
                        (input.Length > 3 && player.MultiplayerStats.Name.Contains(input))
                        || (input.Length <= 3 && player.MultiplayerStats.Name.StartsWith(input))
                        || input == playerUid
                    ) {
                        target = player;
                        break;
                    }
                }
            }

            string msg;
            if (input != null)
            {
                if (target != null)
                {
                    var wasMuted = MutedPlayerUids.Contains(target.Uid);
                    if (mute && wasMuted)
                    {
                        msg = String.Format(Localization.Get("SystemChatMessages", "PlayerAlreadyMuted").ToString(), target.MultiplayerStats.Name);
                    }
                    else if (!mute && !wasMuted)
                    {
                        msg = String.Format(Localization.Get("SystemChatMessages", "PlayerNotMuted").ToString(), target.MultiplayerStats.Name);
                    }
                    else if (target == PlayerManager.LocalPlayer)
                    {
                        msg = Localization.Get("SystemChatMessages", "PlayerMuteSelf").ToString();
                    } 
                    else
                    {
                        if (mute) MutedPlayerUids.Add(target.Uid);
                        else MutedPlayerUids.Remove(target.Uid);
                        msg = String.Format(Localization.Get("SystemChatMessages", $"Player{(mute ? "M" : "Unm")}uted").ToString(), target.MultiplayerStats.Name);
                    }
                }
                else msg = String.Format(Localization.Get("SystemChatMessages", "PlayerNotFound").ToString(), input);
            }
            else msg = Localization.Get("SystemChatMessages", "NoArguments").ToString();

            UIManager.ChatLobbyDisplay.AddMessage(new()
            {
                Message = msg,
                AuthorName = "system"
            });
        }

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
