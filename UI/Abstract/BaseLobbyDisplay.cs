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
        internal string ParentPath;
        internal Transform Parent => GameObject.Find(ParentPath).transform;

        internal Vector3 AnchorPosition;
        internal Vector3 Step;
        internal Vector3 PopupOffset;
        internal float PopupX;
 
        internal TextAnchor TextAnchor;
        internal int FontSize;

        internal Text Title;
        internal Dictionary<object, Text> TextList;
        internal List<object> PositionList;

        internal Lobby Lobby;
        internal bool DoesSort = false;

        internal BaseLobbyDisplay()
        {
            TextList = new();
            PositionList = new();
        }

        /// <summary>
        /// Adds a <see cref="Text"/> to the display, aligning it visually.
        /// </summary>
        /// <param name="key">Key to which this <see cref="Text"/> will be linked.</param>
        /// <returns>A new <see cref="Text"/>.</returns>
        internal Text AddText(object key)
        {
            if (Parent is null)
            {
                Main.Logger.Error("Couldn't find the parent gameobject.");
                return null;
            }

            GameObject newTextObj = Utilities.CreateText(Parent, "LobbyEntry");
            newTextObj.transform.localPosition = AnchorPosition + Step * TextList.Count;
            newTextObj.transform.localScale = Vector3.one;
            newTextObj.GetComponent<RectTransform>().sizeDelta = new(200f,200f);

            Text text = newTextObj.GetComponent<Text>();
            text.alignment = TextAnchor;
            text.fontSize = FontSize;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;

            TextList.Add(key, text);
            PositionList.Add(key);

            return text;
        }

        /// <summary>
        /// Removes the <see cref="Text"/> from the display.
        /// </summary>
        /// <param name="key">Key to which the <see cref="Text"/> was linked.</param>
        /// <param name="realignAfter">Whether to realign the entire display visually.</param>
        internal void RemoveText(object key)
        {
            Text text = TextList[key];
            TextList.Remove(key);
            PositionList.Remove(key);
            if (text != null)
            {
                GameObject obj = text.gameObject;
                if (obj != null) UnityEngine.Object.Destroy(obj);
            }
        }

        /// <summary>
        /// Clears every <see cref="Text"/> on the display.
        /// </summary>
        /// <param name="keepTitle">Whether to keep the title <see cref="Text"/>.</param>
        internal void ClearText(bool keepTitle = false)
        {
            foreach ((object key, _) in TextList)
            {
                if (keepTitle && key is not Player) continue;

                RemoveText(key);
            }
        }

        /// <summary>
        /// Sorts the <see cref="PositionList"/> according to the <see cref="LobbyGoal"/> of the <see cref="Lobby"/>.
        /// </summary>
        internal void Sort()
        {
            if (Lobby is null) return;

            switch (Lobby.Goal)
            {
                case LobbyGoal.Accuracy:
                    PositionList.Sort((t1,t2) => 
                    {
                        if (t1 is not Player || t2 is not Player) return 0;
                        var battlestats1 = ((Player)t1).BattleStats;
                        var battlestats2 = ((Player)t2).BattleStats;

                        if (battlestats1.Accuracy == battlestats2.Accuracy) 
                        {
                            return ((battlestats1.Earlies + battlestats1.Lates)*-1).CompareTo(((battlestats2.Earlies + battlestats2.Lates)*-1));
                        } 
                        else return battlestats1.Accuracy.CompareTo(battlestats2.Accuracy);
                    });
                    break;
                case LobbyGoal.Score:
                    PositionList.Sort((t1, t2) =>
                    {
                        if (t1 is not Player || t2 is not Player) return 0;
                        return ((Player)t1).BattleStats.Score.CompareTo(((Player)t2).BattleStats.Score);
                    });
                    break;
                case LobbyGoal.Custom:
                    break;
            }

            foreach (object key in PositionList)
            {
                Text text = TextList[key];
                text.transform.localPosition = AnchorPosition + Step * PositionList.IndexOf(key);
            }
        }

        /// <summary>
        /// Updates the display to show the <see cref="Data.Lobbies.Lobby"/> information.
        /// </summary>
        internal void Update()
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

        internal virtual void UpdateTexts()
        {
            foreach ((object key, Text text) in TextList)
            {
                if (key is not Player) continue;
                Player player = (Player)key;

                text.text = player == Lobby.Host ? $"<color=#fff700ff>[Host]</color> {player.MultiplayerStats.Name}" : player.MultiplayerStats.Name;
            }
        }

        /// <summary>
        /// Displays a small popup near the main text.
        /// </summary>
        /// <param name="text">Contents of the message.</param>
        /// <param name="key">Key to which the text belongs.</param>
        internal virtual void Popup(string text, object key)
        {
            if (!TextList.TryGetValue(key, out Text owner)) return;

            GameObject popup = GameObject.Instantiate(owner.gameObject, Parent);
            popup.name = "EntryPopup";
            popup.transform.localPosition = owner.transform.localPosition + PopupOffset;
            popup.transform.DOMoveX(PopupX, 1.5f).SetRelative().SetEase(Ease.InOutSine).OnComplete((Action)(() => GameObject.Destroy(popup)));

            Text popupText = popup.GetComponent<Text>();
            popupText.text = text;

            //DOTween.ToAlpha(new(Marshal.GetFunctionPointerForDelegate(() => popupText.color)), new(Marshal.GetFunctionPointerForDelegate(x => popupText.color = x)), 0f, 1.5f);
        }

        /// <summary>
        /// Creates the display for the given <see cref="Data.Lobbies.Lobby"/>.
        /// </summary>
        /// <param name="lobby"><see cref="Data.Lobbies.Lobby"/> whose information will be displayed.</param>
        internal void Create(Lobby lobby, bool addTitle = true)
        {
            if (lobby is null || Lobby != null) return;
            Lobby = lobby;

            if (addTitle) Title = AddText(lobby);
            Update();
        }

        /// <summary>
        /// Destroys the current display of the <see cref="Data.Lobbies.Lobby"/>.
        /// </summary>
        internal void Destroy()
        {
            if (Lobby is null) return;
            ClearText();
            Lobby = null;
        }
    }
}
