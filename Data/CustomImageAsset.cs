using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
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

        private void LoadTexture(byte[] bytes)
        {
            Texture = new Texture2D(2, 2, TextureFormat.ARGB32, false)
            {
                wrapMode = TextureWrapMode.MirrorOnce
            };
            ImageConversion.LoadImage(Texture, bytes);
        }

        private void LoadTexture(Image<Rgba32> image, bool flip = false)
        {
            Texture = new Texture2D(image.Width, image.Height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.MirrorOnce
            };

            byte[] pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);

            if (flip)
            {
                int rowSize = image.Width * 4;
                byte[] flipped = new byte[pixels.Length];

                for (int y = 0; y < image.Height; y++)
                {
                    int srcIndex = y * rowSize;
                    int dstIndex = (image.Height - 1 - y) * rowSize;
                    Buffer.BlockCopy(pixels, srcIndex, flipped, dstIndex, rowSize);
                }

                Texture.LoadRawTextureData(flipped);
            }
            else Texture.LoadRawTextureData(pixels);

            Texture.Apply();
        }

        internal CustomImageAsset(Image<Rgba32> image, bool flip = false)
        {
            Main.Dispatch(() =>
            {
                LoadTexture(image, flip);
                Init();
            });
        }

        internal CustomImageAsset(byte[] bytes)
        {
            LoadTexture(bytes);
            Init();
        }

        internal CustomImageAsset(Texture2D texture2D)
        {
            Texture = texture2D;
            Init();
        }
    }
}
