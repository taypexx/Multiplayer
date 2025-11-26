using CustomAlbums.Data;
using CustomAlbums.Managers;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;

namespace Multiplayer.Managers
{
    internal static class ChartManager
    {
        private static Dictionary<string, MusicInfo> CustomCharts;

        /// <summary>
        /// Gets the MD5 hash of a custom chart from its <see cref="MusicInfo"/>.
        /// </summary>
        internal static string GetMD5(MusicInfo musicInfo)
        {
            if (musicInfo.albumIndex != AlbumManager.Uid) return null;

            Album album = AlbumManager.GetByUid(musicInfo.uid);
            if (album == null) return null;
            if (!album.Sheets.TryGetValue(2, out Sheet sheet)) return null;

            return sheet.Md5;
        }

        /// <summary>
        /// Gets the playlist entry key from a <see cref="MusicInfo"/>.
        /// </summary>
        internal static string GetEntryKey(MusicInfo musicInfo)
        {
            string md5 = GetMD5(musicInfo);
            if (md5 != null) return md5;
            return musicInfo.uid;
        }

        internal static string GetEntry(MusicInfo musicInfo, int difficulty) => String.Format("{0}#{1}", GetEntryKey(musicInfo), difficulty);

        internal static int CurrentDifficulty => GlobalDataBase.dbMusicTag.selectedDiffTglIndex == 3 && Singleton<SpecialSongManager>.instance.IsInvokeHideBms(GlobalDataBase.dbMusicTag.CurMusicInfo().uid) ? 4 : GlobalDataBase.dbMusicTag.selectedDiffTglIndex;

        /// <summary>
        /// Gets the <see cref="MusicInfo"/> by the hash/vanilla uid.
        /// </summary>
        /// <param name="str">MD5 hash or vanilla uid.</param>
        internal static MusicInfo GetMusicInfo(string str)
        {
            if (str.Length >= 16)
            {
                return CustomCharts[str];
            } 
            else
            {
                return GlobalDataBase.dbMusicTag.GetMusicInfoFromAll(str);
            }
        }

        internal static void Init()
        {
            CustomCharts = new();

            foreach ((_, Album album) in AlbumManager.LoadedAlbums)
            {
                if (!album.Sheets.TryGetValue(2, out Sheet sheet)) continue;
                if (CustomCharts.ContainsKey(sheet.Md5)) continue;

                CustomCharts.Add(sheet.Md5, GlobalDataBase.dbMusicTag.GetMusicInfoFromAll($"{AlbumManager.Uid}-{album.Index}"));
            }
        }
    }
}
