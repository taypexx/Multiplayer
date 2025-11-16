using UnityEngine;

namespace Multiplayer.UI
{
    internal sealed class MainLobbyDisplay : BaseLobbyDisplay
    {
        internal MainLobbyDisplay()
        {
            TextReferencePath = "UI/Standerd/PnlStage/StageUi/Info/ImgAlbumTittle/DisplayArea/TxtAlbumTittle";
            ParentPath = "UI/Standerd/PnlNavigation";
            AnchorPosition = new(-170f, -90f, 0f);
            Step = new(0f, -40f, 0f);
            TextAnchor = TextAnchor.UpperRight;
        }
    }
}
