using LocalizeLib;
using Multiplayer.Data;
using Multiplayer.Managers;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;
using System.Net.Http.Json;

namespace Multiplayer.UI
{
    internal sealed class PublicLobbiesWindow : BaseMultiplayerWindow
    {
        private Dictionary<ForumObject, Lobby> ButtonsLobbies;

        private static LocalString MainDescription => Localization.Get("PublicLobbies", "Description");
        private static LocalString EmptyDescription => Localization.Get("PublicLobbies", "DescriptionEmpty");

        internal PublicLobbiesWindow() : base(Localization.Get("PublicLobbies","Title"), UIManager.LobbiesWindow, "Lobbies.png")
        {
            ButtonsLobbies = new();
            AddReturnButton();
            AddRefreshButton();
        }

        /// <summary>
        /// Updates current public lobbies.
        /// </summary>
        internal async Task Update()
        {
            var payload = new
            {
                Token = Client.Token,
                Uid = PlayerManager.LocalPlayer.Uid
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
            AddRefreshButton();

            ReturnButton.Contents = LobbyManager.PublicLobbies.Count > 0 ? MainDescription : EmptyDescription;
            ButtonsLobbies.Clear();

            foreach (Lobby lobby in LobbyManager.PublicLobbies)
            {
                ForumObject button = AddButton(new($"{lobby.Name} ({lobby.Players.Count}/{lobby.MaxPlayers})"), null, MainDescription);
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

        internal override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == RefreshButton)
            {
                Refresh();
            } else if (ButtonsLobbies.TryGetValue(button, out Lobby lobby))
            {
                OpenLobbyWindow(lobby);
            }
        }
    }
}
