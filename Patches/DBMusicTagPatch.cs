using HarmonyLib;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Managers;
using Multiplayer.Static;
using PopupLib.UI;
using System.Reflection;

namespace Multiplayer.Patches
{
    internal static class DBMusicTagPatch
    {
        /// <summary>
        /// Prevents the local player from adding/removing charts to/from the hidden tab. Also synchronizes with the server when hiddens were updated.
        /// </summary>
        [HarmonyPatch]
        [HarmonyPriority(Priority.First)]
        internal static class UpdateHiddenPatch
        {
            private static readonly Type DB = typeof(DBMusicTag);
            private static readonly MethodBase[] PatchMethods = {
                DB.GetMethod(nameof(DBMusicTag.AddHide)),
                DB.GetMethod(nameof(DBMusicTag.AddHideMusicUid)),
                //DB.GetMethod(nameof(DBMusicTag.ClearHideMusicInfoAndReset)),
                DB.GetMethod(nameof(DBMusicTag.RemoveHide))
            };
            private static IEnumerable<MethodBase> TargetMethods() { return PatchMethods; }

            private static bool Prefix()
            {
                if (LobbyManager.IsInLobby) PopupUtils.ShowInfo(Localization.Get("Lobby", "HiddensLocked"));
                return !LobbyManager.IsInLobby;
            }

            private static void Postfix() { PlayerManager.SyncHiddens(); }
        }

        /// <summary>
        /// Tricks the game into thinking that the current selected chart is the first chart from the playlist.
        /// </summary>
        [HarmonyPatch(typeof(DBMusicTag), nameof(DBMusicTag.CurMusicInfo))]
        internal static class BattleStartMusicInfoPatch
        {
            private static bool Prefix(ref MusicInfo __result)
            {
                if (LobbyManager.IsPlaylistChartComingUp)
                {
                    __result = LobbyManager.LocalLobby.CurrentPlaylistEntry.MusicInfo;
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Always returns true (says that current master is unlocked) if this is a chart from the playlist.
        /// </summary>
        [HarmonyPatch]
        internal static class BattleStartMasterUnlockPatch
        {
            private static IEnumerable<MethodBase> TargetMethods()
            {
                return typeof(DataHelper).GetMethods().Where(m => m.Name == nameof(DataHelper.CheckMusicUnlockMaster));
            }

            private static bool Prefix(ref bool __result)
            {
                if (LobbyManager.IsPlaylistChartComingUp)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
    }
}
