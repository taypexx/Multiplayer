using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppAssets.Scripts.PeroTools.Managers;

namespace Multiplayer.Managers
{
    internal static class SoundManager
    {
        private static AudioManager AudioManager;

        internal static void PlayClick()
        {
            if (AudioManager == null) return;
            AudioManager.PlayButtonClickedSfx(ClickSfxType.Click);
        }

        internal static void PlaySwitch()
        {
            if (AudioManager == null) return;
            AudioManager.PlayButtonClickedSfx(ClickSfxType.Switch);
        }

        internal static void PlayBack()
        {
            if (AudioManager == null) return;
            AudioManager.PlayButtonClickedSfx(ClickSfxType.Back);
        }

        internal static void PlayConfirm()
        {
            if (AudioManager == null) return;
            AudioManager.PlayButtonClickedSfx(ClickSfxType.Confirm);
        }

        internal static void Init()
        {
            AudioManager = Singleton<AudioManager>.instance;
        }
    }
}
