using Multiplayer.Managers;

namespace Multiplayer.UI
{
    internal sealed class MoeStatsWindow : BaseMultiplayerWindow
    {
        internal MoeStatsWindow() : base(UIManager.ProfileWindow)
        {
            AddReturnButton();
        }
    }
}
