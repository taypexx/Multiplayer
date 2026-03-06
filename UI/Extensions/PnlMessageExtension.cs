using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Extensions
{
    internal static class PnlMessageExtension
    {
        private static PnlMessage PnlMessage;
        private static TimeSpan CellDelay = TimeSpan.FromMilliseconds(300);
        internal static bool Visible = PnlMessage != null && PnlMessage.gameObject.active;

        private static void AddEntry(string text, bool checkmarkIcon = true, Sprite icon = null, bool useTrophy = false)
        {
            var entry = GameObject.Instantiate(PnlMessage.achievement);
            entry.SetActive(true);

            var transform = entry.transform;
            transform.parent = PnlMessage.layout;

            transform.Find("TxtDescription").GetComponent<Text>().text = text;
            transform.Find("ImgCherkMark").gameObject.SetActive(checkmarkIcon);

            if (!useTrophy)
            {
                var img = transform.Find("Icon/ImgTrophy").GetComponent<Image>();
                Component.Destroy(img.GetComponent<Animator>());

                if (icon is null) 
                    img.gameObject.SetActive(false);
                else 
                    img.sprite = icon;
            }
        }

        internal static async Task AddOne(string text, bool checkmarkIcon = true, Sprite icon = null, bool useTrophy = false)
        {
            Main.Dispatch(() => AddEntry(text, checkmarkIcon, icon, useTrophy));
            await Task.Delay(CellDelay);
        }

        internal static async Task AddMultiple(string[] texts, bool checkmarkIcon = true, Sprite icon = null, bool useTrophy = false)
        {
            foreach (var text in texts)
            {
                await AddOne(text, checkmarkIcon, icon, useTrophy);
            }
        }

        internal static void Enable(bool clearVanillaMessages = false)
        {
            if (PnlMessage.gameObject.active) return;

            if (clearVanillaMessages)
            {
                for (int i = 0; i < PnlMessage.layout.childCount; i++)
                {
                    var vanillaMsg = PnlMessage.layout.GetChild(i);
                    GameObject.Destroy(vanillaMsg.gameObject);
                }
            }

            PnlMessage.gameObject.SetActive(true);
            PnlMessage.btnDone.gameObject.SetActive(true);
        }

        internal static void Create()
        {
            PnlMessage = UIManager.PnlMessage.GetComponent<PnlMessage>();
        }
    }
}
