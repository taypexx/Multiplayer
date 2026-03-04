using Multiplayer.Data.Chat;
using Multiplayer.Data.Lobbies;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Multiplayer.UI.Displays
{
    internal sealed class ChatLobbyDisplay : BaseLobbyDisplay
    {
        private int MessageHistoryIndex = 0;
        private float OutlineOffset = 25f;
        private int MaxVisibleLines = 9;

        private Vector2 GetFrameSize(int lines) => new(FrameSize.x, EntrySize.y * (lines + (lines > (MaxVisibleLines + 1) ? 0.5f : 0f)));
        private Vector2 GetScrollFrameSize(int lines) => 
            lines > MaxVisibleLines
            ? ScrollFrameSize
            : new(EntrySize.x + OutlineOffset, EntrySize.y * lines + OutlineOffset);

        internal InputField InputField { get; private set; }
        private Text PlaceholderText;
        private Dictionary<string, Action> OpenProfileActions;

        private static GameObject TxtStageDesigner => GameObject.Find("UI/Standerd/PnlPreparation/TxtStageDesigner");
        private static Sprite BtnBaseSprite = Addressables.LoadAssetAsync<Sprite>("BtnBase").WaitForCompletion();

        internal ChatLobbyDisplay() : base()
        {
            FrameParentPath = "UI/Standerd/PnlNavigation";

            TextAnchor = TextAnchor.UpperLeft;
            TextHorizontalWrapMode = HorizontalWrapMode.Wrap;
            TextVerticalWrapMode = VerticalWrapMode.Overflow;
            MaxLines = null;
            FontSize = 20;
            EntryWidth = 480f;
            EntryDir = -1;

            ScrollFrameSize = new(EntrySize.x + OutlineOffset, EntrySize.y * (MaxVisibleLines + 1) + OutlineOffset);

            FrameAnchorPosition = new(10f, -90f);
            Pivot = new(0f, 1f);
            OpenProfileActions = new();
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
        internal void ResetMessageHistoryIndex() => MessageHistoryIndex = 0;

        internal void ScrollToBottom() => ScrollRect.SetNormalizedPosition(0, 1);

        /// <summary>
        /// Adds the <see cref="ChatMessage"/> to the display.
        /// </summary>
        internal void AddMessage(ChatMessage chatMessage)
        {
            if (Lobby is null) return;

            Action clickAction = null;
            if (chatMessage.Message == "PlaylistAdd")
            {
                clickAction = new(() => UIManager.JumpToChart(chatMessage.ExtraData));
            }
            else if (!chatMessage.IsSystemMessage && chatMessage.AuthorUid != null)
            {
                if (OpenProfileActions.TryGetValue(chatMessage.AuthorUid, out var action))
                {
                    clickAction = action;
                }
                else
                {
                    clickAction = new(() => _ = UIManager.OpenProfileWindow(chatMessage.AuthorUid));
                    OpenProfileActions.Add(chatMessage.AuthorUid, action);
                }
            }

            Text text = AddText(chatMessage, clickAction);
            text.text = chatMessage.ToString();

            PositionList.Remove(Lobby);
            PositionList.Add(Lobby);

            SetTextPositions();

            var lineCount = text.cachedTextGenerator.lineCount;
            if (lineCount > 1)
            {
                text.GetComponent<RectTransform>().sizeDelta = new(EntrySize.x, EntrySize.y * lineCount);
            }

            Update();

            if (!ScrollRect.m_Dragging && (ScrollRect.verticalNormalizedPosition < 0.1f || chatMessage.IsSystemMessage || chatMessage.AuthorUid == PlayerManager.LocalPlayerUid))
            {
                ScrollToBottom();
            }
        }

        internal override void Update()
        {
            if (Lobby is null) { Destroy(); return; }
            if (Title == null || PlaceholderText == null) return;

            PlaceholderText.transform.position = Title.transform.position;

            var totalLines = GetTotalLines();
            Frame.GetComponent<RectTransform>().sizeDelta = GetFrameSize(totalLines);
            ScrollFrame.GetComponent<RectTransform>().sizeDelta = GetScrollFrameSize(totalLines);
        }

        internal override void Create(Lobby lobby, bool addTitle = true, bool scrollable = false)
        {
            base.Create(lobby, addTitle, scrollable);
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
            PlaceholderText.text = Localization.Get("SystemChatMessages", "ChatPlaceholderText").ToString();
            InputField.placeholder = PlaceholderText;

            var totalLines = GetTotalLines();
            Frame.GetComponent<RectTransform>().sizeDelta = GetFrameSize(totalLines);
            ScrollFrame.GetComponent<RectTransform>().sizeDelta = GetScrollFrameSize(totalLines);

            var background = ScrollFrame.AddComponent<Image>();
            background.type = Image.Type.Tiled;
            background.sprite = BtnBaseSprite;
            background.color = new(0f, 0f, 0f, 0.15f);

            // Disable text designer because it overlaps and looks fucking ugly
            TxtStageDesigner.SetActive(false);
        }

        internal override void Destroy()
        {
            base.Destroy();
            if (TxtStageDesigner != null) TxtStageDesigner.SetActive(true);
            OpenProfileActions.Clear();
        }
    }
}
