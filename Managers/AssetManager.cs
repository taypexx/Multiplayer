using CustomAlbums.Utilities;
using Multiplayer.Data;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Managers
{
    internal static class AssetManager
    {
        private static Assembly Assembly = Assembly.GetExecutingAssembly();
        private static Dictionary<string, CustomImageAsset> ImageAssets;
        private static GameObject AssetHolder;

        /// <summary>
        /// Gets the <see cref="CustomImageAsset"/> reference or creates a new one and caches it.
        /// </summary>
        /// <param name="relativePath">Path relative to Assets.</param>
        /// <returns><see cref="CustomImageAsset"/> reference.</returns>
        internal static CustomImageAsset GetImageAsset(string relativePath)
        {
            if (ImageAssets.TryGetValue(relativePath, out CustomImageAsset asset))
            {
                return asset;
            } else
            {
                if (AssetHolder == null)
                {
                    AssetHolder = new("MultiplayerAssetHolder");
                    UnityEngine.Object.DontDestroyOnLoad(AssetHolder);
                }

                using Stream stream = Assembly.GetManifestResourceStream("Multiplayer.Assets." + relativePath);
                if (stream == null) return null;

                byte[] bytes = stream.ToMemoryStream().ReadFully();
                if (bytes == null) return null;

                CustomImageAsset newAsset = new(bytes);
                if (newAsset == null) return null;

                ImageAssets.Add(relativePath, newAsset);

                GameObject go = new("Img");
                go.transform.parent = AssetHolder.transform;
                go.AddComponent<Image>().sprite = newAsset.Sprite;

                return newAsset;
            }
        }

        /// <summary>
        /// Gets the file content as <see langword="string"/>.
        /// </summary>
        /// <param name="relativePath">Path relative to the executing assembly.</param>
        /// <returns><see langword="string"/> content</returns>
        internal static string GetStringAsset(string relativePath)
        {
            using Stream stream = Assembly.GetManifestResourceStream("Multiplayer." + relativePath);
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
