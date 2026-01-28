using Multiplayer.Managers;
using LocalizeLib;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Net.Http.Headers;
using System.Diagnostics;
using Il2CppAssets.Scripts.Database;
using Il2CppSirenix.Serialization.Utilities;
using MelonLoader.Utils;
using System.Net;
using System.Net.WebSockets;
using Multiplayer.Data.Chat;
using UnityEngine;
using System.Linq.Expressions;

namespace Multiplayer.Static
{
    internal static class Client
    {
        internal static bool Connected { get; private set; } = false;
        private static bool Debounce = false;

        private static Stopwatch Stopwatch = new Stopwatch();
        internal static ushort PingMS { get; private set; }

        internal static readonly string APIAddress = $"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/api/";
        internal static readonly Uri WebsocketAddress = new($"wss://{Constants.ServerAddress}/ws");

        internal static string ServerVersion { get; private set; }
        internal static bool Outdated => ServerVersion != null && ServerVersion != Constants.Version;
        private static LocalString OutdatedWarning;
        private static LocalString LowLevelWarning;

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

        internal static async Task WebsocketListen()
        {
            if (!LobbyManager.IsInLobby || WebSocket.State == WebSocketState.Open) return;

            WebSocket.Options.SetRequestHeader("Authorization", $"Basic {PlayerManager.LocalPlayerUid}#{Token}");

            Main.Logger.Msg("Connecting to the Websocket...");
            await WebSocket.ConnectAsync(WebsocketAddress, CancellationToken.None);
            Main.Logger.Msg(ConsoleColor.Green, "Websocket connection was successfully established at " + WebsocketAddress);

            var buffer = new byte[4096];
            while (WebSocket.State == WebSocketState.Open && LobbyManager.IsInLobby)
            {
                var msgBuilder = new StringBuilder();
                WebSocketReceiveResult res = null;

                do {
                    res = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    unchecked
                    {
                        PingMS = Stopwatch.ElapsedMilliseconds == 0 ? PingMS : (ushort)Stopwatch.ElapsedMilliseconds;
                    }
                    Stopwatch.Stop();

                    if (res.MessageType == WebSocketMessageType.Close || !LobbyManager.IsInLobby)
                    {
                        await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        goto End;
                    }
                    else if (res.MessageType != WebSocketMessageType.Text) continue;

                    msgBuilder.Append(Encoding.UTF8.GetString(buffer, 0, res.Count));
                } 
                while (!res.EndOfMessage);

                var message = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(msgBuilder.ToString());
                switch (message["Type"].GetString())
                {
                    case "Sync":
                        _ = LobbyManager.LocalLobby.UpdateFields(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message["Body"]), false, true);
                        break;
                    case "Chat":
                        Main.Dispatcher.Enqueue(() => Chat.Recieve(JsonSerializer.Deserialize<ChatMessage>(message["Body"])));
                        break;
                }
            }
            End:
            UIManager.Debounce = true;
            await LobbyManager.LeaveLobby(true);
            UIManager.Debounce = false;

            Main.Logger.Msg("Websocket connection ended.");
        }

        internal static async Task WebsocketSend(object payload, bool recordPing = false)
        {
            if (WebSocket.State != WebSocketState.Open) return;
            var bytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(payload));

