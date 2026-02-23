using Il2CppAssets.Scripts.PeroTools.Nice.Events;
using Multiplayer.Managers;
using Multiplayer.Static;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.UI.Extensions
{
    internal static class SideNotification
    {
        internal static bool Visible { get; private set; } = false;

        private static GameObject Notification;
        private static GameObject ImgBase;

        private static GameObject ButtonMain;
        private static Text ButtonMainText;
        private static GameObject ButtonSecondary;
        private static Text ButtonSecondaryText;

        private static Text Message;
        private static OnActivate CloseObject;

        internal static void Popup(string content, Tuple<string, Color, Color, Action> buttonMain = null, Tuple<string, Color, Color, Action> buttonSecondary = null) 
        {
            if (Visible) Close();

            Notification.SetActive(true);
            Message.text = content;

            ButtonMain.SetActive(buttonMain != null);
            if (ButtonMain.active)
            {
                ButtonMainText.text = buttonMain.Item1;
                ButtonMainText.color = buttonMain.Item2;
                ButtonMain.GetComponent<Image>().color = buttonMain.Item3;

                var button = ButtonMain.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener((UnityAction)buttonMain.Item4);
            }

            ButtonSecondary.SetActive(buttonSecondary != null);
            if (ButtonSecondary.active)
            {
                ButtonSecondaryText.text = buttonSecondary.Item1;
                ButtonSecondaryText.color = buttonSecondary.Item2;
                ButtonSecondary.GetComponent<Image>().color = buttonSecondary.Item3;

                var button = ButtonSecondary.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener((UnityAction)buttonSecondary.Item4);
            }

            Visible = true;
        }

        internal static void Update(string newContent = null, Tuple<string, Color, Color> newButtonMain = null, Tuple<string, Color, Color> newButtonSecondary = null)
        {
            if (!Visible) return;

            if (newButtonMain != null)
            {
                ButtonMainText.text = newButtonMain.Item1;
                ButtonMainText.color = newButtonMain.Item2;
                ButtonMain.GetComponent<Image>().color = newButtonMain.Item3;
            }

            if (newButtonSecondary != null)
            {
                ButtonSecondaryText.text = newButtonSecondary.Item1;
                ButtonSecondaryText.color = newButtonSecondary.Item2;
                ButtonSecondary.GetComponent<Image>().color = newButtonSecondary.Item3;
            }

            if (newContent != null) Message.text = newContent;
        }

        internal static void Close()
        {
            if (!Visible) return;

            CloseObject.OnEnable();

            Visible = false;
        }

        internal static void Create()
        {
            Notification = GameObject.Instantiate(UIManager.PnlCloudMessage, UIManager.PnlCloudMessage.transform.parent);
            Notification.name = "MultiplayerSideNotification";
            Notification.SetActive(false);
            Notification.AddComponent<GraphicRaycaster>();

            ImgBase = Notification.transform.Find("ImgBase").gameObject;

            var baseRect = ImgBase.GetComponent<RectTransform>();
            baseRect.anchoredPosition = new(0f, 150f);
            baseRect.sizeDelta = new Vector2(350f, 300f);

            var messageBase = ImgBase.transform.Find("Synchronizing").GetComponent<RectTransform>();
            messageBase.pivot = new(0.5f, 1f);
            messageBase.anchorMin = messageBase.pivot;
            messageBase.anchorMax = messageBase.pivot;
            messageBase.gameObject.SetActive(true);

            Message = messageBase.Find("TxtSynchronizing").GetComponent<Text>();
            Message.alignment = TextAnchor.UpperRight;
            Message.verticalOverflow = VerticalWrapMode.Overflow; // 4 lines max

            var sprRoundedSquare = Addressables.LoadAssetAsync<Sprite>("SprRoundedsquare").WaitForCompletion();//GameObject.Find("UI/Standerd/PnlMenu/Panels/PnlRole/Properties/BtnApply/ImgInUse").GetComponent<Image>().sprite;
            ButtonMain = new GameObject("BtnMain");

            var buttonMainRect = ButtonMain.AddComponent<RectTransform>();
            buttonMainRect.parent = ImgBase.transform;
            buttonMainRect.anchoredPosition3D = new(-10f, 10f, 0f);
            buttonMainRect.pivot = new(1f, 0f);
            buttonMainRect.anchorMin = buttonMainRect.pivot;
            buttonMainRect.anchorMax = buttonMainRect.pivot;
            buttonMainRect.localScale = Vector3.one;
            buttonMainRect.sizeDelta = new(120f, 60f);

            var buttonMainImg = ButtonMain.AddComponent<Image>();
            buttonMainImg.sprite = sprRoundedSquare;
            buttonMainImg.type = Image.Type.Tiled;

            ButtonMain.AddComponent<Button>();

            var buttonMainTextGo = new GameObject("BtnTxt");

            var buttonMainTextRect = buttonMainTextGo.AddComponent<RectTransform>();
            buttonMainTextRect.parent = buttonMainRect.transform;
            buttonMainTextRect.anchoredPosition3D = Vector3.zero;
            buttonMainTextRect.localScale = Vector3.one;
            buttonMainTextRect.sizeDelta = buttonMainTextRect.sizeDelta;

            ButtonMainText = buttonMainTextGo.AddComponent<Text>();
            ButtonMainText.alignment = TextAnchor.MiddleCenter;
            ButtonMainText.text = "Button";
            ButtonMainText.fontSize = 28;
            ButtonMainText.horizontalOverflow = HorizontalWrapMode.Overflow;
            ButtonMainText.font = Utilities.NormalFont;

            ButtonSecondary = GameObject.Instantiate(ButtonMain, ButtonMain.transform.parent);
            ButtonSecondary.name = "BtnSecondary";
            ButtonSecondary.GetComponent<RectTransform>().anchoredPosition3D = new(-150f, 10f);

            ButtonSecondaryText = ButtonSecondary.transform.Find("BtnTxt").GetComponent<Text>();

            CloseObject = ImgBase.transform.Find("SynchronizingCompleted").GetComponent<OnActivate>();
            CloseObject.enabled = true;

            GameObject.Destroy(ImgBase.transform.Find("Synchronizing/TxtSynchronizing/ImgSynchronizing").gameObject);
            GameObject.Destroy(ImgBase.transform.Find("SynchronizingFail").gameObject);

            Component.Destroy(messageBase.GetComponent<OnCustomEvent>());
            Component.Destroy(CloseObject.GetComponent<OnCustomEvent>());
        }
    }
}
