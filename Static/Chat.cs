using Multiplayer.Data.Websocket;
using Multiplayer.Managers;
using Multiplayer.UI;
using Newtonsoft.Json;

namespace Multiplayer.Static
{
    internal static class Chat
    {
        internal static void Recieve(ChatMessage chatMessage)
        {
            if (!LobbyManager.IsInLobby) return;

            Main.Dispatcher.Enqueue(() => UIManager.ChatLobbyDisplay.AddMessage(chatMessage));

            if (!chatMessage.IsSystemMessage)
            {
                var player = PlayerManager.GetCachedPlayer(chatMessage.AuthorUid);
                if (player is null) return;

                Main.Dispatcher.Enqueue(() => AdvancedPnlHome.PlayerSpeak(player, chatMessage.Message));
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
    }
}
