using Il2CppAssets.Scripts.PeroTools.Commons;
using Multiplayer.Static;
using Multiplayer.UI.Extensions;
using UnityEngine;

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

        /// <summary>
        /// Performs an input check each times it gets called.
        /// </summary>
        internal static void Update()
        {
            if (!UIManager.Initialized) return;

            if (UIManager.ChatLobbyDisplay != null && InputEnabled == UIManager.ChatLobbyDisplay.InputField?.isFocused)
            {
                InputEnabled = !InputEnabled;
            }

            // Ping switch
            bool pingModeToggled = Input.GetKey(Constants.PingKeyCode);
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

                // Focusing on chat
                if (Input.GetKeyDown(Constants.ChatFocusKeyCode) && InputEnabled)
                {
                    UIManager.ChatLobbyDisplay.ResetMessageHistoryIndex();
                    UIManager.ChatLobbyDisplay.ScrollToBottom();

                    inputField.text = string.Empty;
                    inputField.Select();
                    inputField.ActivateInputField();
                }
                // Sending a chat message
                else if (Input.GetKeyDown(Constants.ChatSendKeyCode))
                {
                    var msg = inputField.text.TrimStart().TrimEnd().Replace("\r", string.Empty).Replace("\n", string.Empty);
                    if (msg != string.Empty)
                    {
                        inputField.text = string.Empty;
                        inputField.DeactivateInputField();
                        UIManager.ChatLobbyDisplay.ResetMessageHistoryIndex();
                        Chat.Send(msg);
                    }
                    return;
                }
                // Message history browsing
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
        }
    }
}
