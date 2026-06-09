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
        public string AuthorName { get; set { field = value?.Trim('\n','\r'); } }
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

                    if (Message == "PlayerMissingChart" && param.Length > 2)
                    {
                        param = new string[] { param[0], string.Join("#", param.Skip(1)) };
                    }

                    if (Message == "PlaylistAdd" || Message == "PlaylistRemove")
                    {
                        string chartKey = "";
                        int difficulty = 0;
                        string chooserName = "Unknown";
                        string chartName = "Unknown Chart";

                        // Server format: p.Name#ChartKey#Difficulty#OwnerName#NiceChartName
                        // Old server format: p.Name#ChartKey#Difficulty
                        if (param.Length >= 3)
                        {
                            chooserName = param[0];
                            chartKey = param[1];
                            int.TryParse(param[2], out difficulty);
                        }

                        MusicInfo musicInfo = ChartManager.GetMusicInfo(chartKey);
                        if (musicInfo != null)
                        {
                            chartName = ChartManager.GetNiceChartName(musicInfo, difficulty);
                            ExtraData = musicInfo.uid;
                        }
                        else
                        {
                            if (param.Length >= 5)
                            {
                                // New format includes the nice chart name
                                chartName = string.Join("#", param.Skip(4));
                            }
                            else
                            {
                                var loc = Localization.Get("Lobby", "UnknownCustomChart");
                                chartName = loc != null ? loc.ToString() : "Unknown Custom Chart";
                            }
                        }
                        
                        param = new string[] { chooserName, chartName };
                    }

                    var locMsg = Localization.Get("SystemChatMessages", Message);
                    return string.Format(locMsg != null ? locMsg.ToString() : Message, param);
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
