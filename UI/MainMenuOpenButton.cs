using Il2CppAssets.Scripts.PeroTools.UI;
using Multiplayer.Data;
using Multiplayer.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.UI
{
    internal sealed class MainMenuOpenButton
    {
        private GameObject Button;
        private GameObject ButtonIcon;
        private Button ButtonComponent;

        private CustomImageAsset BaseAsset;
        private CustomImageAsset IconAsset;

        private static Vector3 ButtonOffset = new Vector3(-132f, 0f, 0f);
        private static Vector3 IconOffset = new Vector3(-12f, 0f, 0f);

        internal MainMenuOpenButton()
        {
            BaseAsset = AssetManager.GetImageAsset("UI.PcSprButton.png");
            IconAsset = AssetManager.GetImageAsset("UI.People.png");
            Create();
        }

        /// <summary>
        /// Creates a new button to open the main menu.
        /// </summary>
        internal void Create()
        {
            if (Button != null) return;

            Button = GameObject.Instantiate(
                GameObject.Find("UI/Standerd/PnlNavigation/Top/BtnOption"),
                GameObject.Find("UI/Standerd/PnlNavigation/Top").transform
            );
            Button.name = "BtnMultiplayerOpen";
            Button.transform.localPosition = Button.transform.localPosition + ButtonOffset;
            Button.GetComponent<Image>().sprite = BaseAsset.Sprite;
            Component.Destroy(Button.GetComponent<InputKeyBinding>());

            ButtonIcon = Button.transform.Find("ImgIcon").gameObject;
            ButtonIcon.transform.localPosition = ButtonIcon.transform.localPosition + IconOffset;
            ButtonIcon.GetComponent<Image>().sprite = IconAsset.Sprite;
            ButtonIcon.SetActive(true);

            ButtonComponent = Button.GetComponent<Button>();
            ButtonComponent.onClick.RemoveAllListeners();
            ButtonComponent.onClick.AddListener((UnityAction)new Action(UIManager.MainMenu.Open));

            Button.SetActive(true);
        }
    }
}
