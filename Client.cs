using Multiplayer.Managers;
using Newtonsoft.Json;
using System.Text;

namespace Multiplayer
{
    internal static class Client
    {
        internal static bool IsConnected => Connected;
        private static bool Connected = false;
        internal static bool TriedConnecting = false;

        private static readonly string Endpoint = Settings.Config.MultiplayerAPI;

        internal static HttpMessageHandler HttpHandler = new HttpClientHandler();
        internal static HttpClient Http;

        /// <summary>
        /// Performs an <see langword="async"/> GET request.
        /// </summary>
        /// <param name="path">Path of the request relative to the API endpoint.</param>
        /// <param name="isFullPath">(Optional) Makes the <paramref name="path">path</paramref> absolute if true.</param>
        /// <returns><see cref="HttpContent"/> if the request was successful, otherwise null.</returns>
        private static async Task<HttpContent> GetAsync(string path, bool isFullPath = false)
        {
            try
            {
                HttpResponseMessage response = await Http.GetAsync(isFullPath ? path : Endpoint + path);
                if (!response.IsSuccessStatusCode)
                {
                    Disconnect();
                    return null;
                }
                return response.Content;
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
        /// <param name="isFullPath">(Optional) Makes the <paramref name="path">path</paramref> absolute if true.</param>
        /// <returns><see langword="true"/> if the request was successful, otherwise <see langword="false"/>.</returns>
        private static async Task<bool> PostAsync(string path, object data, bool isFullPath = false)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(data);
                var sendContent = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await Http.PostAsync(isFullPath ? path : Endpoint + path, sendContent);
                if (!response.IsSuccessStatusCode)
                {
                    Disconnect();
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                Main.Logger.Error("Couldn't perform a POST request!");
                Disconnect();
                return false;
            }
        }

        internal static async Task Connect()
        {
            if (IsConnected || TriedConnecting) { return; }
            TriedConnecting = true;

            Main.Logger.Msg("Connecting to the server...");

            Http = new HttpClient(HttpHandler);

            if (await GetAsync("connect") != null)
            {
                Connected = true;
                Main.Logger.Success("Connected to the server successfully!");

                //PlayerManager.Init(); UNCOMMENT THIS LATER WHEN THE SERVER IS UP
            } else
            {
                Main.Logger.Error("Failed to connect to the server!");
            }
            Connected = true;// REMOVE THIS LATER WHEN THE SERVER IS UP
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
