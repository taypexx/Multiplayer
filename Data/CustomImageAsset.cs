using UnityEngine;

namespace Multiplayer.Data
{
    public class CustomImageAsset
    {
        internal Texture2D Texture { get; private set; }
        internal Sprite Sprite { get; private set; }

        private void Init()
        {
            if (Sprite != null || Texture == null) return;
            Sprite = Sprite.Create(Texture, new Rect(0, 0, Texture.width, Texture.height), new Vector2(0.5f, 0.5f));

            UnityEngine.Object.DontDestroyOnLoad(Texture);
            UnityEngine.Object.DontDestroyOnLoad(Sprite);
        }

        internal CustomImageAsset(byte[] bytes)
        {
            Texture = new Texture2D(2, 2, TextureFormat.ARGB32, false)
            {
                wrapMode = TextureWrapMode.MirrorOnce
            };
            ImageConversion.LoadImage(Texture, bytes);
            Init();
        }

        internal CustomImageAsset(Texture2D texture2D)
        {
            Texture = texture2D;
            Init();
        }
    }
}
