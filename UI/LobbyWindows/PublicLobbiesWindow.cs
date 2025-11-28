using LocalizeLib;
using Multiplayer.Data;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;
using System.Net.Http.Json;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class PublicLobbiesWindow : BaseMultiplayerWindow
    {
        private Dictionary<ForumObject, Lobby> ButtonsLobbies;

        private static LocalString MainDescription => Localization.Get("PublicLobbies", "Description");
        private static LocalString EmptyDescription => Localization.Get("PublicLobbies", "DescriptionEmpty");

        internal PublicLobbiesWindow() : base(Localization.Get("PublicLobbies", "Title"), UIManager.LobbiesWindow, "Lobbies.png")
        {
            ButtonsLobbies = new();
            AddReturnButton();
            AddRefreshButton();
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
            var payload = new
            {
                Client.Token,
                PlayerManager.LocalPlayer.Uid
            };

            var response = await Client.PostAsync("getLobbies", payload);
            if (response == null) return;

            var lobbyIds = await response.Content.ReadFromJsonAsync<List<int>>();
            foreach (int id in lobbyIds)
            {
                Lobby lobby = await LobbyManager.GetLobby(id);
                await lobby.Update();
            }

            RemoveAllButtons(true);
            ReturnButton.Contents = LobbyManager.PublicLobbies.Count > 0 ? MainDescription : EmptyDescription;
            RefreshButton.Contents = ReturnButton.Contents;
            ButtonsLobbies.Clear();

            foreach (Lobby lobby in LobbyManager.PublicLobbies)
            {
                ForumObject button = AddButton(new(GetLobbyString(lobby)), null, MainDescription);
                ButtonsLobbies.Add(button, lobby);
            }
        }

        /// <summary>
        /// Refreshes current public lobbies and displays the updated list.
        /// </summary>
        private async void Refresh()
        {
            UIManager.Debounce = true;

            await Update();

            Main.Dispatcher.Enqueue(() =>
            {
                UIManager.Debounce = false;
                Window.Show();
            });
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == RefreshButton)
            {
                Refresh();
            }
            else if (ButtonsLobbies.TryGetValue(button, out Lobby lobby))
            {
                _ = UIManager.OpenLobbyWindow(lobby);
            }
        }
    }
}
