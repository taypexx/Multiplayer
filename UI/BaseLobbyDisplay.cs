using Multiplayer.Data;
using Multiplayer.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI
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
        internal Dictionary<object,Text> TextList;

        internal Lobby Lobby;

        internal BaseLobbyDisplay()
        {
            TextList = new();
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

            GameObject newTextObj = GameObject.Instantiate(TextReference,Parent);
            newTextObj.name = "LobbyEntry";
            Component.Destroy(newTextObj.GetComponent<Il2CppAssets.Scripts.PeroTools.GeneralLocalization.Localization>());
            newTextObj.transform.localPosition = AnchorPosition + (Step * TextList.Count);

            Text text = newTextObj.GetComponent<Text>();
            text.alignment = TextAnchor;
            TextList.Add(key, text);
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
            if (text != null)
            {
                GameObject obj = text.gameObject;
                if (obj != null) GameObject.Destroy(obj);
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
            for (int i = 0; i < TextList.Count; i++)
            {
                Text text = TextList.ElementAt(i).Value;
                text.transform.localPosition = AnchorPosition + (Step * i);
            }
        }

        /// <summary>
        /// Starts the auto update loop and updates the lobby every <see cref="AutoUpdateInterval"/>.
        /// </summary>
        /// <returns></returns>
        private async Task AutoUpdateStart()
        {
            while (Lobby != null)
            {
                await Task.Delay(LobbyManager.AutoUpdateInterval);
                if (UIManager.LobbyWindow.IsAutoUpdating || UIManager.LobbyWindow.UpdateDebounce) continue;

                await Update(true);
            }
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
            _ = AutoUpdateStart();
        }

        /// <summary>
        /// Updates the display to show the <see cref="Data.Lobby"/> information.
        /// </summary>
        /// <param name="updateLobby">Whether to update the <see cref="Data.Lobby"/> as well.</param>
        internal async Task Update(bool updateLobby = false)
        {
            if (Lobby is null) return;
            if (updateLobby)
            {
                await Lobby.Update();
            }

            Main.Dispatcher.Enqueue(() => 
            {
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
