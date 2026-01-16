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
using System.Net.WebSockets;
using Multiplayer.Data;

namespace Multiplayer.Static
{
    internal static class Client
    {
        internal static bool Connected { get; private set; } = false;
        internal static bool TriedConnecting { get; private set; } = false;

        private static Stopwatch Stopwatch = new Stopwatch();
        internal static ushort PingMS { get; private set; }

        internal static readonly string APIAddress = $"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/api/";
        internal static readonly Uri WebsocketAddress = new($"wss://{Constants.ServerAddress}/chat");

        internal static string ServerVersion { get; private set; }
        internal static bool Outdated => ServerVersion != null && ServerVersion != Constants.Version;
        private static LocalString OutdatedWarning;

        private static HttpMessageHandler HttpHandler;
        private static HttpClient Http;
        private static UdpClient Udp;
        private static ClientWebSocket WebSocket;

        internal static string Token { get; 
            private set 
            {
                if (value is null)
                {
                    Http.DefaultRequestHeaders.Authorization = null;
                } 
                else if (Http != null)
                {
                    Http.DefaultRequestHeaders.Authorization = new("Bearer", value);
                }
                field = value;
            } 
        }
        internal static readonly string TokenPath = Path.Combine(MelonEnvironment.GameRootDirectory, "mdmp.token");

        internal static async Task ChatWebsocketListen()
        {
            if (!LobbyManager.IsInLobby || WebSocket.State == WebSocketState.Open) return;

            WebSocket.Options.SetRequestHeader("Authorization", $"Basic {PlayerManager.LocalPlayerUid}#{Token}");

            Main.Logger.Msg("Connecting to the chat Websocket...");
            await WebSocket.ConnectAsync(WebsocketAddress, CancellationToken.None);
            Main.Logger.Msg("Websocket connection was successfully established at " + WebsocketAddress);

            var buffer = new byte[4096];
            while (WebSocket.State == WebSocketState.Open && LobbyManager.IsInLobby)
            {
                var msgBuilder = new StringBuilder();
                WebSocketReceiveResult res = null;

                do {
                    res = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (res.MessageType == WebSocketMessageType.Close || !LobbyManager.IsInLobby)
                    {
                        await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        goto End;
                    }
                    else if (res.MessageType != WebSocketMessageType.Text) continue;

                    msgBuilder.Append(Encoding.UTF8.GetString(buffer, 0, res.Count));
                } 
                while (!res.EndOfMessage);

                ChatMessage chatMessage = System.Text.Json.JsonSerializer.Deserialize<ChatMessage>(msgBuilder.ToString());
                Chat.Recieve(chatMessage);
            }
            End:
            Main.Logger.Msg("Websocket connection ended.");
        }

        internal static void ChatWebsocketSend(string payload)
        {
            if (WebSocket.State != WebSocketState.Open) return;
            var bytes = Encoding.UTF8.GetBytes(payload);
            _ = WebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }

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
                await Udp.SendAsync(data, data.Length, Constants.ServerAddress, Constants.PortUDP);
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
        /// <param name="getAnyway">Will return the <see cref="HttpResponseMessage"/> regardless of it being unsuccessful.</param>
        /// <param name="doAuth">Whether to include the Authorization header.</param>
        /// <returns><see cref="HttpContent"/> if the request was successful, otherwise <see langword="null"/>.</returns>
        internal static async Task<HttpResponseMessage> GetAsync(string path, bool isFullPath = false, bool doAuth = true, bool getAnyway = false)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, isFullPath ? path : APIAddress + path);

                if (!doAuth)
                {
                    request.Headers.Authorization = null;
                }

                HttpResponseMessage response = await Http.SendAsync(request);
                request.Dispose();

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
        /// <param name="doAuth">Whether to include the Authorization header.</param>
        /// <returns><see langword="true"/> if the request was successful, otherwise <see langword="false"/>.</returns>
        internal static async Task<HttpResponseMessage> PostAsync(string path, object data, bool isFullPath = false, bool doAuth = true, bool getAnyway = false)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, isFullPath ? path : APIAddress + path);
                request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                
                if (!doAuth)
                {
                    request.Headers.Authorization = null;
                }

                Stopwatch.Restart();

                HttpResponseMessage response = await Http.SendAsync(request);
                request.Dispose();

                unchecked
                {
                    PingMS = (ushort)Stopwatch.ElapsedMilliseconds;
                }
                Stopwatch.Stop();

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

            if (!DataHelper.isLogin || DataHelper.PeroUid.IsNullOrWhitespace())
            {
                UIManager.WarnNotification(Localization.Get("Warning", "NoAccount"));
                return;
            }

            string uid = DataHelper.PeroUid;
            if (uid == null) return;

            TriedConnecting = true;
            PopupUtils.ShowInfo(Localization.Get("MainMenu", "Connecting"));
            Main.Logger.Msg("Connecting to the server...");

            object payload = code is null ? new { Uid = uid } : new { Uid = uid, Code = code };

            var response = await PostAsync($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/login", payload, true, code is null, true);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

                ServerVersion = content["Version"].GetString();
                if (Outdated)
                {
                    OutdatedWarning = new(string.Format(Localization.Get("Warning", "Outdated").ToString(), Constants.Version, ServerVersion));
                    Disconnect();
                    Main.Logger.Error("Outdated version of the mod, cannot proceed!");
                    return;
                }

                var newToken = content["Token"].GetString();
                if (newToken != null)
                {
                    Token = newToken;
                    File.WriteAllText(TokenPath, Token);
                }

                Connected = true;
                _ = Main.InitConnect();
                Main.Logger.Msg("Connected to the server successfully!");
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    UIManager.WarningChooseAction = LoginOption;
                    UIManager.WarnChooseNotification(Localization.Get("Warning", "LoginRequired"));
                }

                Main.Logger.Error("Failed to connect to the server!");
            }
        }

        private static void LoginOption(bool doLogin)
        {
            if (!doLogin) return;

            TriedConnecting = false;
            Utilities.OpenBrowserLink(Constants.DiscordAuthURL);
            UIManager.MainMenu.CodeWindow.Show();
        }

        private static void UpdateModOption(bool doUpdate)
        {
            if (!doUpdate) return;
            Utilities.OpenBrowserLink($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}:{Constants.PortHTTP}/home");
        }

        private static void ReconnectOption(bool doReconnect)
        {
            if (!doReconnect) return;
            TriedConnecting = false;
            UIManager.MainMenu.Open();
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
                UIManager.WarningChooseAction = UpdateModOption;
                UIManager.WarnChooseNotification(OutdatedWarning);
                return;
            }

            UIManager.WarningChooseAction = ReconnectOption;
            UIManager.WarnChooseNotification(Localization.Get("Warning", "Offline"));
        }

        internal static void Init()
        {
            HttpHandler = new HttpClientHandler();
            Http = new(HttpHandler);
            Http.DefaultRequestHeaders.ConnectionClose = false;
            Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Udp = new();
            WebSocket = new();

            if (File.Exists(TokenPath)) Token = File.ReadAllText(TokenPath);
        }
    }
}
