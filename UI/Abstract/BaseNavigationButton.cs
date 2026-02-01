using Il2CppAssets.Scripts.PeroTools.UI;
using Multiplayer.Data;
using Multiplayer.Managers;
using Multiplayer.Static;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.UI.Abstract
{
    internal abstract class BaseNavigationButton
    {
        internal string ButtonName { get; private set; }
        internal bool IsRight { get; private set; }
        internal int Position { get; private set; }
        internal bool AlwaysVisible { get; set; } = false;
        internal UnityAction ButtonAction;
        
        private GameObject Button;
        private GameObject ButtonIcon;
        private Button ButtonComponent;
        private CustomImageAsset IconAsset;

        private static CustomImageAsset BaseAsset;
        private static readonly Vector3 IconOffset = new Vector3(-10f, 0f, 0f);
        private const float ButtonOffset = 132f;
        private const float LeftOffset = 192f;

        internal BaseNavigationButton(string iconFileName, int position = 1, bool isRight = true, string buttonName = "BtnMultiplayer")
        {
            ButtonName = buttonName;
            IsRight = isRight;
            Position = position;

            if (BaseAsset is null) BaseAsset = AssetManager.GetImageAsset("UI.Navigation.PcSprButton.png");
            IconAsset = AssetManager.GetImageAsset("UI.Navigation." + iconFileName);
        }

        /// <summary>
        /// Enables/disables the button.
        /// </summary>
        internal void Toggle(bool state)
        {
            if (Button is null) return;
            Button.SetActive(state || AlwaysVisible);
        }

        /// <summary>
        /// Creates the body of the button.
        /// </summary>
        internal void Create()
        {
            if (Button != null || ButtonAction is null) return;

            Button = GameObject.Instantiate(
                GameObject.Find("UI/Standerd/PnlNavigation/Top/BtnOption"),
                GameObject.Find("UI/Standerd/PnlNavigation/Top").transform
            );
            Button.name = ButtonName;

            var rect = Button.GetComponent<RectTransform>();
            rect.anchoredPosition = new
            (
                rect.anchoredPosition.x * (IsRight ? 1f : -1f) + (IsRight ? 0f : LeftOffset) + (ButtonOffset * (IsRight ? -1f : 1f) * Position),
                rect.anchoredPosition.y
            );

            var pivotDir = IsRight ? 1 : 0;
            rect.pivot = new(pivotDir, rect.pivot.y);
            rect.anchorMin = new(pivotDir, rect.anchorMin.y);
            rect.anchorMax = new(pivotDir, rect.anchorMax.y);

            rect.localScale = new
            (
                IsRight ? rect.localScale.x : -rect.localScale.x, 
                rect.localScale.y, 
                rect.localScale.z
            );

            Button.GetComponent<Image>().sprite = BaseAsset.Sprite;
            GameObject.Destroy(Button.GetComponent<InputKeyBinding>());

            ButtonIcon = Button.transform.Find("ImgIcon").gameObject;
            ButtonIcon.transform.localPosition = ButtonIcon.transform.localPosition + IconOffset;
            ButtonIcon.transform.localScale = new(IsRight ? ButtonIcon.transform.localScale.x : -ButtonIcon.transform.localScale.x, ButtonIcon.transform.localScale.y, ButtonIcon.transform.localScale.z);
            ButtonIcon.GetComponent<Image>().sprite = IconAsset.Sprite;
            ButtonIcon.SetActive(true);

            ButtonComponent = Button.GetComponent<Button>();
            ButtonComponent.onClick.RemoveAllListeners();
            ButtonComponent.onClick.AddListener(ButtonAction);

            Button.SetActive(Settings.Config.ShowNavigationButtons || AlwaysVisible);
        }
    }
}
