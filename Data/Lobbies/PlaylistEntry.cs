using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Managers;

namespace Multiplayer.Data.Lobbies
{
    public class PlaylistEntry
    {
        public MusicInfo MusicInfo
        {
            get
            {
                if (string.IsNullOrEmpty(EntryKey)) return null;
                return ChartManager.GetMusicInfo(EntryKey);
            }
        }

        public int Difficulty { get; private set; }
        public string Entry { get; private set; }
        public string EntryKey { get; private set; }
        public bool IsCustom { get; private set; }

        internal PlaylistEntry(MusicInfo musicInfo, int difficulty, string entry)
        {
            Difficulty = difficulty;
            Entry = entry;
            if (musicInfo != null)
            {
                EntryKey = ChartManager.GetEntryKey(musicInfo);
                IsCustom = musicInfo.albumIndex == AlbumManager.Uid;
            }
            else
            {
                string[] str = entry.Split("#");
                EntryKey = str.Length > 0 ? str[0] : string.Empty;
                IsCustom = true;
            }
        }

    }
}
