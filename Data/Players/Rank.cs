using LocalizeLib;
using Multiplayer.Static;

namespace Multiplayer.Data.Players
{
    public class Rank
    {
        public const ushort SubdivisionGap = 150;

        public static readonly List<string> SubdivisionSuffixes = new()
        {
            "I","II","III"
        };

        public static readonly List<Rank> RanksList = new()
        {
            new(3000, Localization.Get("Ranks","1"), 0),
            new(2500, Localization.Get("Ranks","2")),
            new(2000, Localization.Get("Ranks","3")),
            new(1500, Localization.Get("Ranks","4")),
            new(1000, Localization.Get("Ranks","5")),
            new(500,Localization.Get("Ranks","6")),
            new(0, Localization.Get("Ranks","7"), 0)
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
        public LocalString Name;
        public byte SubRanks;

        internal Rank(ushort elo, LocalString name, byte subRanks = 3)
        {
            ELO = elo;
            Name = name;
            SubRanks = subRanks;
        }
    }
}
