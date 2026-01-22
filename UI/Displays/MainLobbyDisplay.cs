using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Displays
{
    internal sealed class MainLobbyDisplay : BaseLobbyDisplay
    {
        internal MainLobbyDisplay()
        {
            ParentPath = "UI/Standerd/PnlNavigation";
            AnchorPosition = new(830f, 350f, 0f);
            EntrySize = new(200f, 200f);
            Step = new(0f, -35f, 0f);
            PopupOffset = new(-135f, 0, 0);
            PopupX = 50f;
            TextAnchor = TextAnchor.UpperRight;
            TextHorizontalWrapMode = HorizontalWrapMode.Overflow;
            FontSize = 26;
        }

        internal override void UpdateTexts()
        {
            var playersVisible = !PnlHomeExtension.Visible;
            foreach ((object key, Text text) in TextList)
            {
                if (key is not Player) continue;
                Player player = (Player)key;

                text.enabled = playersVisible;
                if (playersVisible)
                {
                    text.text = player == Lobby.Host ? $"<color=#fff700ff>[Host]</color> {player.MultiplayerStats.Name}" : player.MultiplayerStats.Name;

                    if (InputManager.PingMode)
                    {
                        text.text += $" — <color=#{Utilities.GetPingColor(player.PingMS)}>{player.PingMS}ms</color>";
                    }
                }
            }
        }
    }
}
