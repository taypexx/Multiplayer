using Multiplayer.Data.Stats;

namespace Multiplayer.Data
{
    public class Player
    {
        public string Uid { get { return MoeStats.Uid; } }
        
        public MultiplayerStats MultiplayerStats { get; private set; }
        public BattleStats BattleStats { get; private set; }
        public HQStats HQStats { get; private set; }
        public MoeStats MoeStats { get; private set; }

        public ushort TotalRecords { get { return (ushort)(HQStats.Records + MoeStats.Records); } }
        public ushort TotalAPs { get { return (ushort)(HQStats.APs + MoeStats.APs); } }
        public float TotalAverageAccuracy { get { return (HQStats.AverageAccuracy + MoeStats.AverageAccuracy) / 2; } }

        internal Player(string uid)
        {
            MultiplayerStats = new();
            BattleStats = new();
            HQStats = new();
            MoeStats = new(uid);
        }

        /// <summary>
        /// Adds a <see cref="Player"/> to the friend list.
        /// </summary>
        /// <param name="friend">Potential friend.</param>
        /// <returns><see langword="true"/> if the friend was added or <see langword="false"/> if you were friends already.</returns>
        internal bool AddFriend(Player friend)
        {
            if (friend == null) return false;
            if (MultiplayerStats.Friends.Contains(friend)) return false;
            MultiplayerStats.Friends.Add(friend);
            return true;
        }

        /// <summary>
        /// Removes a <see cref="Player"/> from the friend list.
        /// </summary>
        /// <param name="friend">The despicable betrayer.</param>
        /// <returns><see langword="true"/> if the friend was removed or <see langword="false"/> if you weren't even friends.</returns>
        internal bool RemoveFriend(Player friend)
        {
            if (!MultiplayerStats.Friends.Contains(friend)) return false;
            MultiplayerStats.Friends.Remove(friend);
            return true;
        }

        /// <summary>
        /// Updates the <see cref="Player"/>.
        /// </summary>
        /// <param name="multiplayerStats">Updated <see cref="Data.Stats.MultiplayerStats"/>.</param>
        /// <param name="fullUpdate">Whether to update HQStats and MoeStats as well.</param>
        internal void Update(MultiplayerStats multiplayerStats, bool fullUpdate = false)
        {
            MultiplayerStats = multiplayerStats;

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
