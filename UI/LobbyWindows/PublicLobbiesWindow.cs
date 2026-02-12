using LocalizeLib;
using Multiplayer.Data.Lobbies;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class PublicLobbiesWindow : BaseMultiplayerWindow
    {
        private Dictionary<ForumObject, Lobby> ButtonsLobbies;

        private static LocalString MainDescription => Localization.Get("PublicLobbies", "Description");
        private static LocalString EmptyDescription => Localization.Get("PublicLobbies", "EmptyDescription");

        internal PublicLobbiesWindow() : base(Localization.Get("PublicLobbies", "Title"), UIManager.LobbiesWindow, "Lobbies.png")
        {
            ButtonsLobbies = new();
        }

        private string GetLobbyString(Lobby lobby)
        {
            return $"{lobby.Name} ({lobby.Players.Count}/{lobby.MaxPlayers})";
        }

        /// <summary>
        /// Finds a button related to the <see cref="Lobby"/> and updates it.
        /// </summary>
        /// <param name="lobby"></param>
        internal void UpdateLobbyButton(Lobby lobby)
        {
            if (!ButtonsLobbies.ContainsValue(lobby)) return;

            foreach ((ForumObject button, Lobby lobby_) in ButtonsLobbies)
            {
                if (lobby == lobby_)
                {
                    button.Titles = new(GetLobbyString(lobby));
                    return;
                }
            }
        }

        /// <summary>
        /// Updates current public lobbies.
        /// </summary>
        internal async Task Update()
        {
            await LobbyManager.UpdatePublicLobbies();

            Main.Dispatch(() =>
            {
                ButtonsLobbies.Clear();
                RemoveAllButtons();

                if (LobbyManager.PublicLobbies.Count() > 0)
                {
                    foreach (Lobby lobby in LobbyManager.PublicLobbies)
                    {
                        ForumObject button = AddButton(new(GetLobbyString(lobby)), null, MainDescription);
                        ButtonsLobbies.Add(button, lobby);
                    }
                }
                else AddEmptyButton(EmptyDescription);
            });
        }

        /// <summary>
        /// Refreshes current public lobbies and displays the updated list.
        /// </summary>
        private async Task Refresh()
        {
            UIManager.Debounce = true;

            await Update();

            Main.Dispatch(() =>
            {
                UIManager.Debounce = false;
                Window.Show();
            });
        }

        internal override void OnRefresh()
        {
            Window.ForceClose();
            _ = Refresh();
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];

            if (ButtonsLobbies.TryGetValue(button, out Lobby lobby))
            {
                _ = UIManager.OpenLobbyWindow(lobby);
            }
        }
    }
}
