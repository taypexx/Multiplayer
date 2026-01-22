using Il2CppAssets.Scripts.PeroTools.Commons;
using Multiplayer.Static;
using Multiplayer.UI;
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

        internal static void FocusOnChatField()
        {
            if (UIManager.ChatLobbyDisplay == null) return;

            var inputField = UIManager.ChatLobbyDisplay.InputField;
            inputField.text = string.Empty;
            inputField.Select();
            inputField.ActivateInputField();
        }

        internal static void Update()
        {
            if (!UIManager.Initialized) return;

            if (UIManager.ChatLobbyDisplay != null && InputEnabled == UIManager.ChatLobbyDisplay.InputField?.isFocused)
            {
                InputEnabled = !InputEnabled;
            }

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

            if (Input.GetKeyDown(Constants.ChatSendKeyCode) && UIManager.ChatLobbyDisplay != null && UIManager.ChatLobbyDisplay.Lobby != null)
            {
                // Send a chat message
                var inputField = UIManager.ChatLobbyDisplay.InputField;
                var msg = inputField.text;
                if (msg != string.Empty)
                {
                    inputField.text = string.Empty;
                    inputField.DeactivateInputField();
                    Chat.Send(msg);
                }
            }

            if (InputEnabled && Main.IsUIScene && Settings.Config.EnableShortcuts)
            {
                if (Input.GetKeyDown(Constants.ChatOpenKeyCode))
                {
                    FocusOnChatField();
                }
                else if (UIManager.MainFrame != null && !UIManager.MainFrame.active)
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
}
