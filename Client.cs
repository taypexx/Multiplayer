using Il2CppAssets.Scripts.Database;
using Multiplayer.Managers;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Cryptography;
using System.Net.Http.Json;
using PopupLib.UI;
using Multiplayer.UI;
using System.Text.Json;
using LocalizeLib;
using System.Net.Sockets;
using Il2CppSirenix.Serialization.Utilities;

namespace Multiplayer
{
    internal static class Client
    {
        internal static bool Connected { get; private set; } = false;
        internal static bool TriedConnecting { get; private set; } = false;

        internal static bool Outdated => ServerVersion != null && ServerVersion != Main.Version;
        internal static string ServerVersion { get; private set; }
        private static LocalString OutdatedWarning; 

        internal static string Token { get; private set; } = string.Empty;
        private static readonly string APIEndpoint = $"http://{Settings.Config.ServerIP}:{Settings.Config.PortHTTP}/api/";

        internal static Dictionary<string, float> MoeDifficulties { get; private set; }

        private static HttpMessageHandler HttpHandler;
        private static HttpClient Http;

        private static UdpClient Udp;

        internal static string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(rawData);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Performs an <see langword="async"/> SEND request and awaits for the response from the server.
        /// </summary>
        /// <param name="data">Datagram to send.</param>
        /// <returns></returns>
        internal static async Task<byte[]> UdpSendAsync(byte[] data)
        {
            try
            {
                await Udp.SendAsync(data, data.Length, Settings.Config.ServerIP, Settings.Config.PortUdp);
                var result = await Udp.ReceiveAsync();
                return result.Buffer;
            }
            catch (Exception ex)
            {
                Main.Logger.Error(ex.ToString());
                return null;
            }
        }

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
                HttpResponseMessage response = await Http.GetAsync(isFullPath ? path : APIEndpoint + path);
                if (!response.IsSuccessStatusCode)
                {
                    Main.Logger.Error($"{(int)response.StatusCode}: {response.ReasonPhrase}");
                    //Disconnect();
                    return null;
                }
                return response;
            }
            catch (Exception)
            {
                Main.Logger.Error("Couldn't perform a GET request!");
                //Disconnect();
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

                HttpResponseMessage response = await Http.PostAsync(isFullPath ? path : APIEndpoint + path, sendContent);
                if (!response.IsSuccessStatusCode)
                {
                    Main.Logger.Error($"{response.StatusCode}: {response.ReasonPhrase}");
                    //Disconnect();
                    return null;
                }
                return response;
            }
            catch (Exception)
            {
                Main.Logger.Error("Couldn't perform a POST request!");
                //Disconnect();
                return null;
            }
        }

        internal static async Task Connect()
        {
            if (Connected || TriedConnecting) return;
            PopupUtils.ShowInfo(Localization.Get("MainMenu", "Connecting"));
            TriedConnecting = true;

            if (!DataHelper.isLogin || DataHelper.PeroUid.IsNullOrWhitespace())
            {
                UIManager.WarnNotification(Localization.Get("Warning", "NoAccount"));
                return;
            }

            string uid = DataHelper.PeroUid;
            if (uid == null) return;

            Main.Logger.Msg("Connecting to the server...");

            HttpHandler = new HttpClientHandler();
            Http = new(HttpHandler);

            Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Http.DefaultRequestHeaders.ConnectionClose = false;

            Udp = new();

            var response = await PostAsync("connect", new { Uid = uid });
            if (response != null)
            {
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string,JsonElement>>();

                ServerVersion = content["Version"].GetString();
                if (Outdated)
                {
                    OutdatedWarning = new(string.Format(Localization.Get("Warning", "Outdated").ToString(), Main.Version, ServerVersion));
                    Main.Logger.Error("Outdated version of the mod, cannot proceed!");
                    return;
                }

                Token = content["Token"].GetString();
                Connected = true;
                Main.Logger.Success("Connected to the server successfully!");
            }
            else
            {
                Main.Logger.Error("Failed to connect to the server!");
            }
            response.Dispose();

            var moeDiffs = await GetAsync("https://api.musedash.moe/diffdiff", true);
            if (moeDiffs != null)
            {
                MoeDifficulties = new();
                var moeDiffsJson = await moeDiffs.Content.ReadFromJsonAsync<List<List<object>>>();
                foreach (var chartStats in moeDiffsJson)
                {
                    MoeDifficulties.Add((string)chartStats[0], (float)chartStats[4]); // Key - Uid, Value - RL
                }
                Main.Logger.Success("Recieved chart difficulties from musedash.moe successfully!");
            } else
            {
                Main.Logger.Error("Failed to recieve chart difficulties from musedash.moe!");
            }
            moeDiffs.Dispose();
        }

        internal static void Disconnect()
        {
            if (Connected) 
            {
                Main.Logger.Msg("Disconnecting from the server...");
            }
            Connected = false;

            Http.Dispose();
            Udp.Dispose();

            if (Outdated)
            {
                UIManager.WarnNotification(OutdatedWarning);
                return;
            }

            UIManager.WarningChooseAction = ReconnectOption;
            UIManager.WarnChooseNotification(Localization.Get("Warning","Offline"));
        }

        private static void ReconnectOption(bool? doReconnect)
        {
            if (!(doReconnect ?? false)) return;

            TriedConnecting = false;
            UIManager.MainMenu.Open();
        }
    }
}
