using LocalizeLib;
using Multiplayer.Managers;
using System.Text.Json;

namespace Multiplayer.Static
{
    internal static class Localization
    {
        private static Dictionary<string, Dictionary<string, LocalString>> Strings = new();
        private static LocalString Empty = new();
        private static List<string> Languages = new() { "English", "ChineseS", "ChineseT", "Japanese", "Korean" };
        internal static int LanguageIndex => Languages.IndexOf(Utils.GetLangString()) + 1;

        /// <summary>
        /// Returns a <see cref="LocalString"/> of the given <paramref name="category"/> and <paramref name="key"/> or an empty <see cref="LocalString"/> if not found.
        /// </summary>
        /// <returns>A <see cref="LocalString"/> reference.</returns>
        internal static LocalString Get(string category, string key)
        {
            if (!Strings.TryGetValue(category, out var dic)) return Empty;
            if (!dic.TryGetValue(key, out var localstr)) return Empty;
            return localstr;
        }

        internal static void Init()
        {
            foreach (string language in Languages)
            {
                string localizationJson = AssetManager.GetStringAsset($"Localization.{language}.json");
                if (localizationJson is null)
                {
                    Main.Log($"Failed to load {language} localization!", Main.LogType.Error);
                    continue;
                }

                foreach ((string category, var dic) in JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(localizationJson))
                {
                    if (!Strings.ContainsKey(category)) Strings.Add(category, new());

                    foreach ((string key, string text) in dic)
                    {
                        switch (Languages.IndexOf(language))
                        {
                            case 0:
                                Strings[category][key] = new(text);
                                break;
                            case 1:
                                Strings[category][key].ChineseSimplified = text;
                                break;
                            case 2:
                                Strings[category][key].ChineseTraditional = text;
                                break;
                            case 3:
                                Strings[category][key].Japanese = text;
                                break;
                            case 4:
                                Strings[category][key].Korean = text;
                                break;
                        }
                    }
                }
            }
        }
    }
}
