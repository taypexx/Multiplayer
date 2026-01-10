using Il2CppAssets.Scripts.Database;

namespace Multiplayer.Data.Lobbies
{
    public class PlaylistEntry
    {
        public MusicInfo MusicInfo { get; private set; }
        public int Difficulty { get; private set; }
        public string Entry { get; private set; }

        internal PlaylistEntry(MusicInfo musicInfo, int difficulty, string entry)
        {
            MusicInfo = musicInfo;
            Difficulty = difficulty;
            Entry = entry;
        }
    }
}
