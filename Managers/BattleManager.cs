using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppFormulaBase;
using Multiplayer.Data.Players;
using Multiplayer.Data.Stats;
using Multiplayer.Static;
using System.Buffers.Binary;
using System.Text;

namespace Multiplayer.Managers
{
    internal static class BattleManager
    {
        internal static bool Synchronizing { get; private set; } = false;

        private static byte[] Datagram;
        private static byte[] ReceivedDatagram;

        private const int UidSize = 32;
        private const int TokenSize = 70;
        private const int BattleStatsSize = 21;
        private const int DatagramSize = 123;

        /* Datagram structure:
        
        32B - string UID
        70B - string Token
        4B - int Score
        4B - float Accuracy
        2B - ushort Perfects
        2B - ushort Greats
        2B - ushort Earlies
        2B - ushort Lates
        2B - ushort Misses
        1B - bool FC
        2B - ushort PingMS

        */

        internal static BattleStats BattleStats => PlayerManager.LocalPlayer?.BattleStats;
        private static TaskStageTarget TaskStageTarget;
        private static BattleRoleAttributeComponent BattleRoleAttributeComponent;
        private static StageBattleComponent StageBattleComponent;

        /// <summary>
        /// Sends datagrams to the server and recieves stats of other players.
        /// </summary>
        private static async Task<byte[]> ServerSync()
        {
            Span<byte> span = Datagram;

            Encoding.UTF8.GetBytes(BattleStats.Player.Uid, span.Slice(0, UidSize));
            Encoding.UTF8.GetBytes(Client.Token, span.Slice(UidSize, TokenSize));

            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(UidSize + TokenSize), BattleStats.Score);
            BinaryPrimitives.WriteSingleLittleEndian(span.Slice(UidSize + TokenSize + 4), BattleStats.Accuracy);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + TokenSize + 8), BattleStats.Perfects);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + TokenSize + 10), BattleStats.Greats);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + TokenSize + 12), BattleStats.Earlies);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + TokenSize + 14), BattleStats.Lates); 
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + TokenSize + 16), BattleStats.Misses);
            span[UidSize + TokenSize + 18] = (byte)(BattleStats.FC ? 1 : 0);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + TokenSize + 19), BattleStats.PingMS);

            return await Client.UdpSendAsync(Datagram);
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
            BattleStats.PingMS = Client.PingMS;
        }

        /// <summary>
        /// Updates battle stats of every other player in the lobby.
        /// </summary>
        private static void UpdateOthersBattleStats()
        {
            if (ReceivedDatagram is null) return;

            Span<byte> span = ReceivedDatagram;

            // Datagram size divided by UID + BattleStats size is the amount of entries
            for (int i = 0; i < ReceivedDatagram.Length/(UidSize + BattleStatsSize); i++)
            {
                int startAt = i * (UidSize + BattleStatsSize);
                string uid = Encoding.UTF8.GetString(span.Slice(startAt, UidSize));

                Player player = PlayerManager.GetCachedPlayer(uid);
                if (player is null) continue;
                BattleStats battleStats = player.BattleStats;
                if (battleStats is null) continue;

                battleStats.Score = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(startAt + UidSize));
                battleStats.Accuracy = BinaryPrimitives.ReadSingleLittleEndian(span.Slice(startAt + UidSize + 4));
                battleStats.Perfects = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt + UidSize + 8));
                battleStats.Greats = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt + UidSize + 10));
                battleStats.Earlies = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt + UidSize + 12));
                battleStats.Lates = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt + UidSize + 14));
                battleStats.Misses = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt + UidSize + 16));
                battleStats.FC = span[startAt + UidSize + 18] != 0;
                battleStats.PingMS = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt + UidSize + 19));
            }
        }

        /// <summary>
        /// Runs an async loop and handles data sending/recieving.
        /// </summary>
        private static async Task StartSyncLoop()
        {
            Synchronizing = true;
            while (Synchronizing && Client.Connected)
            {
                Main.Dispatcher.Enqueue(() =>
                {
                    UpdateLocalBattleStats();
                    UpdateOthersBattleStats();

                    UIManager.BattleLobbyDisplay.Update();
                });

                ReceivedDatagram = await ServerSync();
                await Task.Delay(Constants.BattleSyncInterval);
            }
        }

        /// <summary>
        /// Starts synchronizing with the server.
        /// </summary>
        internal static void BattleSyncStart()
        {
            if (Synchronizing || !LobbyManager.IsInLobby || PlayerManager.LocalPlayer is null) return;

            TaskStageTarget = TaskStageTarget.instance;
            BattleRoleAttributeComponent = BattleRoleAttributeComponent.instance;
            StageBattleComponent = StageBattleComponent.instance;

            _ = StartSyncLoop();
            Main.Logger.Msg("Battle synchronization started!");
        }

        /// <summary>
        /// Ends synchronizing with the server.
        /// </summary>
        internal static void BattleSyncStop()
        {
            Synchronizing = false;

            foreach (string playerUid in LobbyManager.LocalLobby.Players)
            {
                Player player = PlayerManager.GetCachedPlayer(playerUid);
                if (player == null) continue;
                player.BattleStats.Reset();
            }

            Main.Logger.Msg("Battle synchronization ended!");
        }

        internal static void Init()
        {
            Datagram = new byte[DatagramSize];
        }
    }
}
