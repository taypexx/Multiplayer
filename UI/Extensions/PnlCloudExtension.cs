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
        private static GameObject PendingMsg;
        private static GameObject CompletedMsg;
        private static GameObject FailMsg;

        private static Text PendingText;

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
            Message.SetActive(false);

            PendingMsg = Message.transform.Find("ImgBase/Synchronizing").gameObject;
            CompletedMsg = Message.transform.Find("ImgBase/SynchronizingCompleted").gameObject;
            FailMsg = Message.transform.Find("ImgBase/SynchronizingFail").gameObject;

            PendingText = PendingMsg.transform.Find("TxtSynchronizing").GetComponent<Text>();
            Component.Destroy(PendingText.GetComponent<Il2CppAssets.Scripts.PeroTools.GeneralLocalization.Localization>());
            Component.Destroy(PendingMsg.GetComponent<OnCustomEvent>());
            CompletedMsg.GetComponent<OnActivate>().enabled = true;
            FailMsg.GetComponent<OnActivate>().enabled = true;
        }
    }
}
