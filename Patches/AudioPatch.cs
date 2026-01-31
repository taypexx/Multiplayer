using HarmonyLib;
using Il2CppAssets.Scripts.PeroTools.Managers;
using Multiplayer.Managers;
using System.Reflection;

namespace Multiplayer.Patches
{
    /// <summary>
    /// Doesn't let other BGM override the current one.
    /// </summary>
    [HarmonyPatch]
    [HarmonyPriority(Priority.First)]
    internal static class AudioPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(AudioManager).GetMethods().Where(m => m.Name == nameof(AudioManager.PlayBGM));
        }

        private static bool Prefix()
        {
            return !SoundManager.BGMLocked;
        }
    }
}
