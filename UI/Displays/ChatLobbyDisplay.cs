using Multiplayer.Data.Lobbies;
using Multiplayer.Data.Websocket;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Displays
{
    internal sealed class ChatLobbyDisplay : BaseLobbyDisplay
    {
        internal int MaxLines { get; private set; } = 10;
        internal InputField InputField { get; private set; }
        private GameObject PlaceholderText;

        internal ChatLobbyDisplay() : base()
        {
            ParentPath = "UI/Standerd/PnlNavigation";
            AnchorPosition = new(-635f, 350f, 0f);
            EntrySize = new(600f, 200f);
            Step = new(0f, -30f, 0f);
            PopupOffset = new(-135f, 0, 0);
            PopupX = 50f;
            TextAnchor = TextAnchor.UpperLeft;
            TextHorizontalWrapMode = HorizontalWrapMode.Wrap;
            FontSize = 20;
        }

        internal override void Update()
        {
            if (Lobby is null) { Destroy(); return; }
            if (Title == null || PlaceholderText == null) return;

            PlaceholderText.transform.position = Title.transform.position;
        }

        internal void AddMessage(ChatMessage chatMessage)
        {
            while (GetTotalLines() >= MaxLines)
            {
                RemoveText(PositionList.First());
            }

            Text text = AddText(chatMessage, chatMessage.Message != "PlaylistAdd" ? null : new(() => UIManager.JumpToChart(chatMessage.ExtraData)));
            text.text = chatMessage.ToString();

            PositionList.Remove(Lobby);
            PositionList.Add(Lobby);

            SetTextPositions();
            Update();
        }

        internal override void Create(Lobby lobby, bool addTitle = true)
        {
            base.Create(lobby, addTitle);

            InputField = Title.gameObject.AddComponent<InputField>();
            InputField.textComponent = Title;
            InputField.lineType = UnityEngine.UI.InputField.LineType.MultiLineSubmit;
            InputField.characterLimit = Constants.ChatMessageCharactersMax;

            Title.raycastTarget = true;

            PlaceholderText = GameObject.Instantiate(Title.gameObject, Title.transform.parent);
            PlaceholderText.name = "PlaceholderEntry";
            Component.Destroy(PlaceholderText.GetComponent<InputField>());

            var placeholderText = PlaceholderText.GetComponent<Text>();
            placeholderText.raycastTarget = false;
            placeholderText.horizontalOverflow = HorizontalWrapMode.Overflow;
            placeholderText.color = new(1f, 1f, 1f, 0.7f);
            placeholderText.text = Localization.Get("SystemChatMessages", "ChatPlaceholderText").ToString();
            InputField.placeholder = placeholderText;
        }

        internal override void Destroy()
        {
            base.Destroy();
            GameObject.Destroy(PlaceholderText);
        }
    }
}
