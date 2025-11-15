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
                await LobbyManager.GetLobby(id);
            }

            var publicLobbies = LobbyManager.PublicLobbies;

            RemoveAllButtons(true);
            AddRefreshButton();

            ReturnButton.Contents = publicLobbies.Count > 0 ? MainDescription : EmptyDescription;
            ButtonsLobbies.Clear();

            foreach (Lobby lobby in publicLobbies)
            {
                ForumObject button = AddButton(new($"{lobby.Name} ({lobby.Players.Count}/{lobby.MaxPlayers})"), UIManager.LobbyWindow, MainDescription);
                ButtonsLobbies.Add(button, lobby);
            }
        }
        internal override void OnButtonClick(IListWindow window, int objectIndex)
        {
            ForumObject button = Window.ForumObjects[objectIndex];

            if (ButtonsLobbies.TryGetValue(button, out Lobby lobby))
            {
                UIManager.LobbyWindow.Update(lobby);
            }

            if (button == RefreshButton)
            {
                Update();
                base.OnButtonClick(window, objectIndex);
            } else
            {
                base.OnButtonClick(window, objectIndex);
            }
        }
    }
}
