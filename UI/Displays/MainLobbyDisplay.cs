using Multiplayer.UI.Abstract;
using UnityEngine;

namespace Multiplayer.UI.Displays
{
    internal sealed class MainLobbyDisplay : BaseLobbyDisplay
    {
        internal MainLobbyDisplay()
        {
            ParentPath = "UI/Standerd/PnlNavigation";
            AnchorPosition = new(500f, 350f, 0f);
            Step = new(0f, -35f, 0f);
            TextAnchor = TextAnchor.UpperRight;
        }
    }
}
