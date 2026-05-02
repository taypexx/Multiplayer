using CustomAlbums.Utilities;
using Multiplayer.Data;
using Multiplayer.Static;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Managers
{
    internal static class AssetManager
    {
        private static Dictionary<string, CustomImageAsset> ImageAssets;
        private static GameObject AssetHolder;

        private static void CacheImageAsset(string name, CustomImageAsset imageAsset)
        {
            if (AssetHolder == null)
            {
                AssetHolder = new("MultiplayerAssets");
                UnityEngine.Object.DontDestroyOnLoad(AssetHolder);
            }
            else if (ImageAssets.ContainsKey(name)) return;

            ImageAssets.Add(name, imageAsset);

            GameObject go = new(name);
            go.transform.parent = AssetHolder.transform;
            go.AddComponent<Image>().sprite = imageAsset.Sprite;
        }

        /// <summary>
        /// Gets the <see cref="CustomImageAsset"/> reference or creates a new one and caches it.
        /// </summary>
        /// <param name="relativePath">Path relative to Assets.</param>
        /// <returns><see cref="CustomImageAsset"/> reference.</returns>
        internal static CustomImageAsset GetImageAsset(string relativePath)
        {
            if (ImageAssets.TryGetValue(relativePath, out CustomImageAsset asset)) return asset;

            using Stream stream = Main.CurrentAssembly.GetManifestResourceStream("Multiplayer.Assets." + relativePath);
            if (stream == null) return null;

            byte[] bytes = stream.ToMemoryStream().ReadFully();
            if (bytes == null) return null;

            CustomImageAsset newAsset = new(bytes);
            if (newAsset == null) return null;

            CacheImageAsset(relativePath, newAsset);

            return newAsset;
        }

        /// <summary>
        /// Gets the <see cref="CustomImageAsset"/> reference or creates a new one and caches it (from web).
        /// </summary>
        /// <param name="url">URL of the image.</param>
        /// <param name="ignoreCache">Whether to download the image from <paramref name="url"/> regardless of it being cached.</param>
        /// <returns><see cref="CustomImageAsset"/> reference.</returns>
        internal static async Task<CustomImageAsset> GetImageAssetFromWeb(string url, bool ignoreCache = false)
        {
            if (ImageAssets.TryGetValue(url, out CustomImageAsset asset)) return asset;

            var bytes = await Client.DownloadAsync(url);
            if (bytes == null) return null;

            CustomImageAsset newAsset = null;

            if (url.EndsWith(".webp"))
            {
                var image = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(bytes);
                newAsset = new(image, true);
            }
            else newAsset = new(bytes);

            if (newAsset == null) return null;

            Main.Dispatch(() => CacheImageAsset(url, newAsset));

            return newAsset;
        }

        /// <summary>
        /// Gets the file content as <see langword="string"/>.
        /// </summary>
        /// <param name="relativePath">Path relative to the executing assembly.</param>
        /// <returns><see langword="string"/> content</returns>
        internal static string GetStringAsset(string relativePath)
        {
            using Stream stream = Main.CurrentAssembly.GetManifestResourceStream("Multiplayer." + relativePath);
            if (stream == null) return null;

            using StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        internal static void Init()
        {
            ImageAssets = new();
        }
    }
}
