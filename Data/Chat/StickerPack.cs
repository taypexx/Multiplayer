using Multiplayer.Static;

namespace Multiplayer.Data.Chat
{
    public class StickerPack
    {
        public string Name { get; private set; }
        public string Description => Localization.Get("StickerPacks", Name ?? "Default").ToString();
        public Dictionary<string, Sticker> Stickers { get; private set; }

        public StickerPack(string name)
        {
            Name = name;
            Stickers = new();
        }
    }
}
