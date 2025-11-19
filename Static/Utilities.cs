using Il2CppSirenix.Serialization.Utilities;

namespace Multiplayer.Static
{
    public static class Utilities
    {
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
    }
}