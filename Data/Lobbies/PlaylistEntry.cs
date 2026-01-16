using Il2CppAssets.Scripts.Database;

namespace Multiplayer.Data.Lobbies
{
    public class PlaylistEntry
    {
        public MusicInfo MusicInfo { get; private set; }
        public int Difficulty { get; private set; }
        public string Entry { get; private set; }
        public bool StartedPlaying {  get; private set; }

        internal void Play() { StartedPlaying = true; }

        internal PlaylistEntry(MusicInfo musicInfo, int difficulty, string entry)
        {
            MusicInfo = musicInfo;
            Difficulty = difficulty;
            Entry = entry;
            StartedPlaying = false;
        }
    }
}
