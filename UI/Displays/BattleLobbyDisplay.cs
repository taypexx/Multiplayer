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
            FrameParentPath = "Forward";

            TextAnchor = TextAnchor.LowerLeft;
            TextHorizontalWrapMode = HorizontalWrapMode.Overflow;
            TextVerticalWrapMode = VerticalWrapMode.Overflow;
            MaxLines = null;
            FontSize = 26;
            EntryWidth = 600f;
            EntryDir = 1;

            FrameAnchorPosition = new(20f, 20f);
            Pivot = new(0f, 0f);
            DoesSort = true;
        }

        protected override void Popup(string text, object key)
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
                
                if (InputManager.PingMode)
                {
                    battleInfo = $"<color=#{Utilities.GetPingColor(player.PingMS)}>{player.PingMS}ms</color>";
                }
                else if (!Lobby.ReadyPlayers.Contains(player.Uid))
                {
                    battleInfo = Localization.Get("Global", "Loading").ToString();
                }
                else if (!Lobby.EveryoneReady)
                {
                    battleInfo = $"<color=#{Constants.Green}>Ready ✓</color>";
                }
                else battleInfo = LobbyManager.LocalLobby.GetBattleInfo(player);

                text.text = $"{PositionList.Count - PositionList.IndexOf(key)}) {(player == PlayerManager.LocalPlayer ? $"<color=#{Constants.Yellow}>{player.MultiplayerStats.Name}</color>" : player.MultiplayerStats.Name)} — {battleInfo}";

                if (battleStats.PrevFC && !battleStats.FC)
                    Popup($"<color=#{Constants.Red}>{Localization.Get("BattleDisplay", "LostFC").ToString()}</color>", key);
                else if (battleStats.PrevAP && !battleStats.AP)
                    Popup($"<color=#{Constants.Yellow}>{Localization.Get("BattleDisplay", "LostAP").ToString()}</color>", key);
                else if (battleStats.PrevAlive && !battleStats.Alive)
                    Popup($"<color=#{Constants.Red}>{Localization.Get("BattleDisplay", "Died").ToString()}</color>", key);
                else if (battleStats.Misses > battleStats.PrevMisses)
                    Popup(Localization.Get("BattleDisplay", "Missed").ToString(), key);

                battleStats.UpdatePrevious();
            }
        }
    }
}
