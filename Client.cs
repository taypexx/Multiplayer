using Il2CppAssets.Scripts.Database;
using Multiplayer.Managers;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Multiplayer
{
    internal static class Client
    {
        internal static bool IsConnected => Connected;
        private static bool Connected = false;
        internal static bool TriedConnecting = false;

        private static string TOKEN = string.Empty;
        private static readonly string Endpoint = Settings.Config.MultiplayerAPI;

        internal static HttpMessageHandler HttpHandler;
        internal static HttpClient Http;

        /// <summary>
        /// Performs an <see langword="async"/> GET request.
        /// </summary>
        /// <param name="path">Path of the request relative to the API endpoint.</param>
        /// <param name="isFullPath">(Optional) Makes the <paramref name="path">path</paramref> absolute if <see langword="true"/>.</param>
        /// <returns><see cref="HttpContent"/> if the request was successful, otherwise <see langword="null"/>.</returns>
        internal static async Task<HttpResponseMessage> GetAsync(string path, bool isFullPath = false)
        {
            try
            {
                HttpResponseMessage response = await Http.GetAsync(isFullPath ? path : Endpoint + path);
                if (!response.IsSuccessStatusCode)
                {
                    Main.Logger.Error($"{response.StatusCode}: {response.ReasonPhrase}");
                    Disconnect();
                    return null;
                }
                return response;
            }
            catch (Exception)
            {
                Main.Logger.Error("Couldn't perform a GET request!");
                Disconnect();
                return null;
            }
        }

        /// <summary>
        /// Performs an <see langword="async"/> POST request.
        /// </summary>
        /// <param name="path">Path of the request relative to the API endpoint.</param>
        /// <param name="data">Data to be serialized as JSON and sent.</param>
        /// <param name="isFullPath">(Optional) Makes the <paramref name="path">path</paramref> absolute if <see langword="true"/>.</param>
        /// <returns><see langword="true"/> if the request was successful, otherwise <see langword="false"/>.</returns>
        internal static async Task<HttpResponseMessage> PostAsync(string path, object data, bool isFullPath = false)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(data);
                var sendContent = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await Http.PostAsync(isFullPath ? path : Endpoint + path, sendContent);
                if (!response.IsSuccessStatusCode)
                {
                    Main.Logger.Error($"{response.StatusCode}: {response.ReasonPhrase}");
                    Disconnect();
                    return null;
                }
                return response;
            }
            catch (Exception)
            {
                Main.Logger.Error("Couldn't perform a POST request!");
                Disconnect();
                return null;
            }
        }

        internal static async Task Connect()
        {
            if (IsConnected || TriedConnecting) return;
            TriedConnecting = true;

            string uid = DataHelper.PeroUid;
            if (uid == null) return;

            Main.Logger.Msg("Connecting to the server...");

            HttpHandler = new HttpClientHandler();
            Http = new HttpClient(HttpHandler);

            Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Http.DefaultRequestHeaders.ConnectionClose = false;

            var response = await PostAsync("connect",uid);

            if (response != null)
            {
                TOKEN = await response.Content.ReadAsStringAsync();
                Http.DefaultRequestHeaders.Authorization = new("Bearer", TOKEN);

                Connected = true;
                Main.Logger.Success("Connected to the server successfully!");

                //AchievementManager.Init(); UNCOMMENT THIS LATER WHEN THE SERVER IS UP
                //PlayerManager.Init(); UNCOMMENT THIS LATER WHEN THE SERVER IS UP
            }
            else
            {
                Main.Logger.Error("Failed to connect to the server!");
            }

            Connected = true;// REMOVE THIS LATER WHEN THE SERVER IS UP
            AchievementManager.Init(); // REMOVE THIS LATER WHEN THE SERVER IS UP
            PlayerManager.Init(); // REMOVE THIS LATER WHEN THE SERVER IS UP
        }

        internal static void Disconnect()
        {
            if (IsConnected) 
            {
                Main.Logger.Msg("Disconnecting from the server...");
            }
            Connected = false;

            Http.Dispose();
            UIManager.WarnNotification(Localization.Get("Warning","Offline"));
        }
    }
}
