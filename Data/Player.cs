using Multiplayer.Data.Stats;

namespace Multiplayer.Data
{
    public class Player
    {
        public string Uid { get; private set; }
        internal string UniqueHash  { get { return Uid.Reverse() + Client.ComputeSha256Hash("ame is gay"); } }
        
        public MultiplayerStats MultiplayerStats { get; private set; }
        public BattleStats BattleStats { get; private set; }
        public HQStats HQStats { get; private set; }
        public MoeStats MoeStats { get; private set; }

        public ushort TotalRecords { get { return (ushort)(HQStats.Records + MoeStats.Records); } }
        public ushort TotalAPs { get { return (ushort)(HQStats.APs + MoeStats.APs); } }
        public float TotalAverageAccuracy { get { return (HQStats.AverageAccuracy + MoeStats.AverageAccuracy) / 2; } }

        internal Player(string uid)
        {
            Uid = uid;
            MultiplayerStats = new(this);
            BattleStats = new(this);
            MoeStats = new(this);
            HQStats = new(this);
        }

        /// <summary>
        /// Updates the <see cref="Player"/>.
        /// </summary>
        /// <param name="fullUpdate">Whether to update HQStats and MoeStats as well.</param>
        internal void Update(bool fullUpdate = false)
        {
            MultiplayerStats.Update();

            if (fullUpdate)
            {
                HQStats.Update();
                MoeStats.Update();
            }
        }

        /// <summary>
        /// Updates the player in-battle stats.
        /// </summary>
        /// <param name="battleStats">Updated <see cref="Data.Stats.BattleStats"/>.</param>
        internal void UpdateBattle(BattleStats battleStats)
        {
            BattleStats = battleStats;
        }
    }
}
