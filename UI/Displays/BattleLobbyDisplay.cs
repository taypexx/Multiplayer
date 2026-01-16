using Multiplayer.Data.Stats;
using Multiplayer.Data.Lobbies;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;
using Multiplayer.Data.Players;

namespace Multiplayer.UI.Displays
{
    internal sealed class BattleLobbyDisplay : BaseLobbyDisplay
    {
        internal BattleLobbyDisplay()
        {
            ParentPath = "Forward";
            AnchorPosition = new(-825f, -425f, 0f);
            EntrySize = new(200f, 200f);
            Step = new(0f, 35f, 0f);
            PopupOffset = new(330f, 0, 0);
            PopupX = 50f;
            TextAnchor = TextAnchor.LowerLeft;
            TextHorizontalWrapMode = HorizontalWrapMode.Overflow;
            FontSize = 28;
            DoesSort = true;
        }

        internal override void Popup(string text, object key)
        {
            if (!Settings.Config.ShowBattlePopups) return;
            base.Popup(text, key);
        }

        internal override void UpdateTexts()
        {
            if (!LobbyManager.IsInLobby) return;

            foreach ((object key, Text text) in TextList)
            {
                if (key is not Player) continue;
                Player player = (Player)key;

                BattleStats battleStats = player.BattleStats;
                string battleInfo = string.Empty;
                
                if (!InputManager.PingMode)
                {
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
                                    battleInfo += $" <color=#{Constants.Blue}>{battleStats.Earlies}E</color>";
                                }
                                if (battleStats.Lates > 0)
                                {
                                    battleInfo += $" <color=#{Constants.Red}>{battleStats.Lates}L</color>";
                                }
                            }
                            else if (battleStats.FC)
                            {
                                battleInfo = $"<color=#{Constants.Blue}>FC</color> {battleStats.Accuracy}%  {battleStats.Greats}G";
                            }
                            else
                            {
                                battleInfo = $"{battleStats.Accuracy}%  {battleStats.Misses}M {battleStats.Greats}G";
                            }

                            break;
                        case LobbyGoal.Score:
                            battleInfo = $"<color=#{Constants.Blue}>{battleStats.Score}</color>";
                            break;
                        case LobbyGoal.Custom:
                            break;
                    }
                } else battleInfo = $"<color=#{Utilities.GetPingColor(player.PingMS)}>{player.PingMS}ms</color>";

                var name = Lobby.ReadyPlayers.Contains(player.Uid) ? player.MultiplayerStats.Name : Localization.Get("Global", "Loading").ToString();
                text.text = $"{PositionList.Count - PositionList.IndexOf(key)}) {(player == PlayerManager.LocalPlayer ? $"<color=#{Constants.Yellow}>{name}</color>" : name)} — {battleInfo}";

                if (battleStats.PrevFC && !battleStats.FC)
                    Popup($"<color=#{Constants.Red}>{Localization.Get("BattleDisplay", "LostFC").ToString()}</color>", key);
                else if (battleStats.PrevAP && !battleStats.AP)
                    Popup($"<color=#{Constants.Yellow}>{Localization.Get("BattleDisplay", "LostAP").ToString()}</color>", key);
                else if (battleStats.Misses > battleStats.PrevMisses)
                    Popup(Localization.Get("BattleDisplay", "Missed").ToString(), key);

                battleStats.UpdatePrevious();
            }
        }
    }
}
