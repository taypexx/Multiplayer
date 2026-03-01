using Multiplayer.Data.Stats;
using Multiplayer.Managers;
using Multiplayer.Static;

namespace Multiplayer.Data.Players
{
    public class Player
    {
        public string Uid { get; private set; }
        public int HQUid { get; private set; }

        public MultiplayerStats MultiplayerStats { get; private set; }
        public BattleStats BattleStats { get; private set; }
        public HQStats HQStats { get; private set; }
        public MoeStats MoeStats { get; private set; }

        public ushort TotalRecords => (ushort)(HQStats.Records + MoeStats.Records);
        public ushort TotalAPs => (ushort)(HQStats.APs + MoeStats.APs);
        public float TotalAverageAccuracy => MoeStats.AverageAccuracy;//(HQStats.AverageAccuracy + MoeStats.AverageAccuracy) / 2;

        public ushort PingMS { 
            get {
                return this == PlayerManager.LocalPlayer ? Client.PingMS : field;
            } internal set; 
        }
        internal DateTime LastUpdated { get; private set; }

        internal Player(string uid, int hqUid = 0)
        {
            Uid = uid;
            HQUid = hqUid;
            MultiplayerStats = new(this);
            BattleStats = new(this);
            MoeStats = new(this);
            HQStats = new(this);
        }

        /// <returns><see langword="true"/> if the <paramref name="otherPlayer"/> is a friend of <see langword="this"/> <see cref="Player"/>, otherwise <see langword="false"/>.</returns>
        internal bool AreFriends(Player otherPlayer)
        {
            return MultiplayerStats.Friends.Contains(otherPlayer.Uid);
        }

        /// <summary>
        /// Gets the <see cref="Player"/> data from the server and updates itself.
        /// </summary>
        /// <param name="fullUpdate">Whether to update <see cref="HQStats"/> and <see cref="MoeStats"/> as well.</param>
        internal async Task Update(bool fullUpdate = false)
        {
            try
            {
                await MultiplayerStats.Update();
                if (fullUpdate)
                {
                    await MoeStats.Update();
                    await HQStats.Update();
                }
                LastUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                Main.Log(ex);
            }
        }
    }
}