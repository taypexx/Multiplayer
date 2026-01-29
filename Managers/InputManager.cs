using Il2CppAssets.Scripts.PeroTools.Commons;
using Multiplayer.Static;
using Multiplayer.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Managers
{
    internal static class InputManager
    {
        internal static bool PingMode { get; private set; } = false;
        internal static bool InputEnabled
        {
            get; 
            private set
            {
                value = value || !LobbyManager.IsInLobby;
                try
                {
                    Singleton<Il2CppAssets.Scripts.PeroTools.Managers.InputManager>.instance.isStopKeyAction = !value;
                }
                catch { }
                field = value;
            }
        } = true;

        internal static void Update()
        {
            if (!UIManager.Initialized) return;

            if (UIManager.ChatLobbyDisplay != null && InputEnabled == UIManager.ChatLobbyDisplay.InputField?.isFocused)
            {
                InputEnabled = !InputEnabled;
            }

            // Ping switch
            bool pingModeToggled = Input.GetKey(Constants.BattleDisplayKeyCode);
            if (InputEnabled && PingMode != pingModeToggled)
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

                PnlHomeExtension.UpdateCurrentPage(true);
                return;
            }

            // Chat controls
            if (Settings.Config.EnableChat && UIManager.ChatLobbyDisplay != null && UIManager.ChatLobbyDisplay.Lobby != null)
            {
                var inputField = UIManager.ChatLobbyDisplay.InputField;
                if (Input.GetKeyDown(Constants.ChatOpenKeyCode) && Settings.Config.EnableShortcuts && InputEnabled)
                {
                    UIManager.ChatLobbyDisplay.ResetMessageHistoryIndex();

                    inputField.text = string.Empty;
                    inputField.Select();
                    inputField.ActivateInputField();
                }
                else if (Input.GetKeyDown(Constants.ChatSendKeyCode))
                {
                    // Send a chat message
                    var msg = inputField.text.TrimStart().TrimEnd(['\r', '\n']);
                    if (msg != string.Empty)
                    {
                        inputField.text = string.Empty;
                        inputField.DeactivateInputField();
                        UIManager.ChatLobbyDisplay.ResetMessageHistoryIndex();
                        Chat.Send(msg);
                    }
                    return;
                }
                else if (UIManager.ChatLobbyDisplay.InputField.isFocused)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        UIManager.ChatLobbyDisplay.BrowseMessageHistory(true);
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        UIManager.ChatLobbyDisplay.BrowseMessageHistory(false);
                    }
                }
            }

            // General shortcuts
            if (InputEnabled && Settings.Config.EnableShortcuts && UIManager.MainFrame != null && !UIManager.MainFrame.active)
            {

                if (Input.GetKeyDown(Constants.MainMenuOpenKeyCode))
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
        }
    }
}
