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
using Multiplayer.UI.Extensions;

namespace Multiplayer.Static
{
    internal static class Client
    {
        internal static bool Connected { get; private set; } = false;
        private static bool Debounce = false;

        private static Stopwatch Stopwatch = new Stopwatch();
        internal static ushort PingMS { get; private set; }

        internal static readonly string APIAddress = $"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/api/";
        internal static readonly Uri WebsocketAddress = new($"wss://{Constants.ServerAddress}:{Constants.PortWebsocket}/ws");

        internal static Version ServerVersion { get; private set; }
        internal static bool Outdated => ServerVersion != null && ServerVersion > Constants.Version_;
        private static LocalString OutdatedWarning;
        private static LocalString LowLevelWarning;

        private static HttpMessageHandler HttpHandler;
        private static HttpClient Http;

        private static UdpClient Udp;

        private static ClientWebSocket WebSocket;
        private static int WebsocketReconnectTries = 0;

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

        private static async Task WebsocketListen()
        {
            var buffer = new byte[4096];

            while (WebSocket.State == WebSocketState.Open && LobbyManager.IsInLobby)
            {
                try
                {
                    var msgBuilder = new StringBuilder();
                    WebSocketReceiveResult res = null;

                    do
                    {
                        res = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        unchecked
                        {
                            PingMS = Stopwatch.ElapsedMilliseconds == 0 ? PingMS : (ushort)Stopwatch.ElapsedMilliseconds;
                        }
                        Stopwatch.Stop();

                        if (res.MessageType == WebSocketMessageType.Binary)
                        {
                            Main.Dispatch(() => BattleManager.Recieve(buffer.AsSpan(0, res.Count)));
                        }
                        else if (res.MessageType == WebSocketMessageType.Text)
                        {
                            msgBuilder.Append(Encoding.UTF8.GetString(buffer, 0, res.Count));
                        }
                        else if (res.MessageType == WebSocketMessageType.Close || !LobbyManager.IsInLobby)
                        {
                            await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                            return;
                        }
                    }
                    while (!res.EndOfMessage);

                    // Executing the recieved text message
                    if (msgBuilder.Length > 0)
                    {
                        var message = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(msgBuilder.ToString());
                        switch (message["Type"].GetString())
                        {
                            case "Sync":
                                _ = LobbyManager.LocalLobby.UpdateFields(JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message["Body"]), false, true);
                                break;
                            case "Chat":
                                Main.Dispatch(() => Chat.Recieve(JsonSerializer.Deserialize<ChatMessage>(message["Body"])));
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Main.Log(ex);
                }
            }
        }

        /// <summary>
        /// Connects to the server websocket and starts listening for incoming messages/lobby updates.
        /// </summary>
        internal static async Task WebsocketStart()
        {
            if (!LobbyManager.IsInLobby || WebSocket.State == WebSocketState.Open) return;

            WebSocket.Options.SetRequestHeader("Authorization", $"Basic {PlayerManager.LocalPlayerUid}#{Token}");

            Main.Log("Connecting to the Websocket...");

            while (WebsocketReconnectTries < Constants.WebsocketTryReconnectTimes)
            {
                try
                {
                    await WebSocket.ConnectAsync(WebsocketAddress, CancellationToken.None);
                    await Task.Delay(1000);

                    Main.Log("Websocket connection was successfully established at " + WebsocketAddress, Main.LogType.Success);
                    WebsocketReconnectTries = 0;

                    await WebsocketListen();

                    if (LobbyManager.IsInLobby)
                    {
                        throw new Exception();
                    }
                    else break;
                }
                catch (Exception)
                {
                    WebsocketReconnectTries++;
                    Main.Log($"Websocket connection was lost, reconnecting... (attempt {WebsocketReconnectTries})", Main.LogType.Warning);

                    if (WebsocketReconnectTries == 1)
                    {
                        Main.Dispatch(() => Chat.Recieve(new()
                        {
                            Message = Localization.Get("SystemChatMessages", "WebsocketReconnect").ToString(),
                            AuthorName = "system"
                        }));
                    }

                    await Task.Delay(Constants.WebsocketReconnectAfterMS);
                }
            }

            if (LobbyManager.IsInLobby)
            {
                Main.Log("Websocket connection was lost.", Main.LogType.Error);

                UIManager.Debounce = true;
                await LobbyManager.LeaveLobby(true);
                UIManager.Debounce = false;

                Main.Dispatch(() =>
                {
                    UIManager.WarnNotification(Localization.Get("Warning", "WebsocketFail"));
                });
            }
            else Main.Log("Websocket connection has ended.");

            WebsocketReconnectTries = 0;
        }

        /// <summary>
        /// Sends a websocket message to the server.
        /// </summary>
        /// <param name="payload">Body to send.</param>
        /// <param name="recordPing">(Optional) Whether to record ping when sending (Stopwatch will be stopped upon receiving).</param>
        internal static async Task WebsocketSend(object payload, bool recordPing = false)
        {
            if (WebSocket.State != WebSocketState.Open) return;

            byte[] bytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(payload));

            if (recordPing) Stopwatch.Restart();
            await WebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        internal static async Task WebsocketSend(byte[] bytes, bool recordPing = false)
        {
            if (WebSocket.State != WebSocketState.Open) return;

            if (recordPing) Stopwatch.Restart();
            await WebSocket.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        /// <summary>
        /// Performs an <see langword="async"/> GET request.
        /// </summary>
        /// <param name="path">Path of the request relative to the API endpoint.</param>
        /// <param name="isFullPath">(Optional) Makes the <paramref name="path">path</paramref> absolute if <see langword="true"/>.</param>
        /// <param name="getAnyway">(Optional) Will return the <see cref="HttpResponseMessage"/> regardless of it being unsuccessful.</param>
        /// <param name="doAuth">(Optional) Whether to include the Authorization header.</param>
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
                    Main.Log($"[{(int)response.StatusCode}] GET request failed: {await response.Content.ReadAsStringAsync()}", Main.LogType.Error);

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
                Main.Log($"GET request timeout at: {address}!", Main.LogType.Error);
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
        /// <param name="getAnyway">(Optional) Will return the <see cref="HttpResponseMessage"/> regardless of it being unsuccessful.</param>
        /// <param name="doAuth">(Optional) Whether to include the Authorization header.</param>
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
                    Main.Log($"[{(int)response.StatusCode}] POST request failed: {await response.Content.ReadAsStringAsync()}", Main.LogType.Error);

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
                
                Main.Log($"POST request timeout at {address}!", Main.LogType.Error);
                if (!isFullPath) Disconnect(true);
                return null;
            }
        }

