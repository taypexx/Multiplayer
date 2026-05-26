using Il2CppAssets.Scripts.Database;
using Multiplayer.Data.Stats;
using Multiplayer.Managers;
using Multiplayer.Static;
using UnityEngine;

namespace Multiplayer.Data.Chat
{
    public class ChatMessage
    {
        public string Message { 
            get; 
            set {
                if (Static.Settings.Get<bool>("FilterChatMessages"))
                {
                    field = Filtering.Filter(value);
                }
                else field = value;
            } 
        }
        public string AuthorName { get; set { field = value.Trim('\n','\r'); } }
        public string AuthorUid { get; set; }
        public string ExtraData { get; set; }
        internal bool IsSystemMessage => AuthorName != null && AuthorName.ToLower() == "system";
        internal bool IsCommand => Message != null && Message.StartsWith("/");
        internal ChatCommand? Command { get; private set; }
        internal string[]? Arguments { get; private set; }

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
            else
            {
                string nameColor = "ffffff";
                if (AuthorUid != null)
                {
                    var player = PlayerManager.GetCachedPlayer(AuthorUid);
                    if (player != null)
                    {
                        nameColor = player.MultiplayerStats.ChatColor;
                    }
                }
                return $"<b><color=#{nameColor}>[{AuthorName}]:</color></b> <color=#e8e8e8>{Message}</color>";
            }
        }

        internal void InitCommand()
        {
            if (!IsCommand) return;
            Arguments = Message.Split(" ");
            Command = IsCommand ? Static.Chat.TotalCommands.TryGetValue(Arguments[0].Substring(1), out var cmd) ? cmd : Static.Chat.TotalCommands["."] : null;
        }
    }
}
