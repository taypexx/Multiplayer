using Multiplayer.UI.Abstract;
using UnityEngine;

namespace Multiplayer.UI.Displays
{
    internal sealed class MainLobbyDisplay : BaseLobbyDisplay
    {
        internal MainLobbyDisplay()
        {
            ParentPath = "UI/Standerd/PnlNavigation";
            AnchorPosition = new(700f, 350f, 0f);
            Step = new(0f, -35f, 0f);
            PopupOffset = new(-135f, 0, 0);
            PopupX = 50f;
            HorizontalWrapMode = HorizontalWrapMode.Wrap;
            TextAnchor = TextAnchor.UpperRight;
            FontSize = 26;
        }
    }
}
