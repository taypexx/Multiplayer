using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Extensions
{
    internal static class PnlMessageExtension
    {
        private static PnlMessage PnlMessage;
        private static bool Visible = PnlMessage != null && PnlMessage.gameObject.active;
        private static TimeSpan CellDelay = TimeSpan.FromMilliseconds(300);

        private static void AddEntry(string text, bool checkmarkIcon = true, Sprite icon = null)
        {
            var entry = GameObject.Instantiate(PnlMessage.achievement);
            entry.SetActive(true);

            var transform = entry.transform;
            transform.parent = PnlMessage.layout;

            transform.Find("TxtDescription").GetComponent<Text>().text = text;
            transform.Find("ImgCherkMark").gameObject.SetActive(checkmarkIcon);

            if (icon != null)
            {
                var img = transform.Find("Icon/ImgTrophy").GetComponent<Image>();
                Component.Destroy(img.GetComponent<Animator>());

                img.sprite = icon;
            }
        }

        internal static async Task AddOne(string text, bool checkmarkIcon = true, Sprite icon = null)
        {
            Main.Dispatch(() => AddEntry(text, checkmarkIcon, icon));
            await Task.Delay(CellDelay);
        }

        internal static async Task AddMultiple(string[] texts, bool checkmarkIcon = true, Sprite icon = null)
        {
            foreach (var text in texts)
            {
                await AddOne(text, checkmarkIcon, icon);
            }
        }

        internal static void Enable()
        {
            if (PnlMessage.gameObject.active) return;

            PnlMessage.gameObject.SetActive(true);
            PnlMessage.btnDone.gameObject.SetActive(true);
        }

        internal static void Create()
        {
            PnlMessage = UIManager.PnlMessage.GetComponent<PnlMessage>();
        }
    }
}
