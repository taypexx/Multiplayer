namespace Multiplayer.Data
{
    public class Rank
    {
        public const ushort SubdivisionGap = 150;

        public static readonly List<string> SubdivisionSuffixes = new()
        {
            "I","II","III"
        };

        public static List<Rank> RanksList = new()
        {
            new(3000, "Autoplayer", 0),
            new(2500, "Master"),
            new(2000, "Advanced"),
            new(1500, "Apprentice"),
            new(1000, "Trainee"),
            new(500, "Rookie"),
            new(0, "Rookie", 0)
        };

        public static ushort TopRankELO 
        { 
            get 
            {
                ushort top = 0;
                foreach (var rank in RanksList)
                {
                    if (rank.ELO > top) top = rank.ELO;
                }
                return top;
            } 
        }

        public ushort ELO;
        public string Name;
        public byte SubRanks;

        internal Rank(ushort elo, string name, byte subRanks = 3)
        {
            ELO = elo;
            Name = name;
            SubRanks = subRanks;
        }
    }
}
