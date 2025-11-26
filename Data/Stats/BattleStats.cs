namespace Multiplayer.Data.Stats
{
    public class BattleStats
    {
        public Player Player { get; private set; }
        public uint Score { get; internal set; } = 0;
        public float Accuracy { get; internal set; } = 100f;
        public bool FC { get; internal set; } = false;
        public bool AP => Accuracy == 100f;
        public bool TrueAP => AP && Earlies == 0 && Lates == 0;

        public ushort Perfects { get; internal set; } = 0;
        public ushort Greats { get; internal set; } = 0;

        public ushort Earlies { get; internal set; } = 0;
        public ushort Lates { get; internal set; } = 0;
        public ushort Misses { get; internal set; } = 0;

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
                else { return Grade.D; }
            }
        }

        public BattleStats(Player player)
        {
            Player = player;
        }
    }
}
