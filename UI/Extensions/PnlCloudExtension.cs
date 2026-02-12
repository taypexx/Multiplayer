using Il2CppAssets.Scripts.PeroTools.Nice.Events;
using Multiplayer.Managers;
using Multiplayer.Static;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Extensions
{
    internal static class PnlCloudExtension
    {
        private static GameObject Message;
        private static GameObject ImgBase;
        private static GameObject PendingMsg;
        private static GameObject CompletedMsg;
        private static GameObject FailMsg;

        private static Text PendingText;

        internal static void StartIntermission()
        {
            if (!Main.IsUIScene || !UIManager.Initialized) return;

            var baseRect = ImgBase.GetComponent<RectTransform>();
            baseRect.anchoredPosition = new(0f, 250f);
            baseRect.sizeDelta = new Vector2(350f, 250f);

            var main = Utilities.CreateText(baseRect, "MainTxt");
            var mainRect = main.GetComponent<RectTransform>();

            mainRect.pivot = Vector2.one;
            mainRect.anchorMin = mainRect.pivot;
            mainRect.anchorMax = mainRect.pivot;

            mainRect.anchoredPosition = new(-20f, 0f);
            mainRect.sizeDelta = new();

            var mainText = main.GetComponent<Text>();
            mainText.fontSize = 32;
            mainText.alignment = TextAnchor.UpperRight;
            
            // TODO: finish
        }

        /// <summary>
        /// Enables the <see cref="PnlCloudMessage"/> so it sits there with the "Synchronizing" label.
        /// </summary>
        /// <param name="text">Text to display.</param>
        internal static void Start(string text = null)
        {
            if (!Main.IsUIScene || !UIManager.Initialized || Message.active) return;

            if (text == null) text = Localization.Get("PnlCloudMessage", "Default").ToString();
            PendingText.text = text;

            CompletedMsg.SetActive(false);
            FailMsg.SetActive(false);

            PendingMsg.SetActive(true);
            Message.SetActive(true);
        }

        /// <summary>
        /// Finishes the <see cref="PnlCloudMessage"/> animation.
        /// </summary>
        /// <param name="success">Will display completed or failed.</param>
        internal static void Finish(bool success)
        {
            if (!Main.IsUIScene || !UIManager.Initialized || !Message.active) return;

            PendingMsg.gameObject.SetActive(false);
            (success ? CompletedMsg : FailMsg).SetActive(true);
        }

        internal static void Create()
        {
            Message = GameObject.Instantiate(UIManager.PnlCloudMessage,UIManager.PnlCloudMessage.transform.parent);
            Message.name = "PnlCloudMessageMultiplayer";
            Message.SetActive(false);

            ImgBase = Message.transform.Find("ImgBase").gameObject;

            PendingMsg = ImgBase.transform.Find("Synchronizing").gameObject;
            CompletedMsg = ImgBase.transform.Find("SynchronizingCompleted").gameObject;
            FailMsg = ImgBase.transform.Find("SynchronizingFail").gameObject;

            PendingText = PendingMsg.transform.Find("TxtSynchronizing").GetComponent<Text>();
            Component.Destroy(PendingText.GetComponent<Il2CppAssets.Scripts.PeroTools.GeneralLocalization.Localization>());
            Component.Destroy(PendingMsg.GetComponent<OnCustomEvent>());
            CompletedMsg.GetComponent<OnActivate>().enabled = true;
            FailMsg.GetComponent<OnActivate>().enabled = true;
        }
    }
}
