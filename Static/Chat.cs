using Multiplayer.Data;
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

            ChatMessage chatMessage = new()
            {
                Message = msg,
                AuthorName = PlayerManager.LocalPlayer.MultiplayerStats.Name,
                AuthorUid = PlayerManager.LocalPlayerUid
            };

            if (chatMessage.IsCommand)
            {
                chatMessage.Command.Run();
            } 
            else Client.ChatWebsocketSend(JsonConvert.SerializeObject(chatMessage));
        }
    }
}
