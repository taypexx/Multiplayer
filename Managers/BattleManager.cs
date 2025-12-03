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

        internal static BattleStats BattleStats => PlayerManager.LocalPlayer?.BattleStats;
        private static TaskStageTarget TaskStageTarget;
        private static BattleRoleAttributeComponent BattleRoleAttributeComponent;
        private static StageBattleComponent StageBattleComponent;

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
        }

        /// <summary>
        /// Updates battle stats of every other player in the lobby.
        /// Recieved datagram structure: 19B BattleStats, 32B Uid. Total 51 bytes for each player of the lobby.
        /// </summary>
        private static void UpdateOthersBattleStats()
        {
            if (ReceivedDatagram is null) return;

            Span<byte> span = ReceivedDatagram;

            for (int i = 0; i < ReceivedDatagram.Length/51; i++)
            {
                int startAt = i * 51;
                string uid = Encoding.UTF8.GetString(span.Slice(startAt + 19, 32));

                Player player = PlayerManager.GetCachedPlayer(uid);
                if (player is null) continue;
                BattleStats battleStats = player.BattleStats;
                if (battleStats is null) continue;

                battleStats.Score = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(startAt));
                battleStats.Accuracy = BinaryPrimitives.ReadSingleLittleEndian(span.Slice(startAt+4));
                battleStats.FC = span[8] != 0;
                battleStats.Perfects = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt+9));
                battleStats.Greats = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt+11));
                battleStats.Earlies = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt+13));
                battleStats.Lates = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt+15));
                battleStats.Misses = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt+17));
            }
        }

        /// <summary>
        /// Runs an async loop and handles data sending/recieving.
        /// </summary>
        /// <returns></returns>
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
            Datagram = new byte[91];
        }
    }
}
