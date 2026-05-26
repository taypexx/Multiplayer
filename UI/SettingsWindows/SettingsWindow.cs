using LocalizeLib;
using Multiplayer.Data.Settings;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Interfaces;
using UnityEngine;

namespace Multiplayer.UI.SettingsWindows
{
    internal class SettingsWindow : BaseMultiplayerWindow
    {
        internal SettingCategory Category { get; private set; }
        private LocalString MainDescription;

        private PromptWindow PromptWindow;
        private InputWindow InputWindow;

        private ISetting CurrentSetting;
        private Dictionary<ForumObject, ISetting> ButtonsSettings;

        internal SettingsWindow(SettingCategory category) : base(Localization.Get("SettingsWindow", category.ToString() + "Category"), UIManager.SettingsWindow, "Settings.png")
        {
            Category = category;

            ButtonsSettings = new();

            PromptWindow = new(new());
            PromptWindow.AutoReset = true;
            PromptWindow.OnCompletion += (window) => OnPromptCompleted();

            InputWindow = new();
            InputWindow.AutoReset = true;
            InputWindow.OnCompletion += (window) => OnInputCompleted();
        }

        internal void CreateButtons()
        {
            foreach (var setting in Settings.Config.Where(s => s.Category == Category))
            {
                var button = AddButton(new(setting.Name), setting is Setting<bool> ? PromptWindow : InputWindow);
                ButtonsSettings.Add(button, setting);
            }
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            string desc = string.Empty;

            foreach (var setting in Settings.Config.Where(s => s.Category == Category))
            {
                string valueString;
                switch (setting)
                {
                    case Setting<bool>:
                        valueString = ((Setting<bool>)setting).Value
                          ? $"<color={Constants.Green}>{Localization.Get("Global", "Yes").ToString()}</color>"
                          : $"<color={Constants.Red}>{Localization.Get("Global", "No").ToString()}</color>";
                        break;
                    case Setting<string>:
                        var value = ((Setting<string>)setting).Value;
                        bool isColor = setting.Name.Contains("color", StringComparison.InvariantCultureIgnoreCase);

                        valueString = $"<color={(isColor ? value : Constants.Yellow)}>{value}</color>";
                        break;
                    default:
                        valueString = $"<color={Constants.Yellow}>{((Setting<int>)setting).Value}</color>";
                        break;
                }

                desc = desc + $"[ <u>{setting.Name}</u> ]: {valueString}\n{setting.Description.ToString()}\n\n";
            }

            MainDescription = new(desc);
            foreach (ForumObject button in Window.ForumObjects)
            {
                button.Contents = MainDescription;
            }
        }

        private void OnPromptCompleted()
        {
            if (PromptWindow.Result != null && CurrentSetting != null && CurrentSetting is Setting<bool>)
            {
                try
                {
                    ((Setting<bool>)CurrentSetting).Value = (bool)PromptWindow.Result;
                    UpdateDescription();
                }
                catch {}
                CurrentSetting = null;
            }

            Window.Show();
        }

        private void OnInputCompleted()
        {
            if (InputWindow.Result != null && CurrentSetting != null && (CurrentSetting is Setting<int> || CurrentSetting is Setting<string>))
            {
                try
                {
                    string input = InputWindow.Result;
                    if (CurrentSetting is Setting<int> && int.TryParse(input, out int parsed))
                    {
                        ((Setting<int>)CurrentSetting).Value = parsed;
                    }
                    else if (CurrentSetting is Setting<string>)
                    {
                        ((Setting<string>)CurrentSetting).Value = input;
                    }
                    UpdateDescription();
                }
                catch {}
                CurrentSetting = null;
            }

            Window.Show();
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            ForumObject button = Window.ForumObjects[objectIndex];

            if (ButtonsSettings.TryGetValue(button, out CurrentSetting))
            {
                switch (CurrentSetting)
                {
                    case Setting<bool>:
                        PromptWindow.Title = CurrentSetting.LocalName;
                        PromptWindow.Text = CurrentSetting.Description;
                        break;
                    default:
                        InputWindow.Title = CurrentSetting.Description;
                        break;
                }
            }

            base.OnButtonClick(window, objectIndex);
        }
    }
}
