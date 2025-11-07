using DiscordRPC;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Data;

namespace Multiplayer.Managers
{
    internal static class DiscordManager
    {
        internal static DiscordRpcClient Client { get; private set; }
        internal static User CurrentUser => Client?.CurrentUser;
        internal static bool Initialized => Client is null ? false : Client.IsInitialized;
        internal static ulong StartTime { get; private set; }

        internal static Dictionary<RPCState, RichPresence> RPCs = new()
        {
            [RPCState.Idle] = new(),
            [RPCState.InLobby] = new(),
            [RPCState.InPrivateLobby] = new(),
            [RPCState.PlayingSolo] = new(),
            [RPCState.PlayingFriends] = new(),
            [RPCState.PlayingQueued] = new(),
        };
        internal static RichPresence CurrentRPC => Client?.CurrentPresence;

        internal static void UpdateRPC(RPCState rpcState)
        {
            Lobby localLobby = LobbyManager.LocalLobby;
            switch (rpcState)
            {
                case RPCState.Idle:
                    CurrentRPC.State = "Idle";
                    CurrentRPC.Details = string.Empty;
                    break;
                case RPCState.InLobby:
                    CurrentRPC.State = "In Lobby";
                    CurrentRPC.Party.ID = localLobby.Id.ToString();
                    CurrentRPC.Details = localLobby.Name;
                    CurrentRPC.Party.Size = localLobby.PlayerUids.Count;
                    CurrentRPC.Party.Max = localLobby.MaxPlayers;
                    CurrentRPC.Secrets.JoinSecret = localLobby.Hash;
                    break;
                case RPCState.InPrivateLobby:
                    CurrentRPC.State = "In Private Lobby";
                    CurrentRPC.Party.ID = localLobby.Id.ToString();
                    CurrentRPC.Details = localLobby.Name;
                    CurrentRPC.Party.Size = localLobby.PlayerUids.Count;
                    CurrentRPC.Party.Max = localLobby.MaxPlayers;
                    break;
                case RPCState.PlayingSolo:
                    CurrentRPC.State = $"Playing {DataHelper.selectedDifficulty}★ {BattleHelper.MusicInfo().name}";
                    break;
                case RPCState.PlayingFriends:
                    CurrentRPC.State = $"Playing {DataHelper.selectedDifficulty}★ {BattleHelper.MusicInfo().name}";
                    CurrentRPC.Details = localLobby.Name;
                    CurrentRPC.Party.Size = localLobby.PlayerUids.Count;
                    CurrentRPC.Party.Max = localLobby.MaxPlayers;
                    break;
                case RPCState.PlayingQueued:
                    CurrentRPC.State = $"Playing {DataHelper.selectedDifficulty}★ {BattleHelper.MusicInfo().name}";
                    CurrentRPC.Details = ""; // Put either ranked or casual
                    CurrentRPC.Party.Size = localLobby.PlayerUids.Count;
                    CurrentRPC.Party.Max = localLobby.MaxPlayers;
                    break;
            }
        }

        internal static void SetRPC(RPCState rpcState = RPCState.Idle)
        {
            var rpc = RPCs.GetValueOrDefault(rpcState);
            rpc.Timestamps = new()
            {
                StartUnixMilliseconds = StartTime
            };

            Client.SetPresence(rpc);
            UpdateRPC(rpcState);
        }

        internal static void Init()
        {
            Client = new DiscordRpcClient("1436371970206728301");
            Client.RegisterUriScheme("774171");
            Client.Initialize();
            //Client.Subscribe(EventType.Join);
            //Client.OnJoin += OnJoin;
            StartTime = (ulong)new DateTimeOffset(DateTime.UtcNow.ToUniversalTime()).ToUnixTimeMilliseconds();
            SetRPC();
        }

        /*internal static void OnJoin(object sender, DiscordRPC.Message.JoinMessage args)
        {
            
        }*/

        internal static void Dispose()
        {
            Client?.Dispose();
        }
    }
}
