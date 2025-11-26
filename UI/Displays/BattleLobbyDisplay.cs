using Multiplayer.Data;
using Multiplayer.Data.Stats;
using Multiplayer.Data.LobbyEnums;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Displays
{
    internal sealed class BattleLobbyDisplay : BaseLobbyDisplay
    {
        internal BattleLobbyDisplay()
        {
            ParentPath = "Forward";
            AnchorPosition = new(625f, -425f, 0f);
            Step = new(0f, 35f, 0f);
            TextAnchor = TextAnchor.LowerRight;
            DoesSort = true;
        }

        internal override void UpdateTexts()
        {
            if (!LobbyManager.IsInLobby) return;

            foreach ((object key, Text text) in TextList)
            {
                if (key is not Player) continue;
                Player player = (Player)key;

                BattleStats battleStats = player.BattleStats;
                string battleInfo = null;
                
                switch (LobbyManager.LocalLobby.Goal)
                {
                    case LobbyGoal.Accuracy:

                        if (battleStats.TrueAP)
                        {
                            battleInfo = $"<color=#{Constants.Red}>TP</color>";
                        }
                        else if (battleStats.AP)
                        {
                            battleInfo = $"<color=#{Constants.Yellow}>AP</color>";
                            if (battleStats.Earlies > 0)
                            {
                                battleInfo += $" <color=#{Constants.Blue}>{battleStats.Earlies}</color>E";
                            }
                            if (battleStats.Lates > 0)
                            {
                                battleInfo += $" <color=#{Constants.Red}>{battleStats.Lates}</color>L";
                            }
                        }
                        else if (battleStats.FC)
                        {
                            battleInfo = $"<color=#{Constants.Blue}>FC</color> {battleStats.Greats}G";
                        }
                        else
                        {
                            battleInfo = $"{battleStats.Misses}M {battleStats.Greats}G";
                        }

                        break;
                    case LobbyGoal.Score:
                        battleInfo = $"<color=#{Constants.Blue}>{battleStats.Score}</color>";
                        break;
                    case LobbyGoal.Custom:
                        break;
                }

                text.text = player == Lobby.Host ? $"{battleInfo} — <color=#{Constants.Yellow}>{player.MultiplayerStats.Name}</color>" : player.MultiplayerStats.Name;
            }
        }
    }
}
