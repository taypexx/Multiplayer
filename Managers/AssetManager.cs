using MelonLoader.Utils;
using Multiplayer.Data;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Managers
{
    internal static class AssetManager
    {
        internal static string AssetsPath = Path.Combine(MelonEnvironment.UserDataDirectory, "MultiplayerAssets");
        private const int FileBufferSize = 81920;

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

                CustomImageAsset newAsset = new(relativePath);
                if (newAsset == null) { return newAsset; }

                ImageAssets.Add(relativePath, newAsset);

                GameObject go = new("Img");
                go.transform.parent = AssetHolder.transform;
                go.AddComponent<Image>().sprite = newAsset.Sprite;

                return newAsset;
            }
        }

        private static async Task ExtractEmbeddedResourcesAsync()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            if (resourceNames.Length == 0) return;

            var extractionTasks = resourceNames.Select(resourceName =>
                ExtractResourceAsync(assembly, resourceName));

            await Task.WhenAll(extractionTasks);
        }

        private static async Task ExtractResourceAsync(Assembly assembly, string resourceName)
        {
            try
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) return;

                var outputPath = Path.Combine(AssetsPath, resourceName);
                await WriteStreamToFileAsync(stream, outputPath);
            }
            catch (Exception ex)
            {
                Main.Logger.Error($"Failed to extract resource '{resourceName}': {ex.Message}");
            }
        }

        private static async Task WriteStreamToFileAsync(Stream inputStream, string filePath)
        {
            try
            {
                using var outputFileStream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: FileBufferSize,
                    useAsync: true);

                await inputStream.CopyToAsync(outputFileStream);
            }
            catch (Exception ex)
            {
                Main.Logger.Error($"Failed to write stream to file '{Path.GetFileName(filePath)}': {ex.Message}");
                throw;
            }
        }

        private static void PrepareDirectory()
        {
            try
            {
                if (!Directory.Exists(AssetsPath))
                {
                    Directory.CreateDirectory(AssetsPath);
                }
            }
            catch (Exception ex)
            {
                Main.Logger.Error($"Failed to prepare directory: {ex.Message}");
                throw;
            }
        }

        internal static async void Init()
        {
            try
            {
                ImageAssets = new();
                PrepareDirectory();
                await ExtractEmbeddedResourcesAsync();
            }
            catch (Exception ex)
            {
                Main.Logger.Error($"Failed to initialize Asset Manager: {ex.Message}");
            }
        }

        internal static void CleanupDirectory()
        {
            try
            {
                if (!Directory.Exists(AssetsPath))
                {
                    Main.Logger.Msg("Directory does not exist - no cleanup needed.");
                    return;
                }

                Directory.Delete(AssetsPath, true);
                Main.Logger.Msg("Directory cleaned up successfully.");
            }
            catch (Exception ex)
            {
                Main.Logger.Error($"Failed to cleanup directory: {ex.Message}");
            }
        }
    }
}
