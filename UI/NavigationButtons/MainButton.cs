using Multiplayer.Managers;
using Multiplayer.UI.Abstract;
using UnityEngine.Events;

namespace Multiplayer.UI.NavigationButtons
{
    internal sealed class MainButton : BaseNavigationButton
    {
        internal MainButton() : base("Globe.png", 1, true)
        {
            ButtonAction = (UnityAction)new Action(() => UIManager.MainMenu.Open());
            AlwaysVisible = true;
            Create();
        }
    }
}
