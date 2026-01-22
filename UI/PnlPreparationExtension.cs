using CustomAlbums.Managers;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.UI;
using Il2CppDG.Tweening.Core.Easing;
using Multiplayer.Data.Lobbies;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Displays;
using PopupLib.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.UI
{
    internal static class PnlPreparationExtension
    {
        internal static void BindCustomPnlPreparationClick(PnlPreparation pnlPreparation)
        {
            var pnlPreparationButton = pnlPreparation.transform.Find("Start/BtnStart").GetComponent<Button>();
            pnlPreparationButton.onClick.RemoveAllListeners();
            pnlPreparationButton.onClick.AddListener((UnityAction)new Action(() => _ = OnPnlPreparationClick()));

            UpdatePnlPreparation();
        }

        /// <summary>
        /// Replaces the functionality of the vanilla PnlPreparation button.
        /// </summary>
        internal static async Task OnPnlPreparationClick()
        {
            if (!LobbyManager.IsInLobby)
            {
                UIManager.PnlPreparation.OnBattleStart();
            }
            else if (LobbyManager.CanChangePlaylist)
            {
                MusicInfo musicInfo = GlobalDataBase.dbMusicTag.CurMusicInfo();
                int difficulty = ChartManager.CurrentDifficulty;

                if (musicInfo.albumIndex == AlbumManager.Uid)
                {
                    if (LobbyManager.LocalLobby.PlayType == LobbyPlayType.VanillaOnly)
                    {
                        PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "VanillaOnly"));
                        return;
                    }
                    else if (!await ChartManager.IsCustomOnWebsite(musicInfo.uid))
                    {
                        PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "WebsiteOnly"));
                        return;
                    }
                }
                else if (LobbyManager.LocalLobby.PlayType == LobbyPlayType.CustomOnly)
                {
                    PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "CustomOnly"));
                    return;
                }

                if (LobbyManager.LocalLobby.HasInPlaylist(ChartManager.GetEntry(musicInfo, difficulty)))
                {
                    _ = LobbyManager.PlaylistRemove(musicInfo, difficulty);
                }
                else if (!LobbyManager.LocalLobby.IsPlaylistFull)
                {
                    _ = LobbyManager.PlaylistAdd(musicInfo, difficulty);
                }
            }
            else if (LobbyManager.LocalLobby.Locked)
            {
                PopupUtils.ShowInfo(Localization.Get("Lobby", "LobbyIsLocked"));
            }
        }

        /// <summary>
        /// Locks/unlocks PnlPreparation depending on the local lobby.
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

            playButton.enabled = (!LobbyManager.IsInLobby || LobbyManager.CanChangePlaylist) && (isRemoving || !isFull);
            keyBinding.enabled = playButton.enabled;
            imgObject.SetActive(playButton.enabled);

            if (!LobbyManager.IsInLobby)
            {
                playText.text = "PLAY!";
            }
            else if (LobbyManager.CanChangePlaylist)
            {
                playText.text = Localization.Get("PnlPreparation",
                    isRemoving
                    ? "PlaylistRemove"
                    : isFull ? "PlaylistFull" : "PlaylistAdd"
                ).ToString();
            }
            else playText.text = Localization.Get("PnlPreparation", "Waiting").ToString();
        }

        /// <summary>
        /// Registers the playlist entry as played, jumps to the chart and locks/unlocks the hidden difficulty if needed.
        /// </summary>
        internal static void PrepareBeforeBattle()
        {
            var entry = LobbyManager.LocalLobby.CurrentPlaylistEntry;
            entry.Play();

            UIManager.JumpToChart(entry.MusicInfo.uid);

            var specialSongManager = Singleton<SpecialSongManager>.instance;
            var currentHiddenUnlocked = specialSongManager.IsInvokeHideBms(entry.MusicInfo.uid);

            // If the current chart's hidden is not unlocked and entry diff is 4
            if (entry.Difficulty == 4 && !currentHiddenUnlocked)
            {
                specialSongManager.InvokeHideBms(entry.MusicInfo, true);
            }
            // If the current chart's hidden is unlocked and entry diff is 3
            else if (entry.Difficulty == 3 && currentHiddenUnlocked)
            {
                // TODO: figure this out
            }

            GlobalDataBase.dbMusicTag.selectedDiffTglIndex = entry.Difficulty;
            GlobalDataBase.dbMusicTag.pnlSelectMusicUid = entry.MusicInfo.uid;
        }
    }
}
