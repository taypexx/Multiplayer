using LocalizeLib;
using MelonLoader;
using Multiplayer.Static;

namespace Multiplayer.Data.Settings
{
    public class Setting<T> : ISetting
    {
        public string Name { get; set; }
        public LocalString LocalName { get; set; }
        public LocalString Description { get; set; }
        public SettingCategory Category { get; set; }

        public readonly Func<T,T>? ValueProcessor;
        public readonly Action<T>? ApplyAction;

        public T Value { 
            get; internal set
            {
                field = ValueProcessor is null ? value : ValueProcessor.Invoke(value);
                Save();
                ApplyAction?.Invoke(field);
            } 
        }

        internal MelonPreferences_Entry<T> MelonPreferencesEntry;

        public void Load()
        {
            Value = MelonPreferencesEntry.Value;
        }

        public void Save()
        {
            MelonPreferencesEntry.Value = Value;
            MelonPreferencesEntry.Save();
            MelonPreferencesEntry.Category.SaveToFile();
        }

        /// <summary>
        /// Creates a new <see cref="Setting"/>.
        /// </summary>
        /// <param name="name">Name of the <see cref="Setting"/> without spaces.</param>
        /// <param name="category">A <see cref="SettingCategory"/> that this <see cref="Setting"/> will be assigned to.</param>
        /// <param name="defaultValue">Default value to be applied upon creation.</param>
        /// <param name="valueProcessor"><see cref="Func"/> that modifies (clamps) the value of the user before it gets applied.</param>
        /// <param name="applyAction"><see cref="Action"/> to be executed after the value is applied.</param>
        internal Setting(string name, SettingCategory category, T defaultValue, Func<T,T>? valueProcessor = null, Action<T>? applyAction = null)
        {
            MelonPreferencesEntry = Static.Settings.MelonCategory.CreateEntry(name, defaultValue);

            Name = name;
            LocalName = new(name);
            Description = Localization.Get("SettingsWindow", name);
            Category = category;
            Value = defaultValue;
            ApplyAction = applyAction;
        }    
    }
}
