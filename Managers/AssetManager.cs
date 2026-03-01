using CustomAlbums.Utilities;
using Multiplayer.Data;
using UnityEngine;
using UnityEngine.Networking;
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
        /// <returns><see cref="CustomImageAsset"/> reference.</returns>
        internal static async Task<CustomImageAsset> GetImageAssetFromWeb(string url)
        {
            if (ImageAssets.TryGetValue(url, out CustomImageAsset asset)) return asset;

            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.isHttpError) return null;

            CustomImageAsset newAsset = new(DownloadHandlerTexture.GetContent(request));
            if (newAsset == null) return null;

            CacheImageAsset(url, newAsset);

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
