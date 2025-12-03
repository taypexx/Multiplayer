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

        public Grade Grade
        {
            get
            {
                if (Accuracy == 100)
                {
                    return Grade.SSS;
                }
                else if (Accuracy >= 95)
                {
                    return Grade.SS;
                }
                else if (Accuracy >= 90)
                {
                    return Grade.S;
                }
                else if (Accuracy >= 80 || Misses == 0)
                {
                    return Grade.A;
                }
                else if (Accuracy >= 70)
                {
                    return Grade.B;
                }
                else if (Accuracy >= 60)
                {
                    return Grade.C;
                }
                else return Grade.D;
            }
        }

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

        internal void UpdatePrevious()
        {
            PrevAP = AP;
            PrevFC = FC;
            PrevMisses = Misses;
        }
    }
}
