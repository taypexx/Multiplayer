using Multiplayer.Managers;
using Multiplayer.UI.Abstract;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI
{
    internal static class BulletinExtension
    {
        internal static BaseMultiplayerWindow CurrentWindow { get; set; }

        internal static Text WindowTitle { get; private set; }
        internal static Button WindowBackButton { get; private set; }
        internal static Button WindowRefreshButton { get; private set; }
        internal static Button WindowExitButton { get; private set; }

        /// <summary>
        /// Enables/disables the extension.
        /// </summary>
        internal static void Toggle(bool state)
        {
            WindowTitle.gameObject.SetActive(state);
            WindowBackButton.gameObject.SetActive(state);
            WindowRefreshButton.gameObject.SetActive(state);
            WindowExitButton.gameObject.SetActive(state);
        }

        private static Button CreateTopButton(Sprite sprite, Vector2 anchoredPosition, UnityAction action)
        {
            var bulletinImgBase = UIManager.MainFrame.transform.Find("ImgBase");

            var btnGo = new GameObject("Btn");

            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.parent = bulletinImgBase;
            btnRect.localScale = new(1f, 1f, 1f);
            btnRect.sizeDelta = new(70f, 70f);
            btnRect.pivot = new(0f, 1f);
            btnRect.anchorMin = btnRect.pivot;
            btnRect.anchorMax = btnRect.pivot;
            btnRect.anchoredPosition3D = anchoredPosition;

            var btnBase = btnGo.AddComponent<Image>();
            btnBase.color = new(0.298f, 0.1176f, 0.5216f, 1f);
            btnBase.sprite = bulletinImgBase.GetComponent<Image>().sprite;

            var btnIconGo = new GameObject("Icon");

            var btnIconRect = btnIconGo.AddComponent<RectTransform>();
            btnIconRect.parent = btnBase.transform;
            btnIconRect.localScale = btnRect.localScale;
            btnIconRect.sizeDelta = new(50f, 50f);
            btnIconRect.anchoredPosition3D = Vector3.zero;

            var btnIcon = btnIconGo.AddComponent<Image>();
            btnIcon.sprite = sprite;

            var btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(action);

            return btn;
        }

        internal static void Create()
        {
            var windowTitleGo = UIManager.MainFrame.transform.Find("TxtTittle").gameObject;
            windowTitleGo.name = "WindowTitle";
            windowTitleGo.SetActive(false);

            var titleRect = windowTitleGo.GetComponent<RectTransform>();
            titleRect.pivot = new(0.5f, 1f);
            titleRect.anchorMin = titleRect.pivot;
            titleRect.anchorMax = titleRect.pivot;
            titleRect.anchoredPosition = new(0f, -170f);

            WindowTitle = windowTitleGo.GetComponent<Text>();

            WindowBackButton = CreateTopButton(AssetManager.GetImageAsset("UI.Back.png").Sprite, new(40f, 40f), (UnityAction)new Action(() =>
            {
                if (CurrentWindow is null) return;
                CurrentWindow.OnReturn();
            }));

            WindowRefreshButton = CreateTopButton(AssetManager.GetImageAsset("UI.Refresh.png").Sprite, new(140f, 40f), (UnityAction)new Action(() =>
            {
                if (CurrentWindow is null) return;
                CurrentWindow.OnRefresh();
            }));

            WindowExitButton = CreateTopButton(AssetManager.GetImageAsset("UI.Cross.png").Sprite, new(1090f, 40f), (UnityAction)new Action(() =>
            {
                if (CurrentWindow is null) return;
                CurrentWindow.Window.ForceClose();
            }));
        }
    }
}
