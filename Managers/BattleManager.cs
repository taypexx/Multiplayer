using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.GameCore.HostComponent;
using Il2CppAssets.Scripts.PeroTools.Commons;
using Multiplayer.Data.Stats;
using System.Text;

namespace Multiplayer.Managers
{
    internal static class BattleManager
    {
        internal static bool Synchronizing { get; private set; } = false;
        private static TimeSpan SyncInterval = TimeSpan.FromMilliseconds(300);

        /// <summary>
        /// Sends datagrams to the server and recieving the current battle data.
        /// Datagram structure: 4B score, 4B acc, 1B FC, 2B perfects, 2B greats, 2B Earlies, 2B Lates, 2B Misses, 32B Uid, 40B Token. Total 91 bytes.
        /// </summary>
        /// <returns></returns>
        private static async Task ServerSync()
        {
            BattleStats stats = PlayerManager.LocalPlayer.BattleStats;
            TaskStageTarget task = TaskStageTarget.instance;
            BattleRoleAttributeComponent role = BattleRoleAttributeComponent.instance;
            byte[] datagram = new byte[91];

            while (Synchronizing)
            {
                int num1 = task.GetHitCountByResult(2u) + task.GetHitCountByResult(4u) + task.GetHitCountByResult(5u) + task.m_Block + task.m_MusicCount + task.m_EnergyCount;
                int num2 = task.GetHitCountByResult(3u);
                int num3 = task.GetHitCountByResult(1u);

                stats.Score = (uint)task.m_Score;
                stats.Accuracy = (float)((num1 + num2 / 2.0) / (num1 + num2 + num3) * 100.0);
                stats.FC = task.IsFullCombo();
                stats.Perfects = (ushort)task.m_PerfectResult;
                stats.Greats = (ushort)task.m_GreatResult;
                stats.Earlies = (ushort)role.early;
                stats.Lates = (ushort)role.late;
                stats.Misses = (ushort)task.GetComboMiss();

                Array.Copy(BitConverter.GetBytes(stats.Score), 0, datagram, 0, 4);
                Array.Copy(BitConverter.GetBytes(stats.Accuracy), 0, datagram, 4, 4);
                Array.Copy(BitConverter.GetBytes(stats.FC), 0, datagram, 8, 1);
                Array.Copy(BitConverter.GetBytes(stats.Perfects), 0, datagram, 9, 2);
                Array.Copy(BitConverter.GetBytes(stats.Greats), 0, datagram, 11, 2);
                Array.Copy(BitConverter.GetBytes(stats.Earlies), 0, datagram, 13, 2);
                Array.Copy(BitConverter.GetBytes(stats.Lates), 0, datagram, 15, 2);
                Array.Copy(BitConverter.GetBytes(stats.Misses), 0, datagram, 17, 2);
                Array.Copy(Encoding.UTF8.GetBytes(stats.Player.Uid), 0, datagram, 19, 32);
                Array.Copy(Encoding.UTF8.GetBytes(Client.Token), 0, datagram, 51, 40);

                byte[] response = await Client.UdpSendAsync(datagram);

                await Task.Delay(SyncInterval);
            }
        }

        /// <summary>
        /// Starts synchronizing with the server.
        /// </summary>
        internal static void BattleSyncStart()
        {
            if (Synchronizing || LobbyManager.LocalLobby is null || PlayerManager.LocalPlayer is null) return;

            Synchronizing = true;
            _ = ServerSync();
            Main.Logger.Msg("Battle synchronization started!");
        }

        /// <summary>
        /// Ends synchronizing with the server.
        /// </summary>
        internal static void BattleSyncStop()
        {
            Synchronizing = false;
            Main.Logger.Msg("Battle synchronization ended!");
        }

        internal static void Init()
        {

        }
    }
}