            if (recordPing) Stopwatch.Restart();
            await WebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
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
            var address = isFullPath ? path : APIAddress + path;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, address);

                if (!doAuth)
                {
                    request.Headers.Authorization = null;
                }

                HttpResponseMessage response = await Http.SendAsync(request);
                request.Dispose();

                if (!response.IsSuccessStatusCode)
                {
                    Main.Logger.Error($"[{(int)response.StatusCode}] GET request failed: {await response.Content.ReadAsStringAsync()}");

                    if (!isFullPath && (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout))
                    {
                        Disconnect(true);
                    }

                    if (!getAnyway) return null;
                }
                return response;
            }
            catch (Exception)
            {
                Main.Logger.Error($"GET request timeout at: {address}!");
                if (!isFullPath) Disconnect(true);
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
            var address = isFullPath ? path : APIAddress + path;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, address);
                request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                
                if (!doAuth)
                {
                    request.Headers.Authorization = null;
                }

                HttpResponseMessage response = await Http.SendAsync(request);
                request.Dispose();

                if (!response.IsSuccessStatusCode)
                {
                    Main.Logger.Error($"[{(int)response.StatusCode}] POST request failed: {await response.Content.ReadAsStringAsync()}");

                    if (!isFullPath && (response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.GatewayTimeout))
                    {
                        Disconnect(true);
                    }

                    if (!getAnyway) return null;
                }
                return response;
            }
            catch (Exception)
            {
                
                Main.Logger.Error($"POST request timeout at {address}!");
                if (!isFullPath) Disconnect(true);
                return null;
            }
        }

        internal static async Task AwaitAndConnect()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{Constants.PortHTTP}/");
            listener.Start();

            Main.Logger.Msg("Awaiting for token...");

            Main.Dispatcher.Enqueue(UIManager.PnlCloudMessageStart);

            HttpListenerContext context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            var code = request.Url.Query.Split("=")[1];
            response.Redirect($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/authFinish");

            context.Response.Close();
            listener.Stop();

            _ = Connect(code);
        }

        internal static async Task Connect(string code = null)
        {
            if (Connected || Debounce) return;

            if (DataHelper.Level < Constants.ModUnlockLevel)
            {
                if (LowLevelWarning is null)
                {
                    LowLevelWarning = new(String.Format(Localization.Get("Warning", "LowLevel").ToString(), Constants.ModUnlockLevel));
                }
                UIManager.WarnNotification(LowLevelWarning);
                return;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                UIManager.WarnNotification(Localization.Get("Warning", "NoInternet"));
                return;
            }

            if (!DataHelper.isLogin || DataHelper.PeroUid.IsNullOrWhitespace())
            {
                UIManager.WarnNotification(Localization.Get("Warning", "NoAccount"));
                return;
            }

            string uid = DataHelper.PeroUid;
            if (uid == null) return;

            UIManager.PnlCloudMessageStart();
            Debounce = true;
            Main.Logger.Msg("Connecting to the server...");

            object payload = code is null 
                ? new { Uid = uid, Name = DataHelper.nickname } 
                : new { Uid = uid, Name = DataHelper.nickname, Code = code };

            var response = await PostAsync($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/login", payload, true, code is null, true);
            Main.Dispatcher.Enqueue(() => UIManager.PnlCloudMessageEnd(response.IsSuccessStatusCode));
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

                ServerVersion = content["Version"].GetString();
                if (!Outdated)
                {
                    var newToken = content["Token"].GetString();
                    if (newToken != null)
                    {
                        Token = newToken;
                        File.WriteAllText(TokenPath, Token);
                    }

                    Connected = true;
                    _ = Main.InitConnect();
                    Main.Logger.Msg(ConsoleColor.Green, "Connected to the server successfully!");
                }
                else
                {
                    OutdatedWarning = new(string.Format(Localization.Get("Warning", "Outdated").ToString(), Constants.Version, ServerVersion));
                    UIManager.WarningChooseAction = UpdateModOption;
                    Main.Dispatcher.Enqueue(() => UIManager.WarnChooseNotification(OutdatedWarning));
                    Main.Logger.Error("Outdated version of the mod, cannot proceed!");
                }
            } else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                UIManager.WarningChooseAction = LoginOption;
                Main.Dispatcher.Enqueue(() => UIManager.WarnChooseNotification(Localization.Get("Warning", "LoginRequired")));
                Main.Logger.Error("Token has expired, login is required.");
            }
            else
            {
                UIManager.WarningChooseAction = ReconnectOption;
                Main.Dispatcher.Enqueue(() => UIManager.WarnChooseNotification(Localization.Get("Warning", "Offline")));
                Main.Logger.Error("Failed to connect to the server!");
            }
            Debounce = false;
        }

        internal static void Disconnect(bool lost = false)
        {
            if (!Connected) return;

            if (LobbyManager.IsInLobby) _ = LobbyManager.LeaveLobby(true);

            Connected = false;
            Main.Logger.Msg("Disconneced from the server.");

            if (lost)
            {
                UIManager.WarningChooseAction = ReconnectOption;
                UIManager.WarnChooseNotification(Localization.Get("Warning", "LostConnection"));
            }
        }

        private static void UpdateModOption(bool doUpdate)
        {
            if (!doUpdate) return;
            Utilities.OpenBrowserLink($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}:{Constants.PortHTTP}/home");
        }

        private static void LoginOption(bool doLogin)
        {
            if (!doLogin) return;
            Utilities.OpenBrowserLink(Constants.DiscordAuthURL);
            _ = AwaitAndConnect();
        }

        private static void ReconnectOption(bool doReconnect)
        {
            if (!doReconnect) return;
            _ = Connect();
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