        /// <summary>
        /// Awaits for the POST on the localhost with the exchange code, then tries to <see cref="Connect(string)"/>
        /// </summary>
        internal static async Task AwaitAndConnect()
        {
            if (Debounce) return;
            Debounce = true;

            var listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{Constants.PortHTTP}/");
            listener.Start();

            Main.Log("Awaiting for token...");

            Main.Dispatch(() => PnlCloudExtension.Start(Localization.Get("PnlCloudMessage", "Awaiting").ToString()));

            HttpListenerContext context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            var code = request.Url.Query.Split("=")[1];
            response.Redirect($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/authFinish");

            context.Response.Close();
            listener.Stop();

            Debounce = false;
            _ = Connect(code);
        }

        /// <summary>
        /// Attempts to log in the multiplayer server using the local token/code.
        /// </summary>
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

            Main.Dispatch(() => PnlCloudExtension.Start(Localization.Get("PnlCloudMessage", "Connecting").ToString()));
            Debounce = true;
            Main.Log("Connecting to the server...");

            object payload = code is null 
                ? new { Uid = uid, Name = DataHelper.nickname } 
                : new { Uid = uid, Name = DataHelper.nickname, Code = code };

            var response = await PostAsync($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/login", payload, true, code is null, true);
            Main.Dispatch(() => PnlCloudExtension.Finish(response.IsSuccessStatusCode));
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

                ServerVersion = new Version(content["Version"].GetString());
                if (!Outdated)
                {
                    var newToken = content["Token"].GetString();
                    if (newToken != null)
                    {
                        Token = newToken;
                        File.WriteAllText(TokenPath, Cipher.Encrypt(Token, Constants.TokenCipherShift));
                    }

                    Connected = true;
                    ChartManager.Init();
                    _ = Main.InitConnect();

                    Main.Log("Connected to the server successfully!", Main.LogType.Success);
                }
                else
                {
                    OutdatedWarning = new(string.Format(Localization.Get("Warning", "Outdated").ToString(), Constants.Version, ServerVersion));
                    UIManager.WarningChooseAction = UpdateModOption;
                    Main.Dispatch(() => UIManager.WarnChooseNotification(OutdatedWarning));
                    Main.Log("Outdated version of the mod, cannot proceed!", Main.LogType.Error);
                }
            } else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                UIManager.WarningChooseAction = LoginOption;
                Main.Dispatch(() => UIManager.WarnChooseNotification(Localization.Get("Warning", "LoginRequired")));
                Main.Log("Token has expired, login is required.", Main.LogType.Warning);
            }
            else
            {
                UIManager.WarningChooseAction = ReconnectOption;
                Main.Dispatch(() => UIManager.WarnChooseNotification(Localization.Get("Warning", "Offline")));
                Main.Log("Failed to connect to the server!", Main.LogType.Error);
            }
            Debounce = false;
        }

        /// <summary>
        /// Resets the connection.
        /// </summary>
        internal static void Disconnect(bool lost = false)
        {
            if (!Connected) return;

            if (LobbyManager.IsInLobby) _ = LobbyManager.LeaveLobby(true);

            Connected = false;
            Main.Log("Disconneced from the server.", Main.LogType.Warning);

            if (lost)
            {
                UIManager.WarningChooseAction = ReconnectOption;
                UIManager.WarnChooseNotification(Localization.Get("Warning", "LostConnection"));
            }
        }

        /// <summary>
        /// Opens the mod download page.
        /// </summary>
        private static void UpdateModOption(bool doUpdate)
        {
            if (!doUpdate) return;
            Utilities.OpenBrowserLink($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}:{Constants.PortHTTP}/home");
        }

        /// <summary>
        /// Opens the discord login page.
        /// </summary>
        private static void LoginOption(bool doLogin)
        {
            if (!doLogin || Debounce) return;
            Utilities.OpenBrowserLink(Constants.DiscordAuthURL);
            _ = AwaitAndConnect();
        }

        /// <summary>
        /// Tries to reconnect.
        /// </summary>
        private static void ReconnectOption(bool doReconnect)
        {
            if (!doReconnect) return;
            _ = Connect();
        }

        /// <summary>
        /// Initializes web and reads the token
        /// </summary>
        internal static void Init()
        {
            HttpHandler = new HttpClientHandler();
            Http = new(HttpHandler);
            Http.DefaultRequestHeaders.ConnectionClose = false;
            Http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Udp = new();
            Udp.Client.ReceiveTimeout = Constants.BattleUpdateTimeoutMS;

            WebSocket = new();

            if (File.Exists(TokenPath)) Token = Cipher.Decrypt(File.ReadAllText(TokenPath), Constants.TokenCipherShift);
        }
    }
}
