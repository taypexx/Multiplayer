using Multiplayer.Managers;

namespace Multiplayer.UI
{
    internal sealed class AchievementsWindow : BaseMultiplayerWindow
    {
        internal AchievementsWindow() : base(UIManager.ProfileWindow)
        {
            AddReturnButton();
        }
    }
}
