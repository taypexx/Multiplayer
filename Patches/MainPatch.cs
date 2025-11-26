using CustomAlbums.Managers;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Multiplayer.Data.LobbyEnums;
using Multiplayer.Managers;
using Multiplayer.Static;
using PopupLib.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.Patches
{
    internal static class MainPatch
    {
        /// <summary>
        /// Replaces the functionality of the vanilla PnlPreparation button.
        /// </summary>
        private static void OnPnlPreparationClick()
        {
            if (!LobbyManager.IsInLobby)
            {
                UIManager.PnlPreparation.OnBattleStart();
            }
            else if (LobbyManager.CanChangePlaylist)
            {
                MusicInfo musicInfo = GlobalDataBase.dbMusicTag.CurMusicInfo();
                int difficulty = GlobalDataBase.dbMusicTag.selectedDiffTglIndex == 3 && Singleton<SpecialSongManager>.instance.IsInvokeHideBms(musicInfo.uid)
                    ? 4 : GlobalDataBase.dbMusicTag.selectedDiffTglIndex;

                if (LobbyManager.LocalLobby.PlayType == LobbyPlayType.VanillaOnly && musicInfo.albumIndex == AlbumManager.Uid)
                {
                    PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "VanillaOnly"));
                    return;
                } else if (LobbyManager.LocalLobby.PlayType == LobbyPlayType.CustomOnly && musicInfo.albumIndex != AlbumManager.Uid)
                {
                    PopupUtils.ShowInfo(Localization.Get("PnlPreparation", "CustomOnly"));
                    return;
                }

                if (LobbyManager.LocalLobby.HasInPlaylist(ChartManager.GetEntry(musicInfo,difficulty)))
                {
                    _ = LobbyManager.PlaylistRemove(musicInfo, difficulty);
                }
                else
                {
                    _ = LobbyManager.PlaylistAdd(musicInfo, difficulty);
                }
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
                pnlPreparationButton.onClick.AddListener((UnityAction)new Action(OnPnlPreparationClick));

                UIManager.UpdatePnlPreparation();
            }
        }

        /// <summary>
        /// Updates itself when it gets enabled.
        /// </summary>
        [HarmonyPatch(typeof(PnlPreparation), nameof(PnlPreparation.OnDiffTglChanged))]
        internal static class PnlPreparationDiffChanged
        {
            private static void Postfix()
            {
                UIManager.UpdatePnlPreparation();
            }
        }

        [HarmonyPatch(typeof(PnlPreparation), nameof(PnlPreparation.OnBattleStart))]
        internal static class PnlPreparationOnBattleStartBatch
        {
            private static bool Prefix()
            {
                bool start = !LobbyManager.IsInLobby || (LobbyManager.IsInLobby && LobbyManager.LocalLobby.Locked);
                if (!start) return start;

                var entry = LobbyManager.LocalLobby.CurrentPlaylistEntry;
                UIManager.PnlStage.SelectAllTagAndJumpToAssginIndex(entry.MusicInfo.uid);
                GlobalDataBase.dbMusicTag.selectedDiffTglIndex = entry.Difficulty;
                GlobalDataBase.dbMusicTag.pnlSelectMusicUid = entry.MusicInfo.uid;

                return start;
            }
        }
    }
}
