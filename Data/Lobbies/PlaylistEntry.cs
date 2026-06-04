using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Managers;

namespace Multiplayer.Data.Lobbies
{
    public class PlaylistEntry
    {
        private MusicInfo _musicInfo;
        public MusicInfo MusicInfo
        {
            get
            {
                if ((_musicInfo == null || _musicInfo.Pointer == IntPtr.Zero) && !string.IsNullOrEmpty(EntryKey))
                {
                    _musicInfo = ChartManager.GetMusicInfo(EntryKey);
                }
                return _musicInfo;
            }
            private set => _musicInfo = value;
        }

        public int Difficulty { get; private set; }
        public string Entry { get; private set; }
        public string EntryKey { get; private set; }
        public bool IsCustom { get; private set; }

        internal PlaylistEntry(MusicInfo musicInfo, int difficulty, string entry)
        {
            MusicInfo = musicInfo;
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

        // Force re-fetch MusicInfo on next access (used after hot-reload)
        internal void InvalidateMusicInfo()
        {
            _musicInfo = null;
        }
    }
}
