using Multiplayer.Managers;
using Multiplayer.UI;
using Newtonsoft.Json;

namespace Multiplayer.Static
{
    internal class ChatMessage
    {
        internal string Message;
        internal string AuthorName;
        internal string AuthorUid;
        internal bool IsSystemMessage => AuthorName.ToLower() == "system";

        public override string ToString()
        {
            if (IsSystemMessage)
            {
                string[] values = Message.Split("#");
                Span<string> param = values;
                return $"<color=#{Constants.Yellow}>(Lobby)</color> {String.Format(Localization.Get("SystemChatMessages", values[0]).ToString(), param.Slice(1).ToArray())}";
            } 
            else return $"[{AuthorName}]: {Message}";
        }
    }

    internal static class Chat
    {
        internal static void Recieve(ChatMessage chatMessage)
        {
            if (!LobbyManager.IsInLobby) return;

            Main.Logger.Msg(chatMessage);
            Main.Dispatcher.Enqueue(() => UIManager.ChatLobbyDisplay.AddText(chatMessage));

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
            Client.ChatWebsocketSend(JsonConvert.SerializeObject(chatMessage));
        }
    }
}
