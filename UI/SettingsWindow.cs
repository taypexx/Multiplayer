using LocalizeLib;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Interfaces;
using System.Reflection;

namespace Multiplayer.UI
{
    internal sealed class SettingsWindow : BaseMultiplayerWindow
    {
        private PromptWindow PromptWindow;
        private InputWindow InputWindow;

        private PropertyInfo[] ConfigProperties;
        private Dictionary<ForumObject, PropertyInfo> ButtonsProperties;
        private PropertyInfo CurrentPropertyInfo;

        private LocalString MainDescription;

        internal SettingsWindow() : base(Localization.Get("SettingsWindow", "Title"), UIManager.MainMenu, "Settings.png")
        {
            ConfigProperties = typeof(Config).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
            ButtonsProperties = new();

            PromptWindow = new(new());
            PromptWindow.AutoReset = true;
            PromptWindow.OnCompletion += (window) => OnPromptCompleted();

            InputWindow = new();
            InputWindow.AutoReset = true;
            InputWindow.OnCompletion += (window) => OnInputCompleted();
        }

        internal void CreateButtons()
        {
            foreach (PropertyInfo prop in ConfigProperties)
            {
                ForumObject button = AddButton(new(prop.Name), prop.PropertyType == typeof(bool) ? PromptWindow : InputWindow);
                ButtonsProperties.Add(button, prop);
            }
            AddReturnButton();
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            string desc = string.Empty;

            foreach (PropertyInfo prop in ConfigProperties)
            {
                string valueString = prop.PropertyType == typeof(bool)
                    ? (bool)prop.GetValue(Settings.Config) 
                          ? $"<color={Constants.Green}>{Localization.Get("Global", "Yes").ToString()}</color>"
                          : $"<color={Constants.Red}>{Localization.Get("Global", "No").ToString()}</color>"
                    : $"<color={Constants.Yellow}>{prop.GetValue(Settings.Config).ToString()}</color>";

                desc = desc + $"[ <u>{prop.Name}</u> ]: {valueString}\n{Localization.Get("SettingsWindow", prop.Name).ToString()}\n\n";
            }

            MainDescription = new(desc);
            foreach (ForumObject button in Window.ForumObjects)
            {
                button.Contents = MainDescription;
            }
        }

        internal void OnPromptCompleted()
        {
            if (PromptWindow.Result != null && CurrentPropertyInfo != null)
            {
                try
                {
                    CurrentPropertyInfo.SetValue(Settings.Config, PromptWindow.Result);
                    UpdateDescription();
                }
                catch { return; }
                CurrentPropertyInfo = null;
            }

            Window.Show();
        }

        internal void OnInputCompleted()
        {
            if (InputWindow.Result != null && CurrentPropertyInfo != null)
            {
                try
                {
                    object value = CurrentPropertyInfo.PropertyType == typeof(int) ? int.Parse(InputWindow.Result) : InputWindow.Result;
                    CurrentPropertyInfo.SetValue(Settings.Config, value);
                    UpdateDescription();
                }
                catch { return; }
                CurrentPropertyInfo = null;
            }

            Window.Show();
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            ForumObject button = Window.ForumObjects[objectIndex];

            if (ButtonsProperties.TryGetValue(button, out CurrentPropertyInfo))
            {
                if (CurrentPropertyInfo.PropertyType == typeof(bool))
                {
                    PromptWindow.Title = new(CurrentPropertyInfo.Name);
                    PromptWindow.Text = Localization.Get("SettingsWindow", CurrentPropertyInfo.Name);
                }
                else InputWindow.Title = Localization.Get("SettingsWindow", CurrentPropertyInfo.Name);
            }

            base.OnButtonClick(window, objectIndex);
        }
    }
}
