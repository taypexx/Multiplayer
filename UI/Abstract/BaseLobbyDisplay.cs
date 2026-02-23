using Il2CppDG.Tweening;
using Il2CppDG.Tweening.Core;
using Multiplayer.Data.Lobbies;
using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Abstract
{
    internal abstract class BaseLobbyDisplay
    {
        internal GameObject Frame { get; private set; }
        protected string FrameParentPath { get; set; }
        protected Transform FrameParent => GameObject.Find(FrameParentPath).transform;

        protected TextAnchor TextAnchor { get; set; }
        protected HorizontalWrapMode TextHorizontalWrapMode { get; set; }
        protected VerticalWrapMode TextVerticalWrapMode { get; set; }
        protected int? MaxLines { get; set; }
        protected int FontSize { get; set; }
        protected float EntryWidth { get; 
            set {
                EntrySize = new(value, FontSize + 8);
                FrameSize = new(EntrySize.x, EntrySize.y * (MaxLines ?? 1));
                field = value;
            } 
        }
        protected Vector2 EntrySize { get; private set; }
        protected int EntryDir { get; set; }

        protected Vector2 FrameAnchorPosition { get; set; }
        protected Vector2 FrameSize { get; private set; }
        protected Vector2 Pivot { get; 
            set {
                PopupDir = Pivot.x * 2 - 1;
                field = value;
            } 
        }
        protected float PopupDir { get; private set; }

        protected Text Title;
        protected Dictionary<object, Text> TextList;
        protected List<object> PositionList;

        internal Lobby Lobby;
        internal bool DoesSort = false;

        internal BaseLobbyDisplay()
        {
            TextList = new();
            PositionList = new();
        }

        /// <returns>Total amount of lines current display shows.</returns>
        internal int GetTotalLines(int? untilIndex = null)
        {
            int count = 0;
            Canvas.ForceUpdateCanvases(); // well, this fixes the choppy textgenerator

            if (untilIndex is null)
            {
                foreach ((_, var text) in TextList)
                {
                    count += text.cachedTextGenerator.lineCount;
                }
            } else
            {
                for (int i = 0; i < PositionList.Count; i++)
                {
                    var key = PositionList[i];
                    Text text = TextList[key];
                    count += text.cachedTextGenerator.lineCount;
                }
            }
            return count;
        }

        /// <summary>
        /// Adds a <see cref="Text"/> to the display, aligning it visually.
        /// </summary>
        /// <param name="key">Key to which this <see cref="Text"/> will be linked.</param>
        /// <param name="clickAction">(Optional) Invokes this <see cref="Action"/> when the <see cref="Button"/> is clicked. Will not add the <see cref="Button"/> component if <see langword="null"/>.</param>
        /// <returns>A new <see cref="Text"/>.</returns>
        protected Text AddText(object key, Action clickAction = null)
        {
            if (Frame == null) return null;

            GameObject newTextObj = Utilities.CreateText(Frame.transform, "Entry" + TextList.Count.ToString(), true);
            var rect = newTextObj.GetComponent<RectTransform>();
            rect.anchorMin = Pivot;
            rect.anchorMax = Pivot;
            rect.pivot = Pivot;
            rect.anchoredPosition3D = new();
            rect.sizeDelta = EntrySize;
            rect.localScale = Vector3.one;

            Text text = newTextObj.GetComponent<Text>();
            text.alignment = TextAnchor;
            text.fontSize = FontSize;
            text.horizontalOverflow = TextHorizontalWrapMode;
            text.verticalOverflow = TextVerticalWrapMode;
            text.raycastTarget = clickAction != null;

            if (clickAction != null)
            {
                var button = newTextObj.AddComponent<Button>();
                button.onClick.AddListener(clickAction);
            }

            TextList.Add(key, text);
            PositionList.Add(key);

            SetTextPositions();

            return text;
        }

        /// <summary>
        /// Removes the <see cref="Text"/> from the display.
        /// </summary>
        /// <param name="key">Key to which the <see cref="Text"/> was linked.</param>
        /// <param name="realignAfter">Whether to realign the entire display visually.</param>
        protected void RemoveText(object key)
        {
            Text text = TextList[key];
            var removeAt = PositionList.IndexOf(key);

            TextList.Remove(key);
            PositionList.Remove(key);

            if (text == null) return;

            // Fill the gap
            foreach ((object k, Text t) in TextList)
            {
                if (PositionList.IndexOf(k) >= removeAt)
                {
                    t.gameObject.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0f, EntrySize.y * text.cachedTextGenerator.lineCount);
                }
            }

            GameObject obj = text.gameObject;
            if (obj != null) UnityEngine.Object.Destroy(obj);
        }

        /// <summary>
        /// Clears every <see cref="Text"/> on the display.
        /// </summary>
        /// <param name="keepTitle">Whether to keep the title <see cref="Text"/>.</param>
        internal void ClearText(bool keepTitle = false)
        {
            foreach ((object key, _) in TextList)
            {
                if (keepTitle && key is Lobby) continue;

                RemoveText(key);
            }

            SetTextPositions();
        }

        /// <summary>
        /// Sorts the <see cref="PositionList"/> according to the <see cref="LobbyGoal"/> of the <see cref="Lobby"/>.
        /// </summary>
        protected void Sort()
        {
            if (Lobby is null) return;

            PositionList.Sort(Lobby.GoalComparison);
            SetTextPositions();
        }

        /// <summary>
        /// Sets the position of every text entry according to the previous lines and its own placement.
        /// </summary>
        protected void SetTextPositions()
        {
            var y = 0f;
            for (int i = 0; i < PositionList.Count; i++)
            {
                var key = PositionList[i];
                if (!TextList.TryGetValue(key, out var text)) continue;

                text.GetComponent<RectTransform>().anchoredPosition = new(0f, y);

                Canvas.ForceUpdateCanvases();
                y += EntrySize.y * text.cachedTextGenerator.lineCount * EntryDir;
            }
        }

        /// <summary>
        /// Updates the display to show the <see cref="Data.Lobbies.Lobby"/> information.
        /// </summary>
        internal virtual void Update()
        {
            if (Lobby is null) { Destroy(); return; }

            if (Title)
            {
                Title.text = Lobby.Name + $" <color=#{Constants.Yellow}>({Lobby.Players.Count}/{Lobby.MaxPlayers})</color>";

                if (Settings.Config.DisplayLobbyStatus)
                {
                    Title.text += $" <color=#{(Lobby.IsPrivate ? Constants.Red : Constants.Green)}>({(Lobby.IsPrivate ? "Private" : "Public")})</color>";
                }
            }

            foreach (string playerUid in Lobby.Players)
            {
                Player player = PlayerManager.GetCachedPlayer(playerUid);
                if (player is null) continue;

                if (!TextList.ContainsKey(player))
                {
                    AddText(player);
                }
            }

            foreach ((object key, _) in TextList)
            {
                if (key is Player && !Lobby.Players.Contains(((Player)key).Uid))
                {
                    RemoveText(key);
                }
            }

            if (DoesSort) Sort();

            UpdateTexts();
        }

        /// <summary>
        /// Updates the text contents of every entry.
        /// </summary>
        internal virtual void UpdateTexts()
        {
            foreach ((object key, Text text) in TextList)
            {
                if (key is not Player) continue;
                Player player = (Player)key;

                text.text = player == Lobby.Host ? $"<color=#{Constants.Yellow}>[Host]</color> {player.MultiplayerStats.Name}" : player.MultiplayerStats.Name;
            }
        }

        /// <summary>
        /// Displays a small popup near the main text.
        /// </summary>
        /// <param name="text">Contents of the message.</param>
        /// <param name="key">Key to which the text belongs.</param>
        protected virtual void Popup(string text, object key)
        {
            if (!TextList.TryGetValue(key, out Text owner)) return;

            GameObject popup = GameObject.Instantiate(owner.gameObject, Frame.transform);
            popup.name = text;

            Text popupText = popup.GetComponent<Text>();
            popupText.text = text;

            var rect = popup.GetComponent<RectTransform>();
            rect.anchoredPosition += new Vector2(owner.preferredWidth + 10f, 0f);
            rect.DOMoveX(50f, 1.5f).SetRelative().SetEase(Ease.OutSine).OnComplete((Action)(() => GameObject.Destroy(popup)));

            //DOTween.ToAlpha(new(Marshal.GetFunctionPointerForDelegate(() => popupText.color)), new(Marshal.GetFunctionPointerForDelegate(x => popupText.color = x)), 0f, 1.5f);
        }

        /// <summary>
        /// Creates the display for the given <see cref="Data.Lobbies.Lobby"/>.
        /// </summary>
        /// <param name="lobby"><see cref="Data.Lobbies.Lobby"/> whose information will be displayed.</param>
        internal virtual void Create(Lobby lobby, bool addTitle = true)
        {
            if (lobby is null || Lobby != null) return;
            Lobby = lobby;

            Frame = new("LobbyDisplay");
            var frameRect = Frame.AddComponent<RectTransform>();
            frameRect.SetParent(FrameParent);
            frameRect.anchorMin = Pivot;
            frameRect.anchorMax = Pivot;
            frameRect.pivot = Pivot;
            frameRect.anchoredPosition3D = FrameAnchorPosition;
            frameRect.sizeDelta = FrameSize;
            frameRect.localScale = Vector3.one;

            if (addTitle) Title = AddText(lobby);
            Update();
        }

        /// <summary>
        /// Destroys the current display of the <see cref="Data.Lobbies.Lobby"/>.
        /// </summary>
        internal virtual void Destroy()
        {
            if (Lobby is null) return;
            ClearText();
            GameObject.Destroy(Frame);
            Lobby = null;
        }
    }
}
