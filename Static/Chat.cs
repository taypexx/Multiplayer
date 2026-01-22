using Multiplayer.Data.Websocket;
using Multiplayer.Managers;
using Multiplayer.UI;

namespace Multiplayer.Static
{
    internal static class Chat
    {
        internal static Dictionary<string, ChatCommand> TotalCommands;

        internal static void Recieve(ChatMessage chatMessage)
        {
            if (!LobbyManager.IsInLobby) return;

            Main.Dispatcher.Enqueue(() => UIManager.ChatLobbyDisplay.AddMessage(chatMessage));

            if (!chatMessage.IsSystemMessage)
            {
                var player = PlayerManager.GetCachedPlayer(chatMessage.AuthorUid);
                if (player is null) return;

                Main.Dispatcher.Enqueue(() => PnlHomeExtension.PlayerSpeak(player, chatMessage.Message));
            }
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

            if (message.Body.IsCommand)
            {
                message.Body.Command.Run();
            } 
            else _ = Client.WebsocketSend(message);
        }

        internal static void Init()
        {
            TotalCommands = new() 
            {
                ["help"] = new ChatCommand("help", new(() =>
                {
                    if (UIManager.ChatLobbyDisplay == null) return;

                    string helpText = string.Empty;
                    foreach ((string commandName, var chatCommand) in TotalCommands)
                    {
                        if (helpText != string.Empty) helpText += "\n";
                        helpText = helpText + $"/{commandName} — {chatCommand.Description}";
                    }

                    UIManager.ChatLobbyDisplay.AddMessage(new()
                    {
                        Message = helpText,
                        AuthorName = "system"
                    });
                })),

                ["clear"] = new ChatCommand("clear", new(() =>
                {
                    if (UIManager.ChatLobbyDisplay == null) return;

                    UIManager.ChatLobbyDisplay.ClearText(true);
                    UIManager.ChatLobbyDisplay.Update();
                })),

                ["website"] = new ChatCommand("website", new(() =>
                {
                    Utilities.OpenBrowserLink($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/home");
                })),

                ["mdmc"] = new ChatCommand("mdmc", new(() =>
                {
                    Utilities.OpenBrowserLink("https://mdmc.moe");
                })),

                ["discord"] = new ChatCommand("discord", new(() =>
                {
                    Utilities.OpenBrowserLink($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/discord");
                }))
            };
        }
    }
}
