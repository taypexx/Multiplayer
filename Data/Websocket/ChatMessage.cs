using Il2CppAssets.Scripts.Database;
using Multiplayer.Managers;
using Multiplayer.Static;

namespace Multiplayer.Data.Websocket
{
    public class ChatMessage
    {
        public string Message { get; set; }
        public string AuthorName { get; set; }
        public string AuthorUid { get; set; }
        public string ExtraData { get; set; }
        internal bool IsSystemMessage => AuthorName.ToLower() == "system";
        internal bool IsCommand => Message != null && Message.StartsWith("/") && Chat.TotalCommands.ContainsKey(Message.Substring(1));
        internal ChatCommand Command => IsCommand ? Chat.TotalCommands[Message.Substring(1)] : null;

        public override string ToString()
        {
            if (IsSystemMessage)
            {
                if (ExtraData != null)
                {
                    string[] param = ExtraData.Split("#");

                    if (Message == "PlaylistAdd" || Message == "PlaylistRemove")
                    {
                        MusicInfo musicInfo = ChartManager.GetMusicInfo(param[1]);
                        if (musicInfo != null)
                        {
                            param[1] = ChartManager.GetNiceChartName(musicInfo, int.Parse(param[2]));
                            param[2] = null;
                            ExtraData = musicInfo.uid;
                        }
                    }

                    return string.Format(Localization.Get("SystemChatMessages", Message).ToString() ?? "Unknown system message", param);
                }
                else return Message;
            }
            else return $"<b><color=#{(LobbyManager.LocalLobby.Host.Uid == AuthorUid ? Constants.Yellow : "ffffff")}>[{AuthorName}]:</color></b> <color=#e8e8e8>{Message}</color>";
        }
    }
}
