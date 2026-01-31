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
        private const int BattleStatsSize = 22;

        /* Datagram structure:
        
        32B - string UID
        4B - int Score
        4B - float Accuracy
        2B - ushort Perfects
        2B - ushort Greats
        2B - ushort Earlies
        2B - ushort Lates
        2B - ushort Misses
        1B - bool FC
        1B - bool Alive
        2B - ushort PingMS
        50-70B - string Token

        */

        internal static BattleStats BattleStats => PlayerManager.LocalPlayer?.BattleStats;
        private static TaskStageTarget TaskStageTarget;
        private static BattleRoleAttributeComponent BattleRoleAttributeComponent;
        private static StageBattleComponent StageBattleComponent;

        /// <summary>
        /// Sends a datagram to the server and recieves <see cref="Data.Stats.BattleStats"/> of other players.
        /// </summary>
        private static async Task<byte[]> ServerSync()
        {
            Datagram = new byte[UidSize + BattleStatsSize + 2 + Client.Token.Length];
            Span<byte> span = Datagram;

            Encoding.UTF8.GetBytes(BattleStats.Player.Uid, span.Slice(0, UidSize));

            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(UidSize), BattleStats.Score);
            BinaryPrimitives.WriteSingleLittleEndian(span.Slice(UidSize + 4), BattleStats.Accuracy);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + 8), BattleStats.Perfects);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + 10), BattleStats.Greats);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + 12), BattleStats.Earlies);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + 14), BattleStats.Lates); 
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + 16), BattleStats.Misses);
            span[UidSize + 18] = (byte)(BattleStats.FC ? 1 : 0);
            span[UidSize + 19] = (byte)(BattleStats.Alive ? 1 : 0);
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + 20), BattleStats.Player.PingMS);

            // Signed string because the length is not fixed
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(UidSize + 22), (ushort)Client.Token.Length);
            Encoding.UTF8.GetBytes(Client.Token, span.Slice(UidSize + 24));

            return await Client.UdpSendAsync(Datagram);
        }

        /// <summary>
        /// Gets local battle information and updates <see cref="Data.Stats.BattleStats"/> of the local <see cref="Player"/>.
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
            BattleStats.Alive = !BattleRoleAttributeComponent.IsDead();
            BattleStats.Perfects = (ushort)TaskStageTarget.m_PerfectResult;
            BattleStats.Greats = (ushort)TaskStageTarget.m_GreatResult;
            BattleStats.Earlies = (ushort)BattleRoleAttributeComponent.early;
            BattleStats.Lates = (ushort)BattleRoleAttributeComponent.late;
            BattleStats.Misses = (ushort)TaskStageTarget.GetComboMiss();
            BattleStats.Player.PingMS = Client.PingMS;
        }

        /// <summary>
        /// Updates <see cref="Data.Stats.BattleStats"/> of every other <see cref="Player"/> in the lobby.
        /// </summary>
        private static void UpdateOthersBattleStats()
        {
            // Validating the datagram length
            if (ReceivedDatagram is null) return;
            if (ReceivedDatagram.Length % (UidSize + BattleStatsSize) != 0) return;

            Span<byte> span = ReceivedDatagram;

            // Datagram size divided by UID + BattleStats size is the amount of entries
            for (int i = 0; i < ReceivedDatagram.Length/(UidSize + BattleStatsSize); i++)
            {
                int startAt = i * (UidSize + BattleStatsSize);
                string uid = Encoding.UTF8.GetString(span.Slice(startAt, UidSize)).TrimEnd('\0'); // Trimming because of lua null padding (not necessary)

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
                battleStats.Alive = span[startAt + UidSize + 19] != 0;
                player.PingMS = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(startAt + UidSize + 20));
            }
        }

        /// <summary>
        /// Starts sending/recieving data to/from the server.
        /// </summary>
        internal static async Task SyncStart()
        {
            if (Synchronizing || !LobbyManager.IsInLobby || PlayerManager.LocalPlayer is null) return;

            TaskStageTarget = TaskStageTarget.instance;
            BattleRoleAttributeComponent = BattleRoleAttributeComponent.instance;
            StageBattleComponent = StageBattleComponent.instance;

            Main.Logger.Msg("Battle synchronization started!");
            Synchronizing = true;

            try
            {
                while (Synchronizing && Client.Connected)
                {
                    Main.Dispatcher.Enqueue(() =>
                    {
                        UpdateLocalBattleStats();
                        UpdateOthersBattleStats();

                        UIManager.BattleLobbyDisplay.Update();
                    });

                    ReceivedDatagram = await ServerSync();
                    await Task.Delay(Settings.Config.BattleUpdateIntervalMS);
                }
            }
            catch (Exception ex)
            {
                Main.Logger.Error(ex);
            }
        }

        /// <summary>
        /// Stops sending/recieving data to/from the server.
        /// </summary>
        internal static void SyncStop()
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
    }
}
