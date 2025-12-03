using DiscordRPC;
using Il2CppAssets.Scripts.Database;
using Multiplayer.Data;
using Multiplayer.Data.Lobbies;

namespace Multiplayer.Managers
{
    internal static class DiscordManager
    {
        private const string DiscordAppID = "1436371970206728301";
        private const string MuseDashSteamID = "774171";

        private static DiscordRpcClient Client;
        internal static User CurrentUser => Client?.CurrentUser;
        internal static bool Initialized => Client is null ? false : Client.IsInitialized;
        private static ulong StartTime;

        private static Dictionary<RPCState, RichPresence> RPCs = new()
        {
            [RPCState.Idle] = new() { State = "Idle" },
            [RPCState.InLobby] = new() { State = "In Lobby" },
            [RPCState.InPrivateLobby] = new() { State = "In Private Lobby" },
            [RPCState.PlayingSolo] = new(),
            [RPCState.PlayingFriends] = new(),
            [RPCState.PlayingQueued] = new(),
        };
        private static RichPresence CurrentRPC => Client?.CurrentPresence;
        private static RPCState CurrentRPCState;

        /// <summary>
        /// Updates the current <see cref="RichPresence"/> according to the local player's actions.
        /// </summary>
        internal static void UpdateRPC()
        {
            if (!Initialized) return;
            Lobby localLobby = LobbyManager.LocalLobby;
            switch (CurrentRPCState)
            {
                case RPCState.Idle:
                    CurrentRPC.Details = string.Empty;
                    break;
                case RPCState.InLobby:
                    CurrentRPC.Party.ID = localLobby.Id.ToString();
                    CurrentRPC.Details = localLobby.Name;
                    CurrentRPC.Party.Size = localLobby.Players.Count;
                    CurrentRPC.Party.Max = localLobby.MaxPlayers;
                    break;
                case RPCState.InPrivateLobby:
                    CurrentRPC.Party.ID = localLobby.Id.ToString();
                    CurrentRPC.Details = localLobby.Name;
                    CurrentRPC.Party.Size = localLobby.Players.Count;
                    CurrentRPC.Party.Max = localLobby.MaxPlayers;
                    break;
                case RPCState.PlayingSolo:
                    CurrentRPC.State = $"Playing {GlobalDataBase.dbBattleStage.m_MusicLevel}★ {BattleHelper.MusicInfo().name}";
                    break;
                case RPCState.PlayingFriends:
                    CurrentRPC.State = $"Playing {GlobalDataBase.dbBattleStage.m_MusicLevel}★ {BattleHelper.MusicInfo().name}";
                    CurrentRPC.Details = localLobby.Name;
                    CurrentRPC.Party.Size = localLobby.Players.Count;
                    CurrentRPC.Party.Max = localLobby.MaxPlayers;
                    break;
                case RPCState.PlayingQueued:
                    CurrentRPC.State = $"Playing {DataHelper.selectedDifficulty}★ {BattleHelper.MusicInfo().name}";
                    CurrentRPC.Details = ""; // Put either ranked or casual
                    CurrentRPC.Party.Size = localLobby.Players.Count;
                    CurrentRPC.Party.Max = localLobby.MaxPlayers;
                    break;
            }
        }

        /// <summary>
        /// Sets the current RPC to the given <see cref="RPCState"/>.
        /// </summary>
        internal static void SetRPC(RPCState rpcState = RPCState.Idle)
        {
            if (!Initialized) return;
            CurrentRPCState = rpcState;

            var rpc = RPCs.GetValueOrDefault(rpcState);
            rpc.Timestamps = new()
            {
                StartUnixMilliseconds = StartTime
            };

            Client.SetPresence(rpc);
            UpdateRPC();
        }

        internal static void Init()
        {
            Client = new DiscordRpcClient(DiscordAppID);
            Client.RegisterUriScheme(MuseDashSteamID);
            Client.Initialize();
            StartTime = (ulong)new DateTimeOffset(DateTime.UtcNow.ToUniversalTime()).ToUnixTimeMilliseconds();
            SetRPC();
        }

        internal static void Dispose()
        {
            Client?.Dispose();
        }
    }
}
