using Multiplayer.Data.Stats;

namespace Multiplayer.Data
{
    public class Player
    {
        public string Uid { get; private set; }

        public MultiplayerStats MultiplayerStats { get; private set; }
        public BattleStats BattleStats { get; private set; }
        public HQStats HQStats { get; private set; }
        public MoeStats MoeStats { get; private set; }

        public ushort TotalRecords { get { return (ushort)(HQStats.Records + MoeStats.Records); } }
        public ushort TotalAPs { get { return (ushort)(HQStats.APs + MoeStats.APs); } }
        public float TotalAverageAccuracy { get { return (HQStats.AverageAccuracy + MoeStats.AverageAccuracy) / 2; } }

        internal DateTime LastUpdated { get; private set; }
        internal Player(string uid)
        {
            Uid = uid;
            MultiplayerStats = new(this);
            BattleStats = new(this);
            MoeStats = new(this);
            HQStats = new(this);
        }

        /// <summary>
        /// Updates stats of the <see cref="Player"/>.
        /// </summary>
        /// <param name="fullUpdate">Whether to update HQStats and MoeStats as well.</param>
        /// <returns><see langword="true"/> if all updates were successful, otherwise <see langword="false"/>.</returns>
        internal async Task<bool> Update(bool fullUpdate = false)
        {
            LastUpdated = DateTime.Now;
            if (fullUpdate)
            {
                return await MultiplayerStats.Update() && await MoeStats.Update() && await HQStats.Update();
            } else
            {
                return await MultiplayerStats.Update();
            }
        }
    }
}
