using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;

namespace Multiplayer.Managers
{
    internal static class SoundManager
    {
        internal static bool BGMLocked { get; private set; } = false;

        internal static void PlayAndLockTroveBGM()
        {
            BGMLocked = true;
            Singleton<AudioManager>.instance.PlayBGM("TroveBgm-InARomanticMood-Lukyanov");
        }

        internal static void PlayAndLockAchievementsBGM()
        {
            BGMLocked = true;
            Singleton<AudioManager>.instance.PlayBGM("AchievementBgm-SweetVermouth-MusMus");
        }

        internal static void UnlockBGM()
        {
            BGMLocked = false;
        }

        internal static void PlayClick()
        {
            Singleton<AudioManager>.instance.PlayButtonClickedSfx(ClickSfxType.Click);
        }

        internal static void PlaySwitch()
        {
            Singleton<AudioManager>.instance.PlayButtonClickedSfx(ClickSfxType.Switch);
        }

        internal static void PlayBack()
        {
            Singleton<AudioManager>.instance.PlayButtonClickedSfx(ClickSfxType.Back);
        }

        internal static void PlayConfirm()
        {
            Singleton<AudioManager>.instance.PlayButtonClickedSfx(ClickSfxType.Confirm);
        }
    }
}
