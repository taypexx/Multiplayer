using Multiplayer.Data.Settings;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows.Interfaces;
using System.Diagnostics;

namespace Multiplayer.UI.SettingsWindows
{
    internal sealed class SettingsCategoriesWindow : BaseMultiplayerWindow
    {
        private HashSet<SettingsWindow> Categories;
        private ForumObject OpenConfigButton;
        private ForumObject ReloadConfigButton;

        internal SettingsCategoriesWindow() : base(Localization.Get("SettingsWindow", "Title"), UIManager.MainMenu, "Settings.png")
        {
            Categories = new();
        }

        internal void CreateButtons()
        {
            foreach (var category in Enum.GetValues<SettingCategory>())
            {
                var window = new SettingsWindow(category);
                window.CreateButtons();

                Categories.Add(window);
                AddButton(window.Title, window);
            }

            OpenConfigButton = AddButton(Localization.Get("SettingsWindow", "OpenConfig"));
            ReloadConfigButton = AddButton(Localization.Get("SettingsWindow", "ReloadConfig"));
        }

        protected override void OnButtonClick(IListWindow _, int objectIndex)
        {
            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == OpenConfigButton)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Settings.ConfigPath,
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            else if (button == ReloadConfigButton)
            {
                Settings.Load();
                OnRefresh();
                return;
            }

            base.OnButtonClick(_, objectIndex);
        }
    }
}
