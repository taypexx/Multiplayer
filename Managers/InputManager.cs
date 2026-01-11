using Multiplayer.Static;
using Multiplayer.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Multiplayer.Managers
{
    internal static class InputManager
    {
        internal static bool PingMode { get; private set; } = false;

        internal static void Update()
        {
            if (!UIManager.Initialized && EventSystem.current.currentSelectedGameObject != null) return;

            bool pingModeToggled = Input.GetKey(Constants.BattleDisplayKeyCode);
            if (PingMode != pingModeToggled)
            {
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
                return;
            }

            if (Main.IsUIScene && Settings.Config.EnableShortcuts)
            {
                if (Input.GetKeyDown(Constants.ChatOpenKeyCode) && UIManager.ChatLobbyDisplay.Lobby != null)
                {
                    // Focus on the chat InputField
                    var inputField = UIManager.ChatLobbyDisplay.Title.GetComponent<InputField>();
                    inputField.text = string.Empty;
                    inputField.Select();
                    inputField.ActivateInputField();
                }
                else if (Input.GetKeyDown(Constants.MainMenuOpenKeyCode))
                {
                    UIManager.MainNavButton.ButtonAction.Invoke();
                }
                else if (Input.GetKeyDown(Constants.LobbyOpenKeyCode))
                {
                    UIManager.LobbyNavButton.ButtonAction.Invoke();
                }
                else if (Input.GetKeyDown(Constants.PlaylistOpenKeyCode))
                {
                    UIManager.PlaylistNavButton.ButtonAction.Invoke();
                }
            }

            if (Input.GetKeyDown(Constants.ChatSendKeyCode) && UIManager.ChatLobbyDisplay.Lobby != null)
            {
                // Send a chat message
                var inputField = UIManager.ChatLobbyDisplay.Title.GetComponent<InputField>();
                var msg = inputField.text;
                if (msg != string.Empty)
                {
                    inputField.text = string.Empty;
                    Chat.Send(msg);
                }
            }
        }
    }
}
