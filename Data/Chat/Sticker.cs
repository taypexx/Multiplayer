using Multiplayer.Managers;

namespace Multiplayer.Data.Chat
{
    public class Sticker
    {
        public string Name { get; private set; }
        public string Path { get; private set; }

        internal CustomImageAsset Asset;

        internal Sticker(string fileName)
        {
            Name = fileName;
            Path = "Stickers." + fileName;
            Asset = AssetManager.GetImageAsset(Path);
        }
    }
}
