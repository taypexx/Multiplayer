using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Managers;

namespace Multiplayer.Data.Lobbies
{
    public class PlaylistEntry
    {
        public MusicInfo MusicInfo { get; private set; }
        public int Difficulty { get; private set; }
        public string Entry { get; private set; }
        public string EntryKey { get; private set; }
        public bool IsCustom { get; private set; }

        internal PlaylistEntry(MusicInfo musicInfo, int difficulty, string entry)
        {
            MusicInfo = musicInfo;
            Difficulty = difficulty;
            Entry = entry;
            EntryKey = ChartManager.GetEntryKey(musicInfo);
            IsCustom = musicInfo.albumIndex == AlbumManager.Uid;
        }
    }
}
