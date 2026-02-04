using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using Multiplayer.UI.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Displays
{
    internal sealed class MainLobbyDisplay : BaseLobbyDisplay
    {
        internal MainLobbyDisplay()
        {
            FrameParentPath = "UI/Standerd/PnlNavigation";

            TextAnchor = TextAnchor.UpperRight;
            TextHorizontalWrapMode = HorizontalWrapMode.Overflow;
            TextVerticalWrapMode = VerticalWrapMode.Overflow;
            MaxLines = null;
            FontSize = 26;
            EntryWidth = 400f;
            EntryDir = -1;

            FrameAnchorPosition = new(-25f, -90f);
            Pivot = new(1f, 1f);
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
