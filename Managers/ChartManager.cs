using CustomAlbums.Data;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Data;
using Multiplayer.Static;
using Multiplayer.UI.Extensions;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Managers
{
    internal static class ChartManager
    {
        internal static bool Initialized { get; private set; } = false;

        internal static int FinishedDownloads { 
            get; 
            private set 
            {
                if (value != 0 && value == DownloadingCharts)
                {
                    var allDownloaded = value == SuccessfulDownloads;
                    field = 0;
                    SuccessfulDownloads = 0;
                    DownloadingCharts = 0;

                    _ = LobbyManager.SetDownloading(false);
                    PnlCloudExtension.Finish(allDownloaded);
                }
                else field = value;
            } 
        } = 0;
        internal static int SuccessfulDownloads { get; private set; } = 0;

        internal static int DownloadingCharts { 
            get; 
            private set 
            {
                var currentText = string.Format(
                    Localization.Get("PnlCloudMessage", "Downloading").ToString(),
                    FinishedDownloads, value
                );

                if (value == 1 && field == 0)
                {
                    _ = LobbyManager.SetDownloading(true);
                    PnlCloudExtension.Start(currentText);
                }
                else if (value > 1)
                {
                    PnlCloudExtension.Update(currentText);
                }

                field = value;
            } 
        } = 0;

        internal static bool IsDownloading => DownloadingCharts > 0;

        // [MD5] = data
        internal static Dictionary<string, CustomChartData> CustomCharts;

        internal static int CurrentDifficulty => GlobalDataBase.dbBattleStage.m_MapDifficulty;

        /// <summary>
        /// Downloads a chart from the MDMC website.
        /// </summary>
        /// <param name="md5">MD5 hash of any map of the chart.</param>
        internal static async Task DownloadChart(string md5)
        {
            if (CustomCharts.ContainsKey(md5)) return;
            DownloadingCharts++;

            var response = await Client.GetAsync(Constants.MDMCAPIEndpoint + "sheets/" + md5, true, false, true);
            if (!response.IsSuccessStatusCode)
            {
                FinishedDownloads++;
                return;
            }

            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
            var chartData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body["chart"]);
            var websiteId = chartData["id"].GetString();
            var chartName = chartData["title"].GetString();
            var chartFileName = $"{chartName}.mdm";

            var chartBytes = await Client.DownloadAsync(Constants.MDMCAPIEndpoint + $"charts/{websiteId}/download");
            var tempChartPath = Path.Combine(Path.GetTempPath(), chartFileName);
            await File.WriteAllBytesAsync(tempChartPath, chartBytes);

            var prevChartCount = AlbumManager.LoadedAlbums.Count;

            string chartPath = Path.Combine(Path.GetFullPath("Custom_Albums"), chartFileName);
            int tries = 0;
            while (true)
            {
                if (File.Exists(chartPath))
                {
                    tries++;
                    chartPath = Path.Combine(Path.GetFullPath("Custom_Albums"), $"{chartName} ({tries}).mdm");
                }
                else break;
            }
            File.Move(tempChartPath, chartPath);

            // Wait for Custom Albums to load the chart
            while (AlbumManager.LoadedAlbums.Count == prevChartCount)
            {
                await Task.Delay(100);
            }

            Main.Dispatch(() =>
            {
                var album = AlbumManager.LoadedAlbums.Values.Last();

                var customChartData = new CustomChartData(album);
                CustomCharts.Add(album.Sheets[2].Md5, customChartData);

                SuccessfulDownloads++;
                FinishedDownloads++;
            });
        }

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
            if (Initialized) return;
            Initialized = true;

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
