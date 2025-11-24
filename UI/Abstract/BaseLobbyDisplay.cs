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
        internal string TextReferencePath;
        internal GameObject TextReference => GameObject.Find(TextReferencePath);

        internal string ParentPath;
        internal Transform Parent => GameObject.Find(ParentPath).transform;

        internal Vector3 AnchorPosition;
        internal Vector3 Step;
        internal TextAnchor TextAnchor;

        internal Text Title;
        internal Dictionary<object, Text> TextList;
        internal List<object> PositionList;

        internal Lobby Lobby;

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
            if (TextReference is null || Parent is null)
            {
                Main.Logger.Error("Couldn't find TextReference or Parent");
                return null;
            }

            GameObject newTextObj = UnityEngine.Object.Instantiate(TextReference, Parent);
            newTextObj.name = "LobbyEntry";
            UnityEngine.Object.Destroy(newTextObj.GetComponent<Il2CppAssets.Scripts.PeroTools.GeneralLocalization.Localization>());
            newTextObj.transform.localPosition = AnchorPosition + Step * TextList.Count;

            Text text = newTextObj.GetComponent<Text>();
            text.alignment = TextAnchor;
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
        internal void RemoveText(object key, bool realignAfter = true)
        {
            Text text = TextList[key];
            TextList.Remove(key);
            PositionList.Remove(key);
            if (text != null)
            {
                GameObject obj = text.gameObject;
                if (obj != null) UnityEngine.Object.Destroy(obj);
            }

            if (realignAfter) RealignText();
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

                RemoveText(key, false);
            }
        }

        /// <summary>
        /// Realigns the text on the display.
        /// </summary>
        internal void RealignText()
        {
            foreach(object key in PositionList)
            {
                Text text = TextList[key];
                text.transform.localPosition = AnchorPosition + Step * PositionList.IndexOf(key);
            }
        }

        /// <summary>
        /// Sorts the <see cref="PositionList"/> according to the provided <see cref="LobbyGoal"/>.
        /// </summary>
        /// <param name="goal"></param>
        internal void SortAccordingTo(LobbyGoal goal)
        {
            switch (goal)
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
            RealignText();
        }

        /// <summary>
        /// Creates the display for the given <see cref="Data.Lobby"/>.
        /// </summary>
        /// <param name="lobby"><see cref="Data.Lobby"/> whose information will be displayed.</param>
        internal void Create(Lobby lobby)
        {
            if (lobby is null || Lobby != null) return;
            Lobby = lobby;

            Title = AddText(lobby);
            _ = Update(false);
        }

        /// <summary>
        /// Updates the display to show the <see cref="Data.Lobby"/> information.
        /// </summary>
        /// <param name="updateLobby">Whether to update the <see cref="Data.Lobby"/> as well.</param>
        internal async Task Update(bool updateLobby = false)
        {
            if (Lobby is null) { Destroy(); return; }
            if (updateLobby)
            {
                await Lobby.Update();
            }

            Main.Dispatcher.Enqueue(() =>
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
                        AddText(player).text = player == Lobby.Host ? $"<color=#fff700ff>{player.MultiplayerStats.Name}</color>" : player.MultiplayerStats.Name;
                    }
                }

                foreach ((object key, _) in TextList)
                {
                    if (key is Player && !Lobby.Players.Contains(((Player)key).Uid))
                    {
                        RemoveText(key);
                    }
                }
            });
        }

        /// <summary>
        /// Destroys the current display of the <see cref="Data.Lobby"/>.
        /// </summary>
        internal void Destroy()
        {
            ClearText();
            Lobby = null;
        }
    }
}
