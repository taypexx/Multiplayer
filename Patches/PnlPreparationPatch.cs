using CustomAlbums.Managers;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Multiplayer.Data.Lobbies;
using Multiplayer.Managers;
using Multiplayer.Static;
using PopupLib.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.Patches
{
    internal static class PnlPreparationPatch
    {
        private static SpecialSongManager SpecialSongManager => Singleton<SpecialSongManager>.instance;

        /// <summary>
        /// Replaces the functionality of the vanilla PnlPreparation button.
        /// </summary>
        private static async Task OnPnlPreparationClick()
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

                if (LobbyManager.LocalLobby.HasInPlaylist(ChartManager.GetEntry(musicInfo,difficulty)))
                {
                    _ = LobbyManager.PlaylistRemove(musicInfo, difficulty);
                }
                else if (!LobbyManager.LocalLobby.IsPlaylistFull)
                {
                    _ = LobbyManager.PlaylistAdd(musicInfo, difficulty);
                }
            } else if (LobbyManager.LocalLobby.Locked)
            {
                PopupUtils.ShowInfo(Localization.Get("Lobby", "LobbyIsLocked"));
            }
        }

        /// <summary>
        /// Updates itself when it gets enabled.
        /// </summary>
        [HarmonyPatch(typeof(PnlPreparation), nameof(PnlPreparation.Awake))]
        internal static class PnlPreparationLock
        {
            // Don't add any other conditions here please :plead:
            private static void Postfix()
            {
                Button pnlPreparationButton = GameObject.Find("UI/Standerd/PnlPreparation/Start/BtnStart").GetComponent<Button>();
                pnlPreparationButton.onClick.RemoveAllListeners();
                pnlPreparationButton.onClick.AddListener((UnityAction)new Action(() => OnPnlPreparationClick()));

                UIManager.UpdatePnlPreparation();
            }
        }

        /// <summary>
        /// Updates itself when difficulty changes.
        /// </summary>
        [HarmonyPatch(typeof(PnlPreparation), nameof(PnlPreparation.OnDiffTglChanged))]
        internal static class PnlPreparationDiffChanged
        {
            private static void Postfix()
            {
                UIManager.UpdatePnlPreparation();
            }
        }

        /// <summary>
        /// Jumps to the current playlist entry of the chart and continues to battle start.
        /// </summary>
        [HarmonyPatch(typeof(PnlPreparation), nameof(PnlPreparation.OnBattleStart))]
        internal static class PnlPreparationOnBattleStart
        {
            private static bool Prefix()
            {
                if (!LobbyManager.IsInLobby) return true;

                bool startCondition = LobbyManager.IsInLobby && LobbyManager.LocalLobby.Locked;
                if (!startCondition) return startCondition;

                var entry = LobbyManager.LocalLobby.CurrentPlaylistEntry;

                UIManager.JumpToChart(entry.MusicInfo.uid);
                //TODO: Get back to diff 3 the hidden was unlocked for some reason
                if (entry.Difficulty == 4 && !SpecialSongManager.IsInvokeHideBms(entry.MusicInfo.uid))
                {
                    SpecialSongManager.InvokeHideBms(entry.MusicInfo, true);
                }
                GlobalDataBase.dbMusicTag.selectedDiffTglIndex = entry.Difficulty;
                GlobalDataBase.dbMusicTag.pnlSelectMusicUid = entry.MusicInfo.uid;

                return startCondition;
            }
        }
    }
}
