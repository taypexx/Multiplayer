using Multiplayer.Data;
using Multiplayer.Data.LobbyEnums;
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
        internal TextAnchor TextAnchor;

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
            newTextObj.GetComponent<RectTransform>().sizeDelta = new(600,200);

            Text text = newTextObj.GetComponent<Text>();
            text.alignment = TextAnchor;
            text.fontSize = 26;
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
                        return ((Player)t1).BattleStats.Accuracy.CompareTo(((Player)t2).BattleStats.Accuracy);
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
        /// Creates the display for the given <see cref="Data.Lobby"/>.
        /// </summary>
        /// <param name="lobby"><see cref="Data.Lobby"/> whose information will be displayed.</param>
        internal void Create(Lobby lobby, bool addTitle = true)
        {
            if (lobby is null || Lobby != null) return;
            Lobby = lobby;

            if (addTitle) Title = AddText(lobby);
            Update();
        }

        /// <summary>
        /// Updates the display to show the <see cref="Data.Lobby"/> information.
        /// </summary>
        internal void Update()
        {
            if (Lobby is null) { Destroy(); return; }

            if (Title) Title.text = $"{Lobby.Name} " +
             $"<color=#fff700ff>({Lobby.Players.Count}/{Lobby.MaxPlayers})</color> " +
             $"<color=#{(Lobby.IsPrivate ? "f542adff" : "1eff00ff")}>({(Lobby.IsPrivate ? "Private" : "Public")})</color>";

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

            UpdateTexts();

            if (DoesSort) Sort();
        }

        internal virtual void UpdateTexts()
        {
            foreach ((object key, Text text) in TextList)
            {
                if (key is not Player) continue;
                Player player = (Player)key;

                text.text = player == Lobby.Host ? $"<color=#fff700ff>{player.MultiplayerStats.Name}</color>" : player.MultiplayerStats.Name;
            }
        }

        /// <summary>
        /// Destroys the current display of the <see cref="Data.Lobby"/>.
        /// </summary>
        internal void Destroy()
        {
            if (Lobby is null) return;
            ClearText();
            Lobby = null;
        }
    }
}
