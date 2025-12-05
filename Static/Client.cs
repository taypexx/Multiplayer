using Multiplayer.Managers;
using PopupLib.UI;
using LocalizeLib;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Net.Http.Headers;
using System.Diagnostics;
using Newtonsoft.Json;
using Il2CppAssets.Scripts.Database;
using Il2CppSirenix.Serialization.Utilities;
using MelonLoader.Utils;
using System.Net;

namespace Multiplayer.Static
{
    internal static class Client
    {
        internal static bool Connected { get; private set; } = false;
        internal static bool TriedConnecting { get; private set; } = false;

        private static Stopwatch Stopwatch = new Stopwatch();
        internal static int PingMS { get; private set; }

        internal static bool Outdated => ServerVersion != null && ServerVersion != Constants.Version;
        internal static string ServerVersion { get; private set; }
        private static LocalString OutdatedWarning;

        internal static string Token { get; private set; }
        internal static readonly string TokenPath = Path.Combine(MelonEnvironment.GameRootDirectory, "mdmp.token");

        internal static string APIEndpoint { get; private set; }
        internal static readonly string MDMCAPIEndpoint = "https://api.mdmc.moe/v3/";

        internal static Dictionary<string, float> MoeDifficulties { get; private set; }

        private static HttpMessageHandler HttpHandler;
        private static HttpClient Http;
        private static UdpClient Udp;

        /// <summary>
        /// Downloads a file from the given path.
        /// </summary>
        /// <returns><see cref="FileStream"/> of the downloaded file.</returns>
        internal static async Task<bool> DownloadAsync(string path, string destinationPath)
        {
            try
            {
                using (Stream contentStream = await Http.GetStreamAsync(path))
                {
                    using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                    {
                        await contentStream.CopyToAsync(fileStream);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Main.Logger.Error("An unexpected error occurred: " + e.Message);
                return false;
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
                Stopwatch.Restart();

                await Udp.SendAsync(data, data.Length, Settings.Config.ServerIP, Settings.Config.PortUdp);
                var result = await Udp.ReceiveAsync();

                Stopwatch.Stop();
                PingMS = (int)Stopwatch.ElapsedMilliseconds;

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
        /// <param name="getAnyway">Will return the <see cref="HttpResponseMessage"/> regardless of it being unsuccessful.</param>
        /// <returns><see cref="HttpContent"/> if the request was successful, otherwise <see langword="null"/>.</returns>
        internal static async Task<HttpResponseMessage> GetAsync(string path, bool isFullPath = false, bool getAnyway = false)
        {
            try
            {
                HttpResponseMessage response = await Http.GetAsync(isFullPath ? path : APIEndpoint + path);
                if (!response.IsSuccessStatusCode)
                {
                    Main.Logger.Error($"{(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    //Disconnect();
                    if (!getAnyway) return null;
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
        /// <param name="getAnyway">Will return the <see cref="HttpResponseMessage"/> regardless of it being unsuccessful.</param>
        /// <returns><see langword="true"/> if the request was successful, otherwise <see langword="false"/>.</returns>
        internal static async Task<HttpResponseMessage> PostAsync(string path, object data, bool isFullPath = false, bool getAnyway = false)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(data);
                var sendContent = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await Http.PostAsync(isFullPath ? path : APIEndpoint + path, sendContent);
                if (!response.IsSuccessStatusCode)
                {
                    Main.Logger.Error($"{(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
                    //Disconnect();
                    if (!getAnyway) return null;
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

        internal static async Task Connect(string code = null)
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

            var response = await PostAsync("connect", new { Uid = uid, Code = code ?? Token }, false, true);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

                ServerVersion = content["Version"].GetString();
                if (Outdated)
                {
                    OutdatedWarning = new(string.Format(Localization.Get("Warning", "Outdated").ToString(), Constants.Version, ServerVersion));
                    Main.Logger.Error("Outdated version of the mod, cannot proceed!");
                    return;
                }

                Token = content["Token"].GetString();
                File.WriteAllText(TokenPath,Token);

                Connected = true;
                response.Dispose();
                Main.Logger.Msg("Connected to the server successfully!");
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    TriedConnecting = false;
                    UIManager.WarnNotification(Localization.Get("Warning", "WrongCode"));
                }
                Main.Logger.Error("Failed to connect to the server!");
            }

            /*
            var moeDiffs = await GetAsync("https://api.musedash.moe/diffdiff", true);
            if (moeDiffs != null)
            {
                MoeDifficulties = new();
                var moeDiffsJson = await moeDiffs.Content.ReadFromJsonAsync<List<List<object>>>();
                foreach (var chartStats in moeDiffsJson)
                {
                    MoeDifficulties.Add((string)chartStats[0], (float)chartStats[4]); // Key - Uid, Value - RL
                }
                moeDiffs.Dispose();
                Main.Logger.Success("Recieved chart difficulties from musedash.moe successfully!");
            } else
            {
                Main.Logger.Error("Failed to recieve chart difficulties from musedash.moe!");
            }
            */
        }

        internal static void Disconnect()
        {
            if (Connected)
            {
                Main.Logger.Msg("Disconnecting from the server...");
            }
            Connected = false;

            LobbyManager.LocalLobby = null;

            if (Outdated)
            {
                UIManager.WarnNotification(OutdatedWarning);
                return;
            }

            UIManager.WarningChooseAction = ReconnectOption;
            UIManager.WarnChooseNotification(Localization.Get("Warning", "Offline"));
        }

        private static void ReconnectOption(bool? doReconnect)
        {
            if (!(doReconnect ?? false)) return;

            TriedConnecting = false;
            UIManager.MainMenu.Open();
        }

        internal static void Init()
        {
            HttpHandler = new HttpClientHandler();
            Http = new(HttpHandler);
            Http.DefaultRequestHeaders.ConnectionClose = false;
            Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Udp = new();

            // Run after settings loaded
            APIEndpoint = $"http://{Settings.Config.ServerIP}:{Settings.Config.PortHTTP}/api/";

            if (File.Exists(TokenPath)) Token = File.ReadAllText(TokenPath);
        }
    }
}
