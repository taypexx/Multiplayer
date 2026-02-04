using Multiplayer.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI
{
    internal static class PnlAwait
    {
        private static GameObject AwaitingLabelRef => GameObject.Find("Forward/PnlReady/PnlReadyGo/Ready");
        private static Transform ParentRef => GameObject.Find("Forward")?.transform;

        private static GameObject PnlGameObject;

        internal static void Create()
        {
            if (AwaitingLabelRef is null || ParentRef is null) return;
            PnlGameObject = GameObject.Instantiate(AwaitingLabelRef, ParentRef);
            PnlGameObject.name = "PnlAwait";

            var origPos = PnlGameObject.transform.localPosition;
            PnlGameObject.transform.localPosition = new(origPos.x,-50f,origPos.z);

            for (int i = 0; i < PnlGameObject.transform.childCount; i++)
            {
                Transform t = PnlGameObject.transform.GetChild(i);
                GameObject gameObject = t.gameObject;

                if (gameObject.name == "ImgA")
                {
                    gameObject.name = "ImgMain";
                    gameObject.GetComponent<RectTransform>().sizeDelta = new(650f,250f);

                    var img = gameObject.GetComponent<Image>();
                    img.preserveAspect = true;
                    img.sprite = AssetManager.GetImageAsset("UI.Awaiting.png").Sprite;
                }
                else GameObject.Destroy(gameObject);
            }

            PnlGameObject.SetActive(true);
        }

        internal static void Destroy()
        {
            GameObject.Destroy(PnlGameObject);
        }
    }
}
