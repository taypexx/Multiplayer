using Multiplayer.Static;
using Multiplayer.UI;
using UnityEngine;

namespace Multiplayer.Managers
{
    internal static class InputManager
    {
        internal static bool PingMode { get; private set; } = false;

        internal static void Update()
        {
            bool pingModeToggled = Input.GetKey(Constants.BattleDisplayKeyCode);
            if (PingMode == pingModeToggled) return;

            PingMode = pingModeToggled;

            if (UIManager.MainLobbyDisplay != null)
            {
                UIManager.MainLobbyDisplay.UpdateTexts();
            }

            if (UIManager.BattleLobbyDisplay != null)
            {
                UIManager.BattleLobbyDisplay.UpdateTexts();
            }

            AdvancedPnlHome.UpdateCurrentPage(true);
        }
    }
}
