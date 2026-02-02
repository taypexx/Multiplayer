using CustomAlbums.Data;
using CustomAlbums.Managers;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Multiplayer.Data;
using Multiplayer.Static;

namespace Multiplayer.Managers
{
    internal static class ChartManager
    {
        // [MD5] = data
        internal static Dictionary<string, CustomChartData> CustomCharts;

        internal static int CurrentDifficulty => 
            GlobalDataBase.dbMusicTag.selectedDiffTglIndex == 3 
            && Singleton<SpecialSongManager>.instance.IsInvokeHideBms(GlobalDataBase.dbMusicTag.CurMusicInfo().uid) 
            ? 4 
            : GlobalDataBase.dbMusicTag.selectedDiffTglIndex;

        /// <summary>
        /// Gets the <see cref="CustomChartData"/> by the <paramref name="uid"/>.
        /// </summary>
        internal static CustomChartData GetCustomChartData(string uid)
        {
            var md5 = GetMD5(uid);
            if (md5 == null) return null;
            if (!CustomCharts.TryGetValue(md5, out CustomChartData data)) return null;
            return data;
        }

        /// <returns>A nice formatted <see cref="string"/> of the given <paramref name="musicInfo"/> and <paramref name="difficulty"/>.</returns>
        internal static string GetNiceChartName(MusicInfo musicInfo, int difficulty) => String.Format(
            "{0} {1}★",
            musicInfo.GetLocal(Localization.LanguageIndex).name,
            musicInfo.GetMusicLevelStringByDiff(difficulty)
        );

        /// <returns>A <see cref="string"/> representation of the future playlist entry.</returns>
        internal static string GetEntry(MusicInfo musicInfo, int difficulty) => String.Format("{0}#{1}", GetEntryKey(musicInfo), difficulty);

        /// <summary>
        /// Gets the MD5 hash of a custom chart by its <see cref="MusicInfo"/>.
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
        /// Gets the MD5 hash of a custom chart by its UID.
        /// </summary>
        internal static string GetMD5(string uid)
        {
            if (!uid.StartsWith(AlbumManager.Uid.ToString())) return null;

            Album album = AlbumManager.GetByUid(uid);
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

        /// <summary>
        /// Gets the playlist entry key by the UID.
        /// </summary>
        internal static string GetEntryKey(string uid)
        {
            string md5 = GetMD5(uid);
            if (md5 != null) return md5;
            return uid;
        }

        /// <summary>
        /// Gets the <see cref="MusicInfo"/> by the hash/vanilla uid.
        /// </summary>
        /// <param name="str">MD5 hash or vanilla uid.</param>
        internal static MusicInfo GetMusicInfo(string str)
        {
            if (str.Length >= 16)
            {
                if (!CustomCharts.TryGetValue(str, out var data)) return null;
                return data.MusicInfo;
            } 
            else return GlobalDataBase.dbMusicTag.GetMusicInfoFromAll(str);
        }

        internal static void Init()
        {
            CustomCharts = new();

            foreach ((_, Album album) in AlbumManager.LoadedAlbums)
            {
                if (!album.Sheets.TryGetValue(2, out Sheet sheet)) continue;
                if (CustomCharts.ContainsKey(sheet.Md5)) continue;

                CustomCharts.Add(sheet.Md5, new(album));
            }
        }
    }
}
