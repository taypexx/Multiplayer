namespace Multiplayer.Data
{
    public class Rank
    {
        public const ushort SubdivisionGap = 50;

        public static readonly List<string> SubdivisionSuffixes = new()
        {
            "I","II","III","IV"
        };

        public static List<Rank> RanksList = new()
        {
            new(3000, "Autoplayer", 0),
            new(2800, ""),
            new(2650, ""),
            new(2500, ""),
            new(2300, ""),
            new(2150, ""),
            new(2000, ""),
            new(1800, ""),
            new(1650, ""),
            new(1500, "Chill guy"),
            new(1300, ""),
            new(1150, ""),
            new(1000, ""),
            new(500, ""),
            new(0, "Noob", 0)
        };

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
