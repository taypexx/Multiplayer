using CustomAlbums.Data;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Data;
using Multiplayer.Static;

namespace Multiplayer.Managers
{
    internal static class ChartManager
    {
        internal static bool Initialized { get; private set; } = false;

        private static Sheet GetSheet(Album album)
        {
            if (album.Sheets.TryGetValue(2, out var sheet)) return sheet;
            if (album.Sheets.TryGetValue(3, out sheet)) return sheet;
            if (album.Sheets.TryGetValue(1, out sheet)) return sheet;
            if (album.Sheets.TryGetValue(0, out sheet)) return sheet;
            return null;
        }

        // [MD5] = data
        internal static Dictionary<string, CustomChartData> CustomCharts;

        internal static int CurrentDifficulty
        {
            get
            {
                int diff = GlobalDataBase.dbMusicTag.selectedDiffTglIndex;
                if (diff == 3)
                {
                    var musicInfo = GlobalDataBase.dbMusicTag.m_CurSelectedMusicInfo;
                    if (musicInfo != null)
                    {
                        string checkUid = musicInfo.uid;
                        if (checkUid.StartsWith("999-"))
                        {
                            var customData = GetCustomChartData(checkUid);
                            if (customData != null) checkUid = customData.Album.Uid;
                        }

                        var ssm = Il2CppAssets.Scripts.PeroTools.Commons.Singleton<Il2Cpp.SpecialSongManager>.instance;
                        if (ssm != null && ssm.IsInvokeHideBms(checkUid))
                        {
                            return 4;
                        }
                    }
                }
                return diff;
            }
        }

        /// <summary>
        /// Gets the <see cref="CustomChartData"/> by the <paramref name="uid"/>.
        /// </summary>
        internal static CustomChartData GetCustomChartData(string uid)
        {
            var md5 = GetMD5(uid);
            if (md5 == null) return null;
            if (!CustomCharts.TryGetValue(md5, out CustomChartData data))
            {
                var album = CustomAlbums.Managers.AlbumManager.GetByUid(uid);
                if (album == null) return null;
                data = new CustomChartData(album);
                CustomCharts.Add(md5, data);
                _ = PlayerManager.SyncCustoms();
            }
            return data;
        }

        /// <returns>A nice formatted <see cref="string"/> of the given <paramref name="musicInfo"/> and <paramref name="difficulty"/>.</returns>
        internal static string GetNiceChartName(MusicInfo musicInfo, int difficulty)
        {
            if (musicInfo == null) return String.Format("Unknown Chart {0}★", difficulty);

            string levelStr = musicInfo.GetMusicLevelStringByDiff(difficulty);
            
            if (difficulty == 4 && musicInfo.uid.StartsWith(CustomAlbums.Managers.AlbumManager.Uid.ToString() + "-"))
            {
                var album = CustomAlbums.Managers.AlbumManager.GetByUid(musicInfo.uid);
                if (album != null && !string.IsNullOrEmpty(album.Info.HideBmsDifficulty) && album.Info.HideBmsDifficulty != "0")
                {
                    levelStr = album.Info.HideBmsDifficulty;
                }
            }

            return String.Format(
                "{0} {1}★",
                musicInfo.GetLocal(Localization.LanguageIndex).name,
                levelStr
            );
        }

        internal static string GetEntry(MusicInfo musicInfo, int difficulty) => String.Format("{0}#{1}#{2}", GetEntryKey(musicInfo), difficulty, Multiplayer.Managers.PlayerManager.LocalPlayer?.MultiplayerStats?.Name ?? "Unknown");

        /// <summary>
        /// Gets the MD5 hash of a custom chart by its <see cref="MusicInfo"/>.
        /// </summary>
        internal static string GetMD5(MusicInfo musicInfo)
        {
            if (musicInfo == null) return null;
            if (musicInfo.albumIndex != AlbumManager.Uid) return null;

            Album album = AlbumManager.GetByUid(musicInfo.uid);
            if (album == null) return null;
            
            var sheet = GetSheet(album);
            if (sheet == null) return null;

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

            var sheet = GetSheet(album);
            if (sheet == null) return null;

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
            if (str.Length >= 16 && !str.StartsWith(CustomAlbums.Managers.AlbumManager.Uid.ToString() + "-"))
            {
                if (CustomCharts.TryGetValue(str, out var data))
                {
                    // Check if the cached album is stale (e.g. deleted by HotReloadManager without firing event)
                    if (AlbumManager.GetByUid(data.Album.Uid) == null)
                    {
                        CustomCharts.Remove(str);
                        data = null;
                    }
                }

                if (data == null)
                {
                    foreach (var pair in AlbumManager.LoadedAlbums)
                    {
                        var album = pair.Value;
                        var sheet = GetSheet(album);
                        if (sheet != null && sheet.Md5 == str)
                        {
                            data = new CustomChartData(album);
                            CustomCharts[str] = data;
                            return data.MusicInfo;
                        }
                    }
                    return null;
                }
                return data.MusicInfo;
            } 
            else return GlobalDataBase.dbMusicTag.GetMusicInfoFromAll(str);
        }

        internal static void Init()
        {
            if (Initialized) return;
            Initialized = true;

            CustomCharts = new();

            foreach ((_, Album album) in AlbumManager.LoadedAlbums)
            {
                var sheet = GetSheet(album);
                if (sheet == null) continue;
                if (CustomCharts.ContainsKey(sheet.Md5)) continue;

                CustomCharts.Add(sheet.Md5, new(album));
            }

            // Listen for hot-reloaded album events
            CustomAlbums.ModExtensions.Events.OnAlbumLoaded += OnAlbumLoaded;
        }

        // Handle hot-reloaded album events to keep chart cache and playlist in sync
        private static void OnAlbumLoaded(object sender, CustomAlbums.ModExtensions.AlbumEventArgs e)
        {
            var album = e.Album;
            if (album == null) return;

            var sheet = GetSheet(album);
            if (sheet == null) return;

            // Remove old entry with the same album name
            var oldKeys = CustomCharts.Where(pair => pair.Value.Album.AlbumName == album.AlbumName).Select(pair => pair.Key).ToList();
            foreach (var key in oldKeys)
            {
                CustomCharts.Remove(key);
            }

            // Add the new entry
            var newChartData = new CustomChartData(album);
            CustomCharts[sheet.Md5] = newChartData;

            // Sync with the server
            _ = PlayerManager.SyncCustoms();
        }
    }
}
