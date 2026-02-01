using HarmonyLib;
using LocalizeLib;
using Il2CppAssets.Scripts.UI.Controls;
using Multiplayer.Managers;

namespace Multiplayer.Patches
{
    /// <summary>
    /// Removes "Code doesn't exist" popup.
    /// </summary>
    [HarmonyPatch(typeof(ShowText), nameof(ShowText.ShowInfo))]
    [HarmonyPriority(Priority.First)]
    internal static class ShowTextPatch
    {
        // TODO: get the local string the right way
        private static LocalString NotFoundMessage = new()
        {
            English = "Redeem Code doesn't exist（T^T）",
            ChineseSimplified = "兑换码不存在哦（T^T）",
            ChineseTraditional = "兌換碼不存在哦（T^T）",
            Japanese = "コードが存在しません。（T^T）",
            Korean = "존재하지 않는 교환 코드에요.（T^T）"
        };

        private static bool Prefix(string info)
        {
            return info != NotFoundMessage.ToString() || !UIManager.Initialized;
        }
    }
}
