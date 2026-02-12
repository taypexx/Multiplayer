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

        private const int UidSize = 32;
        private const int BattleStatsSize = 22;

        /* Packet structure:
        
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

        */

        internal static BattleStats BattleStats => PlayerManager.LocalPlayer?.BattleStats;
        private static TaskStageTarget TaskStageTarget;
        private static BattleRoleAttributeComponent BattleRoleAttributeComponent;
        private static StageBattleComponent StageBattleComponent;

        /// <summary>
        /// Updates <see cref="Data.Stats.BattleStats"/> of every other <see cref="Player"/> in the lobby.
        /// </summary>
        internal static void Recieve(Span<byte> packet)
        {
            // Datagram size divided by UID + BattleStats size is the amount of entries
            for (int i = 0; i < packet.Length / (UidSize + BattleStatsSize); i++)
            {
                int startAt = i * (UidSize + BattleStatsSize);
                string uid = Encoding.UTF8.GetString(packet.Slice(startAt, UidSize)).TrimEnd('\0'); // Trimming because of lua null padding (not necessary)

                Player player = PlayerManager.GetCachedPlayer(uid);
                if (player is null) continue;
                BattleStats battleStats = player.BattleStats;
                if (battleStats is null) continue;

                battleStats.Score = BinaryPrimitives.ReadUInt32LittleEndian(packet.Slice(startAt + UidSize));
                battleStats.Accuracy = BinaryPrimitives.ReadSingleLittleEndian(packet.Slice(startAt + UidSize + 4));
                battleStats.Perfects = BinaryPrimitives.ReadUInt16LittleEndian(packet.Slice(startAt + UidSize + 8));
                battleStats.Greats = BinaryPrimitives.ReadUInt16LittleEndian(packet.Slice(startAt + UidSize + 10));
                battleStats.Earlies = BinaryPrimitives.ReadUInt16LittleEndian(packet.Slice(startAt + UidSize + 12));
                battleStats.Lates = BinaryPrimitives.ReadUInt16LittleEndian(packet.Slice(startAt + UidSize + 14));
                battleStats.Misses = BinaryPrimitives.ReadUInt16LittleEndian(packet.Slice(startAt + UidSize + 16));
                battleStats.FC = packet[startAt + UidSize + 18] != 0;
                battleStats.Alive = packet[startAt + UidSize + 19] != 0;
                player.PingMS = BinaryPrimitives.ReadUInt16LittleEndian(packet.Slice(startAt + UidSize + 20));
            }

            UIManager.BattleLobbyDisplay.Update();
        }

        /// <summary>
        /// Sends a packet to the server containing <see cref="Data.Stats.BattleStats"/> of the local player.
        /// </summary>
        private static void Send()
        {
            // This shit needs to be calculated properly bruh
            int num1 = TaskStageTarget.GetHitCountByResult(2u)
                + TaskStageTarget.GetHitCountByResult(4u)
                + TaskStageTarget.GetHitCountByResult(5u)
                + TaskStageTarget.m_Block + TaskStageTarget.m_MusicCount + TaskStageTarget.m_EnergyCount;
            int num2 = TaskStageTarget.GetHitCountByResult(3u);
            int num3 = TaskStageTarget.GetHitCountByResult(1u);
            float accuracy = (float)((num1 + num2 / 2.0) / (num1 + num2 + num3) * 100.0);

            // Updating battle stats
            BattleStats.Score = (uint)TaskStageTarget.GetScore();
            BattleStats.Accuracy = accuracy;
            BattleStats.FC = TaskStageTarget.IsFullCombo();
            BattleStats.Alive = !BattleRoleAttributeComponent.IsDead();
            BattleStats.Perfects = (ushort)TaskStageTarget.m_PerfectResult;
            BattleStats.Greats = (ushort)TaskStageTarget.m_GreatResult;
            BattleStats.Earlies = (ushort)BattleRoleAttributeComponent.early;
            BattleStats.Lates = (ushort)BattleRoleAttributeComponent.late;
            BattleStats.Misses = (ushort)TaskStageTarget.GetComboMiss();
            BattleStats.Player.PingMS = Client.PingMS;

            // Sending battle stats
            var packet = new byte[UidSize + BattleStatsSize];
            Span<byte> span = packet;

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

            _ = Client.WebsocketSend(packet);
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

            Synchronizing = true;
            Main.Log("Battle synchronization started!");

            while (Synchronizing && Client.Connected)
            {
                try
                {
                    Main.Dispatch(Send);
                }
                catch (Exception ex)
                {
                    Main.Log(ex);
                }
                await Task.Delay(Settings.Config.BattleUpdateIntervalMS);
            }
        }

        /// <summary>
        /// Stops sending/recieving data to/from the server.
        /// </summary>
        internal static void SyncStop()
        {
            Synchronizing = false;
            Main.Log("Battle synchronization ended!");
        }
    }
}
