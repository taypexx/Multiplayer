using CustomAlbums.Data;
using CustomAlbums.Managers;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Static;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Data
{
    public class CustomChartData
    {
        public Album Album;
        public MusicInfo MusicInfo;
        public bool? IsOnWebsite { get; internal set; } = null;
        public string? WebsiteId { get; internal set; } = null;
        public bool? IsRanked { get; internal set; } = null;

        public Dictionary<int, string> MapDifficulties;

        public CustomChartData(Album album)
        {
            Album = album;
            MusicInfo = GlobalDataBase.dbMusicTag.GetMusicInfoFromAll(album.Uid);
            MapDifficulties = new();
        }

        /// <summary>
        /// Gets data from mdmc api and updates fields. Should be called only when necessary.
        /// </summary>
        internal async Task Update()
        {
            if (!Album.Sheets.ContainsKey(2)) return;
            var response = await Client.GetAsync(Constants.MDMCAPIEndpoint + "sheets/" + Album.Sheets[2].Md5, true, false, true);

            // We check for the 404 specifically, because the server might be down or anything.
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                IsOnWebsite = response.IsSuccessStatusCode;
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
                    var chartData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body["chart"]);
                    WebsiteId = chartData["id"].GetString();
                    IsRanked = chartData["ranked"].GetBoolean();
                    MapDifficulties[body["map"].GetInt32()] = body["difficulty"].GetString();

                    var thisSheetId = body["id"].GetString();
                    foreach (var sheetId in JsonSerializer.Deserialize<List<string>>(chartData["sheets"]))
                    {
                        if (sheetId == thisSheetId) continue;

                        var otherSheet = await Client.GetAsync(Constants.MDMCAPIEndpoint + "sheets/" + sheetId, true, false, true);
                        if (!otherSheet.IsSuccessStatusCode) continue;

                        var otherBody = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
                        MapDifficulties[otherBody["map"].GetInt32()] = otherBody["difficulty"].GetString();
                    }
                }
            }
        }
    }
}
