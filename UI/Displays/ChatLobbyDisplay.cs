using Multiplayer.Data.Lobbies;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Displays
{
    internal sealed class ChatLobbyDisplay : BaseLobbyDisplay
    {
        internal int MaxMessages { get; private set; } = 12;
        private InputField InputField;
        private GameObject PlaceholderText;

        internal ChatLobbyDisplay() : base()
        {
            ParentPath = "UI/Standerd/PnlNavigation";
            AnchorPosition = new(-830f, 350f, 0f);
            Step = new(0f, -35f, 0f);
            PopupOffset = new(-135f, 0, 0);
            PopupX = 50f;
            TextAnchor = TextAnchor.UpperLeft;
            FontSize = 26;
        }

        internal override void Update()
        {
            if (Lobby is null) { Destroy(); return; }
            if (Title == null || PlaceholderText == null) return;

            PlaceholderText.transform.position = Title.transform.position;
        }

        internal void AddMessage(ChatMessage chatMessage)
        {
            if (TextList.Count >= MaxMessages)
            {
                RemoveText(PositionList.First());
            }

            AddText(chatMessage);
            Update();
        }

        internal override void Create(Lobby lobby, bool addTitle = true)
        {
            base.Create(lobby, addTitle);

            InputField = Title.gameObject.AddComponent<InputField>();
            InputField.textComponent = Title;
            InputField.characterLimit = Constants.ChatMessageCharactersMax;

            PlaceholderText = GameObject.Instantiate(Title.gameObject, Title.transform.parent);
            PlaceholderText.name = "PlaceholderEntry";
            Component.Destroy(PlaceholderText.GetComponent<InputField>());

            var placeholderText = PlaceholderText.GetComponent<Text>();
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
