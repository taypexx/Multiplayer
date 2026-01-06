using CustomAlbums.Data;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;

namespace Multiplayer.Data
{
    public class CustomChartData
    {
        public Album Album { get; private set; }
        public MusicInfo MusicInfo { get; private set; }
        public bool? IsOnWebsite { get; internal set; } = null;

        public CustomChartData(Album album)
        {
            Album = album;
            MusicInfo = GlobalDataBase.dbMusicTag.GetMusicInfoFromAll($"{AlbumManager.Uid}-{album.Index}");
        }
    }
}
