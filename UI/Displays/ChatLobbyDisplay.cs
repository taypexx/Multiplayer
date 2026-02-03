using Multiplayer.Data.Lobbies;
using Multiplayer.Data.Chat;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Displays
{
    internal sealed class ChatLobbyDisplay : BaseLobbyDisplay
    {
        private int MessageHistoryIndex = 0;

        internal InputField InputField { get; private set; }
        private Text PlaceholderText;

        private static GameObject TxtStageDesigner => GameObject.Find("UI/Standerd/PnlPreparation/TxtStageDesigner");

        internal ChatLobbyDisplay() : base()
        {
            FrameParentPath = "UI/Standerd/PnlNavigation";

            TextAnchor = TextAnchor.UpperLeft;
            TextHorizontalWrapMode = HorizontalWrapMode.Wrap;
            TextVerticalWrapMode = VerticalWrapMode.Overflow;
            MaxLines = 10;
            FontSize = 20;
            EntryWidth = 400f;
            EntryDir = -1;

            FrameAnchorPosition = new(25f, -90f);
            Pivot = new(0f, 1f);
        }

        /// <summary>
        /// Updates the contents of the placeholder text.
        /// </summary>
        /// <param name="keyEnabled">Whether the chat open key is available.</param>
        internal void UpdatePlaceholder(bool keyEnabled)
        {
            if (PlaceholderText == null) return;
            PlaceholderText.text = Localization.Get("SystemChatMessages", keyEnabled ? "ChatPlaceholderText" : "ChatPlaceholderTextNoKey").ToString();
        }

        /// <summary>
        /// Sets the text of the <see cref="InputField"/> to the message found in history.
        /// </summary>
        /// <param name="up">Whether to check the previous or next message in history.</param>
        internal void BrowseMessageHistory(bool up)
        {
            if (InputField == null) return;

            if (InputField.text != string.Empty) MessageHistoryIndex += up ? 1 : -1;
            if (MessageHistoryIndex < 0 || MessageHistoryIndex > Chat.MessageHistory.Count - 1)
            {
                MessageHistoryIndex = 0;
            }
            InputField.text = Chat.MessageHistory[Chat.MessageHistory.Count - 1 - MessageHistoryIndex] ?? InputField.text;
            InputField.MoveTextEnd(false);
        }

        internal void ResetMessageHistoryIndex()
        {
            MessageHistoryIndex = 0;
        }

        /// <summary>
        /// Adds the <see cref="ChatMessage"/> to the display.
        /// </summary>
        internal void AddMessage(ChatMessage chatMessage)
        {
            if (Lobby is null) return;

            while (GetTotalLines() >= MaxLines)
            {
                RemoveText(PositionList.First());
            }

            Action clickAction = null;
            if (chatMessage.Message == "PlaylistAdd")
            {
                clickAction = new(() => UIManager.JumpToChart(chatMessage.ExtraData));
            }
            else if (!chatMessage.IsSystemMessage && chatMessage.AuthorUid != null)
            {
                clickAction = new(() => _ = UIManager.OpenProfileWindow(chatMessage.AuthorUid));
            }

            Text text = AddText(chatMessage, clickAction);
            text.text = chatMessage.ToString();

            PositionList.Remove(Lobby);
            PositionList.Add(Lobby);

            SetTextPositions();
            Update();
        }

        internal override void Update()
        {
            if (Lobby is null) { Destroy(); return; }
            if (Title == null || PlaceholderText == null) return;

            PlaceholderText.transform.position = Title.transform.position;
        }

        internal override void Create(Lobby lobby, bool addTitle = true)
        {
            base.Create(lobby, addTitle);
            Frame.SetActive(Settings.Config.EnableChat);

            InputField = Title.gameObject.AddComponent<InputField>();
            InputField.textComponent = Title;
            InputField.lineType = InputField.LineType.MultiLineNewline;
            InputField.characterLimit = Constants.ChatMessageCharactersMax;

            Title.raycastTarget = true;

            var placeholderText = GameObject.Instantiate(Title.gameObject, Title.transform.parent);
            placeholderText.name = "PlaceholderEntry";
            Component.Destroy(placeholderText.GetComponent<InputField>());

            PlaceholderText = placeholderText.GetComponent<Text>();
            PlaceholderText.raycastTarget = false;
            PlaceholderText.horizontalOverflow = HorizontalWrapMode.Overflow;
            PlaceholderText.color = new(1f, 1f, 1f, 0.7f);
            UpdatePlaceholder(Settings.Config.EnableShortcuts);
            InputField.placeholder = PlaceholderText;

            // Disable text designer because it overlaps and looks fucking ugly
            TxtStageDesigner.SetActive(false);
        }

        internal override void Destroy()
        {
            base.Destroy();
            TxtStageDesigner.SetActive(true);
        }
    }
}
