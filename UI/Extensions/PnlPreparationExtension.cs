using CustomAlbums.Managers;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.UI;
using Multiplayer.Data.Lobbies;
using Multiplayer.Managers;
using Multiplayer.Static;
using PopupLib.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.UI.Extensions
{
    internal static class PnlPreparationExtension
    {
        internal static bool IsRetrieving = false;
        private static bool PlaylistDebounce = false;

        private static async Task DoPlaylistDebounce()
        {
            PlaylistDebounce = true;
            await Task.Delay(2000);
            PlaylistDebounce = false;
        }

        internal static void BindCustomPnlPreparationClick(PnlPreparation pnlPreparation)
        {
            var pnlPreparationButton = pnlPreparation.transform.Find("Start/BtnStart").GetComponent<Button>();
            pnlPreparationButton.onClick.RemoveAllListeners();
            pnlPreparationButton.onClick.AddListener((UnityAction)new Action(OnPnlPreparationClick));

            UpdatePnlPreparation();
        }

        /// <summary>
        /// Replaces the functionality of the vanilla <see cref="PnlPreparation"/> button.
        /// </summary>
        internal static void OnPnlPreparationClick()
        {
            if (IsRetrieving || PlaylistDebounce) return;

            var lobby = LobbyManager.LocalLobby;

            if (!LobbyManager.IsInLobby)
            {
                UIManager.PnlPreparation.OnBattleStart();
            }
            else if (LobbyManager.CanChangePlaylist)
            {
                SoundManager.PlayClick();

                MusicInfo musicInfo = GlobalDataBase.dbMusicTag.CurMusicInfo();
                int difficulty = ChartManager.CurrentDifficulty;

                if (difficulty == 5)
                {
                    PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "TouhouNotSupported"));
                    return;
                }
                else if (musicInfo.albumIndex == AlbumManager.Uid)
                {
                    if (lobby.PlayType == LobbyPlayType.VanillaOnly)
                    {
                        PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "VanillaOnly"));
                        return;
                    }
                    else if (ChartManager.GetCustomChartData(musicInfo.uid).IsOnWebsite != true)
                    {
                        PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "WebsiteOnly"));
                        return;
                    }
                }
                else if (lobby.PlayType == LobbyPlayType.CustomOnly)
                {
                    PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "CustomOnly"));
                    return;
                }
                else if (!lobby.IsValidDifficulty(musicInfo.GetDifficultyLevel(difficulty)))
                {
                    PopupUtils.ShowInfo(String.Format(
                        Localization.Get("PnlPreparation", "InvalidDifficulty").ToString(), 
                        lobby.DifficultyRange.Item1, 
                        lobby.DifficultyRange.Item2
                    ));
                    return;
                }
                else if (Constants.UnsupportedChartUids.Contains(musicInfo.uid))
                {
                    PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "ChartNotSupported"));
                    return;
                }

                _ = DoPlaylistDebounce();

                if (lobby.HasInPlaylist(ChartManager.GetEntry(musicInfo, difficulty)))
                {
                    _ = LobbyManager.PlaylistRemove(musicInfo, difficulty);
                }
                else if (!lobby.IsPlaylistFull)
                {
                    _ = LobbyManager.PlaylistAdd(musicInfo, difficulty);
                }
            }
            else if (lobby.Locked)
            {
                PopupUtils.ShowInfo(Localization.Get("Lobby", "LobbyIsLocked"));
            }
        }

        /// <summary>
        /// Locks/unlocks <see cref="PnlPreparation"/> depending on the local <see cref="Lobby"/>.
        /// </summary>
        internal static void UpdatePnlPreparation()
        {
            if (!Main.IsUIScene || UIManager.PnlPreparation is null) return;

            MusicInfo curMusicInfo = GlobalDataBase.dbMusicTag.CurMusicInfo();
            if (curMusicInfo is null) return;

            GameObject playObject = GameObject.Find("UI/Standerd/PnlPreparation/Start/BtnStart");
            GameObject imgObject = playObject.transform.Find("TxtStart/ImgBtnA").gameObject;
            Text playText = playObject.transform.Find("TxtStart").GetComponent<Text>();
            Button playButton = playObject.GetComponent<Button>();
            InputKeyBinding keyBinding = playObject.GetComponent<InputKeyBinding>();

            bool isRemoving = LobbyManager.IsInLobby && LobbyManager.LocalLobby.HasInPlaylist(ChartManager.GetEntry(curMusicInfo, ChartManager.CurrentDifficulty));
            bool isFull = LobbyManager.IsInLobby && LobbyManager.LocalLobby.IsPlaylistFull;

            playButton.enabled = (!LobbyManager.IsInLobby || LobbyManager.CanChangePlaylist) && (isRemoving || !isFull) && !IsRetrieving;
            keyBinding.enabled = playButton.enabled;
            imgObject.SetActive(playButton.enabled);

            if (!LobbyManager.IsInLobby)
            {
                playText.text = "PLAY!";
            }
            else if (!LobbyManager.CanChangePlaylist)
            {
                playText.text = Localization.Get("PnlPreparation", "Waiting").ToString();
            }
            else if (IsRetrieving)
            {
                playText.text = Localization.Get("PnlPreparation", "RetrievingInfo").ToString();
            }
            else
            {
                playText.text = Localization.Get("PnlPreparation",
                    isRemoving
                    ? "PlaylistRemove"
                    : isFull ? "PlaylistFull" : "PlaylistAdd"
                ).ToString();
            }
        }
    }
}
