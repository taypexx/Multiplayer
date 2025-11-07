using Multiplayer.Managers;
using UnityEngine;

namespace Multiplayer.Data
{
    public class CustomImageAsset
    {
        internal string Path {  get; private set; }
        internal Texture2D Texture { get; private set; }
        internal Sprite Sprite { get; private set; }

        internal CustomImageAsset(string path, bool isFull = false)
        {
            Path = isFull ? path : System.IO.Path.Combine(AssetManager.AssetsPath, "Multiplayer.Assets." + path);
            Load();
        }

        private void Load()
        {
            if (!File.Exists(Path)) return;

            byte[] bytes = File.ReadAllBytes(Path);

            Texture = new Texture2D(2, 2, TextureFormat.ARGB32, false)
            {
                wrapMode = TextureWrapMode.MirrorOnce
            };
            ImageConversion.LoadImage(Texture, bytes);

            Sprite = Sprite.Create(Texture, new Rect(0, 0, Texture.width, Texture.height), new Vector2(0.5f, 0.5f));

            UnityEngine.Object.DontDestroyOnLoad(Texture);
            UnityEngine.Object.DontDestroyOnLoad(Sprite);
        }
    }
}
