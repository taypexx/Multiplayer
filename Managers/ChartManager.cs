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
        private static Dictionary<string, CustomChartData> CustomCharts;

        internal static int CurrentDifficulty => 
            GlobalDataBase.dbMusicTag.selectedDiffTglIndex == 3 
            && Singleton<SpecialSongManager>.instance.IsInvokeHideBms(GlobalDataBase.dbMusicTag.CurMusicInfo().uid) 
            ? 4 
            : GlobalDataBase.dbMusicTag.selectedDiffTglIndex;

        /// <summary>
        /// Checks whether the custom chart is on the website.
        /// </summary>
        internal static async Task<bool> IsCustomOnWebsite(string uid)
        {
            bool onWebsite = false;
            if (!CustomCharts.TryGetValue(uid, out CustomChartData data)) return onWebsite;

            if (data.IsOnWebsite is null)
            {
                var response = await Client.GetAsync(Constants.MDMCAPIEndpoint + "sheets/" + GetMD5(uid), true, false, true);

                // We want the 404 specifically, because the server might be down or anything.
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    onWebsite = response.IsSuccessStatusCode;
                    data.IsOnWebsite = onWebsite;
                }
            }
            else onWebsite = (bool)data.IsOnWebsite;

            return onWebsite;
        }

        internal static string GetNiceChartName(MusicInfo musicInfo, int diff) => String.Format(
            "{0} {1}★",
            musicInfo.name,
            musicInfo.GetMusicLevelStringByDiff(diff)
        );

        internal static string GetEntry(MusicInfo musicInfo, int difficulty) => String.Format("{0}#{1}", GetEntryKey(musicInfo), difficulty);

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
                return CustomCharts[str].MusicInfo;
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

                CustomCharts.Add(sheet.Md5, new(album));
            }
        }
    }
}
