using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Il2CppSirenix.Serialization.Utilities;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Multiplayer.Static
{
    public static class Utilities
    {
        public static Font NormalFont = Addressables.LoadAssetAsync<Font>("Normal").WaitForCompletion();

        /// <summary>
        /// Gets the name of a girl with the given <paramref name="girlId"/>.
        /// </summary>
        internal static string GetGirl(int girlId)
        {
            if (girlId < 0) return string.Empty;

            var configManager = Singleton<Il2CppAssets.Scripts.PeroTools.Managers.ConfigManager>.instance;
            var character = configManager.GetJson("character", true)[girlId];

            var characterType = configManager.GetConfigObject<DBConfigCharacter>()
                .GetCharacterInfoByIndex(girlId)
                .characterType;

            return string.Equals(characterType, "Special")
                ? character["characterName"].ToString()
                : character["cosName"].ToString();
        }

        /// <summary>
        /// Gets the name of a elfin with the given <paramref name="elfinId"/>.
        /// </summary>
        internal static string GetElfin(int elfinId)
        {
            if (elfinId < 0) return string.Empty;

            return Singleton<Il2CppAssets.Scripts.PeroTools.Managers.ConfigManager>.instance.GetJson("elfin", true)[elfinId]["name"].ToString();
        }

        /// <summary>
        /// Rounds the <paramref name="value"/> to 2 decimal places.
        /// </summary>
        public static float RoundFloat(float value)
        {
            if (float.IsNaN(value)) return 0f;
            return (float)Math.Round((decimal)(value * 100)) / 100;
        }

        /// <summary>
        /// Checks if the <paramref name="str"/> is within the given bounds.
        /// </summary>
        public static bool IsValidString(string str, int minLength, int maxLength)
        {
            if (str.IsNullOrWhitespace()) return false;
            return str.Length <= maxLength && str.Length >= minLength;
        }

        /// <summary>
        /// Checks if the <paramref name="num"/> is within the given bounds.
        /// </summary>
        public static int? GetValidNumber(string num, int? min = null, int? max = null)
        {
            if (num.IsNullOrWhitespace()) return null;
            if (!Int32.TryParse(num, out int num_)) return null;
            if (min != null && max != null && (num_ < min || num_ > max)) return null;
            return num_;
        }

        /// <summary>
        /// Creates a new <see cref="GameObject"/> with the <see cref="Text"/> component.
        /// </summary>
        public static GameObject CreateText(Transform parent, string name = "Text", bool addShadow = false)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, true);
            gameObject.transform.localScale = Vector3.one;

            var text = gameObject.AddComponent<Text>();
            text.text = name;
            text.font = NormalFont;

            if (addShadow)
            {
                Shadow shadow = gameObject.AddComponent<Shadow>();
                shadow.effectDistance = new(2f, -2f);
                shadow.effectColor = new(0f, 0f, 0f, 0.3f);
            }

            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchoredPosition3D = Vector3.zero;

            return gameObject;
        }

        /// <summary>
        /// Opens a <paramref name="url"/> in the default browser.
        /// </summary>
        public static void OpenBrowserLink(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { }
        }

        /// <returns>The hex color code of the given <paramref name="pingMS"/>.</returns>
        public static string GetPingColor(ushort pingMS)
        {
            foreach ((var level, var color) in Constants.PingColors)
            {
                if (pingMS < level) return color;
            }
            return Constants.PingColors.LastOrDefault().Value;
        }
    }
}