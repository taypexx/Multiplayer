using CustomAlbums.Data;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Static;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Data
{
    public class CustomChartData
    {
        public Album Album;
        public MusicInfo MusicInfo
        {
            get
            {
                return GlobalDataBase.dbMusicTag.GetMusicInfoFromAll(Album.Uid);
            }
        }
        public bool? IsOnWebsite { get; internal set; } = null;
        public string? WebsiteId { get; internal set; } = null;
        public bool? IsRanked { get; internal set; } = null;

        public CustomChartData(Album album)
        {
            Album = album;
        }

        /// <summary>
        /// Gets data from mdmc api and updates fields. Should be called only if necessary.
        /// </summary>
        internal async Task Update()
        {
            try
            {
                if (!Album.Sheets.ContainsKey(2)) return;
                using var response = await Client.GetAsync(Constants.MDMCAPIEndpoint + "sheets/" + Album.Sheets[2].Md5, true, false, true);

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
                    }
                }
            }
            catch (Exception ex)
            {
                Main.Log(ex);
            }
        }
    }
}
