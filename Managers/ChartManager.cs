using CustomAlbums.Data;
using CustomAlbums.Managers;
using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Multiplayer.Static;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Managers
{
    internal static class ChartManager
    {
        private static Dictionary<string, MusicInfo> CustomCharts;
        private static Dictionary<string, bool> CustomChartsRanked;

        internal static int CurrentDifficulty => 
            GlobalDataBase.dbMusicTag.selectedDiffTglIndex == 3 
            && Singleton<SpecialSongManager>.instance.IsInvokeHideBms(GlobalDataBase.dbMusicTag.CurMusicInfo().uid) 
            ? 4 
            : GlobalDataBase.dbMusicTag.selectedDiffTglIndex;
        
        /// <summary>
        /// Checks whether the custom chart is ranked.
        /// </summary>
        internal static async Task<bool> IsCustomRanked(string uid)
        {
            string md5 = GetMD5(uid);
            bool ranked = false;
            if (CustomChartsRanked.TryGetValue(md5, out ranked)) return ranked;

            var response = await Client.GetAsync(Constants.MDMCAPIEndpoint + "sheets/" + md5, true, false, true);
            if (response.IsSuccessStatusCode)
            {
                var sheetData = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
                ranked = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(sheetData["chart"].GetRawText())["ranked"].GetBoolean();
            }

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                CustomChartsRanked.Add(md5, ranked);
            }

            return ranked;
        }

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
