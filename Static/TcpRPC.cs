using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Multiplayer.Static
{
    internal static class TcpRPC
    {
        internal static TcpClient Client;
        internal static NetworkStream Stream;
        private static SemaphoreSlim WriteLock = new(1, 1);
        private static ConcurrentDictionary<string, TaskCompletionSource<string>> PendingRequests = new();

        internal static async Task ConnectAsync(string ip, int port)
        {
            Client = new TcpClient();
            await Client.ConnectAsync(ip, port);
            Stream = Client.GetStream();
            _ = Task.Run(ReceiveLoop);
        }

        internal static void Disconnect()
        {
            Client?.Close();
            Client = null;
            Stream = null;
            foreach (var tcs in PendingRequests.Values) tcs.TrySetResult(null);
            PendingRequests.Clear();
        }

        internal static async Task<string> PostAsync(string path, object data)
        {
            if (Client == null || !Client.Connected) return null;
            
            var reqId = Guid.NewGuid().ToString("N");
            var payload = new
            {
                ReqId = reqId,
                Path = path,
                Data = data
            };
            
            var json = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            var lengthBytes = BitConverter.GetBytes(bytes.Length);
            
            var tcs = new TaskCompletionSource<string>();
            PendingRequests[reqId] = tcs;
            
            try
            {
                await WriteLock.WaitAsync();
                try
                {
                    await Stream.WriteAsync(lengthBytes, 0, 4);
                    await Stream.WriteAsync(bytes, 0, bytes.Length);
                }
                finally
                {
                    WriteLock.Release();
                }
            }
            catch
            {
                PendingRequests.TryRemove(reqId, out _);
                return null;
            }
            
            var result = await Task.WhenAny(tcs.Task, Task.Delay(15000));
            if (result == tcs.Task) return await tcs.Task;
            
            PendingRequests.TryRemove(reqId, out _);
            return null; // Timeout
        }

        // 即发即忘, 不等待响应
        internal static async Task SendAsync(string path, object data)
        {
            if (Client == null || !Client.Connected) return;

            var payload = new { Path = path, Data = data };
            var json = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            var lengthBytes = BitConverter.GetBytes(bytes.Length);

            try
            {
                await WriteLock.WaitAsync();
                try
                {
                    await Stream.WriteAsync(lengthBytes, 0, 4);
                    await Stream.WriteAsync(bytes, 0, bytes.Length);
                }
                finally
                {
                    WriteLock.Release();
                }
            }
            catch { }
        }

        private static async Task ReceiveLoop()
        {
            var header = new byte[4];
            while (Client != null && Client.Connected)
            {
                try
                {
                    int read = 0;
                    while (read < 4)
                    {
                        int r = await Stream.ReadAsync(header, read, 4 - read);
                        if (r == 0) return;
                        read += r;
                    }
                    
                    int length = BitConverter.ToInt32(header, 0);
                    if (length <= 0) return;
                    var body = new byte[length];
                    read = 0;
                    while (read < length)
                    {
                        int r = await Stream.ReadAsync(body, read, length - read);
                        if (r == 0) return;
                        read += r;
                    }
                    
                    var json = Encoding.UTF8.GetString(body);
                    var doc = JsonDocument.Parse(json);
                    
                    if (doc.RootElement.TryGetProperty("ReqId", out var reqIdProp) && reqIdProp.ValueKind == JsonValueKind.String)
                    {
                        var reqId = reqIdProp.GetString();
                        if (PendingRequests.TryRemove(reqId, out var tcs))
                        {
                            tcs.TrySetResult(json);
                        }
                    }
                    else if (doc.RootElement.TryGetProperty("Type", out var typeProp))
                    {
                        var type = typeProp.GetString();
                        if (type == "Sync")
                        {
                            var bodyObj = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, JsonElement>>(doc.RootElement.GetProperty("Body").GetRawText());
                            _ = Managers.LobbyManager.LocalLobby.UpdateFields(bodyObj, false, false);
                        }
                        else if (type == "Chat")
                        {
                            var chatMsg = JsonSerializer.Deserialize<Data.Chat.ChatMessage>(doc.RootElement.GetProperty("Body").GetRawText());
                            Main.Dispatch(() => Chat.Recieve(chatMsg));
                        }
                        else if (type == "Battle")
                        {
                            var base64 = doc.RootElement.GetProperty("Body").GetString();
                            var battleBytes = Convert.FromBase64String(base64);
                            Main.Dispatch(() => Managers.BattleManager.Recieve(battleBytes));
                        }
                    }
                }
                catch
                {
                    break;
                }
            }
            Disconnect();
            if (Managers.LobbyManager.IsInLobby)
            {
                Main.Dispatch(() => Managers.UIManager.WarnNotification(Localization.Get("Warning", "WebsocketFail")));
                _ = Managers.LobbyManager.LeaveLobby(true);
            }
        }
    }
}
