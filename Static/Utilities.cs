using Il2CppSirenix.Serialization.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Multiplayer.Static
{
    public static class Utilities
    {
        public static Font NormalFont = Addressables.LoadAssetAsync<Font>("Normal").WaitForCompletion();

        public static bool IsValidString(string str, int minLength, int maxLength)
        {
            if (str.IsNullOrWhitespace()) return false;
            return str.Length <= maxLength && str.Length >= minLength;
        }

        public static int? GetValidNumber(string num_, int? min = null, int? max = null)
        {
            if (num_.IsNullOrWhitespace()) return null;
            if (!Int32.TryParse(num_, out int num)) return null;
            if (min != null && max != null && (num < min || num > max)) return null;
            return num;
        }

        public static GameObject CreateText(Transform parent, string name = "Text")
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, true);

            Text text = gameObject.GetComponent<Text>() ?? gameObject.AddComponent<Text>();
            text.text = name;
            text.font = NormalFont;

            return gameObject;
        }
    }
}