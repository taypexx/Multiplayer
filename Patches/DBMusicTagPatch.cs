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
    }
}
