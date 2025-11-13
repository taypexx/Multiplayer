using LocalizeLib;
using Multiplayer.UI;
using PopupLib.UI;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.Managers
{
    internal static class UIManager
    {
        internal static bool Debounce = false;
        internal static Text WindowTitle { get; private set; }
        internal static GameObject MainFrame => GameObject.Find("UI/Forward/Tips/PnlBulletinNew");

        internal static MessageWindow Warning;
        internal static PromptWindow WarningChoose;
        internal static Action<bool?> WarningChooseAction;

        internal static MainMenu MainMenu { get; private set; }
        internal static MainMenuOpenButton MainMenuOpenButton { get; private set; }

        internal static ProfileWindow ProfileWindow { get; private set; }

        internal static FriendsWindow FriendsWindow { get; private set; }
        internal static AchievementsWindow AchievementsWindow { get; private set; }

        /// <summary>
        /// Displays a warning popup with the given <see cref="LocalString"/>.
        /// </summary>
        /// <param name="warning">Warning message.</param>
        internal static void WarnNotification(LocalString warning)
        {
            Warning.Text = warning;
            Warning.Show();
        }

        /// <summary>
        /// Displays a warning popup with the given <see cref="LocalString"/> and asks to continue or not.
        /// WarningChooseAction (<see cref="Action"/>) will be invoked once the local player clicks on either of options.
        /// </summary>
        /// <param name="warning">Warning message.</param>
        internal static void WarnChooseNotification(LocalString warning)
        {
            WarningChoose.Text = warning;
            WarningChoose.Show();
        }

        private static void WarningChooseCompletion(BaseWindow window)
        {
            if (WarningChooseAction is null) return;

            WarningChooseAction.Invoke(WarningChoose.Result);
            WarningChooseAction = null;
        }

        /// <summary>
        /// Initializes every time a UISystem_PC scene gets loaded.
        /// </summary>
        internal static void InitUISystemMain()
        {
            var windowTitleGo = GameObject.Instantiate(
                GameObject.Find("UI/Forward/Tips/PnlAchievementsTips/TxtTittle"),
                MainFrame.transform
            );
            windowTitleGo.transform.localPosition = new(0f, 330f, 0f);
            windowTitleGo.SetActive(false);
            WindowTitle = windowTitleGo.GetComponent<Text>();

            MainMenuOpenButton.Create();
            ProfileWindow.CreateAvatarBox();
        }

        internal static void Init()
        {
            if (MainMenu != null) return;

            Warning = new(new(), Localization.Get("Warning","Title"))
            {
                AutoReset = true
            };
            WarningChoose = new(new(), Localization.Get("Warning", "Title"))
            {
                AutoReset = true
            };
            WarningChoose.OnCompletion += WarningChooseCompletion;

            MainMenu = new();
            MainMenuOpenButton = new();

            ProfileWindow = new();

            FriendsWindow = new();
            AchievementsWindow = new();

            ProfileWindow.CreateButtons();
            MainMenu.CreateButtons();
        }
    }
}
