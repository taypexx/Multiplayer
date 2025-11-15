using Il2CppAssets.Scripts.GameCore.HostComponent;
using Multiplayer.Data.Stats;
using System.Buffers.Binary;
using System.Text;

namespace Multiplayer.Managers
{
    internal static class BattleManager
    {
        internal static CancellationTokenSource CancellationTokenSource { get; private set; }
        private static TimeSpan SyncInterval = TimeSpan.FromMilliseconds(300);

        private static BattleStats BattleStats => PlayerManager.LocalPlayer?.BattleStats;
        private static TaskStageTarget TaskStageTarget;
        private static BattleRoleAttributeComponent BattleRoleAttributeComponent;
        private static byte[] Datagram;
        private static byte[] ReceivedDatagram;

        /// <summary>
        /// Sends datagrams to the server and recieves stats of other players.
        /// Datagram structure: 4B score, 4B acc, 1B FC, 2B perfects, 2B greats, 2B Earlies, 2B Lates, 2B Misses, 32B Uid, 40B Token. Total 91 bytes.
        /// </summary>
        /// <returns></returns>
        private static async Task<byte[]> ServerSync()
        {
            Span<byte> span = Datagram;

            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(0), BattleStats.Score);
            BinaryPrimitives.WriteSingleLittleEndian(span.Slice(4), BattleStats.Accuracy);
            span[8] = (byte)(BattleStats.FC ? 1 : 0);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(9), BattleStats.Perfects);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(11), BattleStats.Greats);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(13), BattleStats.Earlies);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(15), BattleStats.Lates); 
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(17), BattleStats.Misses);
            Encoding.UTF8.GetBytes(BattleStats.Player.Uid, span.Slice(19, 32));
            Encoding.UTF8.GetBytes(Client.Token, span.Slice(51, 40));

            return await Client.UdpSendAsync(Datagram, CancellationTokenSource.Token);
        }

        /// <summary>
        /// Gets local battle information and updates <see cref="Data.Stats.BattleStats"/> of the local player.
        /// </summary>
        private static void UpdateLocalBattleStats()
        {
            int num1 = TaskStageTarget.GetHitCountByResult(2u) 
                + TaskStageTarget.GetHitCountByResult(4u) 
                + TaskStageTarget.GetHitCountByResult(5u) 
                + TaskStageTarget.m_Block + TaskStageTarget.m_MusicCount + TaskStageTarget.m_EnergyCount;
            int num2 = TaskStageTarget.GetHitCountByResult(3u);
            int num3 = TaskStageTarget.GetHitCountByResult(1u);

            BattleStats.Score = (uint)TaskStageTarget.m_Score;
            BattleStats.Accuracy = (float)((num1 + num2 / 2.0) / (num1 + num2 + num3) * 100.0);
            BattleStats.FC = TaskStageTarget.IsFullCombo();
            BattleStats.Perfects = (ushort)TaskStageTarget.m_PerfectResult;
            BattleStats.Greats = (ushort)TaskStageTarget.m_GreatResult;
            BattleStats.Earlies = (ushort)BattleRoleAttributeComponent.early;
            BattleStats.Lates = (ushort)BattleRoleAttributeComponent.late;
            BattleStats.Misses = (ushort)TaskStageTarget.GetComboMiss();
        }

        /// <summary>
        /// Updates battle stats of every other player in the lobby.
        /// </summary>
        private static void UpdateOthersBattleStats()
        {
            if (ReceivedDatagram is null) return;
            // Decode the RecievedDatagram, loop through local lobby and update each player's battlestats with the provided by the datagram
        }

        /// <summary>
        /// Runs an async loop and handles data sending/recieving.
        /// </summary>
        /// <returns></returns>
        private static async Task StartSyncLoop()
        {
            while (true)
            {
                try
                {
                    Main.Dispatcher.Enqueue(() =>
                    {
                        UpdateLocalBattleStats();
                        UpdateOthersBattleStats();
                    });

                    ReceivedDatagram = await ServerSync();
                    await Task.Delay(SyncInterval, CancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    CancellationTokenSource.Dispose();
                    CancellationTokenSource = null;
                    return;
                }
            }
        }

        /// <summary>
        /// Starts synchronizing with the server.
        /// </summary>
        internal static void BattleSyncStart()
        {
            if (CancellationTokenSource is not null || LobbyManager.LocalLobby is null || PlayerManager.LocalPlayer is null) return;

            TaskStageTarget = TaskStageTarget.instance;
            BattleRoleAttributeComponent = BattleRoleAttributeComponent.instance;

            CancellationTokenSource = new();
            _ = StartSyncLoop();
            Main.Logger.Msg("Battle synchronization started!");
        }

        /// <summary>
        /// Ends synchronizing with the server.
        /// </summary>
        internal static void BattleSyncStop()
        {
            CancellationTokenSource?.Cancel();
            Main.Logger.Msg("Battle synchronization ended!");
        }

        internal static void Init()
        {
            Datagram = new byte[91];
        }
    }
}
