using Il2Cpp;
using Il2CppAssets.Scripts.UI.Panels.Menu;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.UI
{
    internal class MainMenuOpenButton
    {
        internal GameObject Button;
        internal Button ButtonComponent;

        internal MainMenuOpenButton()
        {
            Init();
        }

        internal void Init()
        {
            if (Button != null) { return; }

            var pnlNavigation = GameObject.Find("UI/Standerd/PnlNavigation");

            Button = GameObject.Instantiate(
                GameObject.Find("UI/Standerd/PnlMenu/Panels/PnlOption/Toggles/Account"),
                pnlNavigation.transform
            );
            Button.name = "MultiplayerMenuOpen";

            var tglAccount = Button.transform.Find("TglAccount");
            Button.transform.Find("ImgSelected").gameObject.SetActive(false);
            tglAccount.Find("ImgAccount").gameObject.SetActive(false);
            tglAccount.Find("ImgAccountClose").gameObject.SetActive(true);

            Component.Destroy(tglAccount.Find("TxtAccount").gameObject.GetComponent<Il2CppAssets.Scripts.PeroTools.GeneralLocalization.Localization>());
            Component.Destroy(Button.GetComponent<PnlMenuAccount>());
            Component.Destroy(Button.GetComponent<ButtonPointerEnter>());
            
            Button.transform.localScale = new(0.5f, 0.5f, 0.5f);
            Button.transform.localPosition = new(870f, 390f, 0f);

            var buttonText = tglAccount.transform.Find("TxtAccount").GetComponent<Text>();
            buttonText.text = Localization.Get("MainMenu", "Open").ToString();
            buttonText.color = new(0.75f, 0.53f, 1f, 1f);

            tglAccount.GetComponent<Image>().color = new(0.3f, 1f, 1f, 1f);

            ButtonComponent = Button.GetComponent<Button>();
            ButtonComponent.onClick.RemoveAllListeners();
        }
    }
}
