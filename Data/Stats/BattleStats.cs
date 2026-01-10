using Multiplayer.Data.Players;

namespace Multiplayer.Data.Stats
{
    public enum Grade : byte
    {
        SSS = 0,
        SS = 1,
        S = 2,
        A = 3,
        B = 4,
        C = 5,
        D = 6
    }

    public class BattleStats
    {
        public Player Player { get; private set; }
        public uint Score { get; internal set; }
        public float Accuracy { get; 
            internal set {
                if (float.IsNaN(value)) { field = 100f; return; }
                field = (float)Math.Round((decimal)value * 100) / 100f;
            } 
        }
        public bool FC { get; internal set; }
        public bool AP => Accuracy == 100f;
        public bool TrueAP => AP && Earlies == 0 && Lates == 0;

        public ushort Perfects { get; internal set; }
        public ushort Greats { get; internal set; }

        public ushort Earlies { get; internal set; }
        public ushort Lates { get; internal set; }
        public ushort Misses { get; internal set; } 

        public Grade Grade => Accuracy switch
        {
            100f => Grade.SSS,
            >= 95f => Grade.SS,
            >= 90f => Grade.S,
            >= 80f => Grade.A,
            _ when Misses == 0 => Grade.A,
            >= 70f => Grade.B,
            >= 60f => Grade.C,
            _ => Grade.D
        };

        public BattleStats(Player player)
        {
            Player = player;
            Reset();
            UpdatePrevious();
        }

        /// <summary>
        /// Resets everything to default.
        /// </summary>
        internal void Reset()
        {
            Score = 0;
            Accuracy = 100f;
            FC = true;
            Perfects = 0;
            Greats = 0;
            Earlies = 0;
            Lates = 0;
            Misses = 0;
        }

        public bool PrevAP { get; private set; }
        public bool PrevFC { get; private set; }
        public ushort PrevMisses { get; private set; }

        /// <summary>
        /// Saves previous AP, FC and misses.
        /// </summary>
        internal void UpdatePrevious()
        {
            PrevAP = AP;
            PrevFC = FC;
            PrevMisses = Misses;
        }
    }
}
