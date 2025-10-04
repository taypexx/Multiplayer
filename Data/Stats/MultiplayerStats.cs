using LocalizeLib;

namespace Multiplayer.Data.Stats
{
    public class MultiplayerStats
    {
        public string Name { get; internal set; } = "Player";
        public LocalString NameLocal { get; internal set; }
        public string AvatarName { get; internal set; } = "default";

        public List<Player> Friends { get; internal set; } = new();
        public List<Achievement> Achievements { get; internal set; } = new();

        public ushort ELO { get; internal set; } = 1500;
        public bool Banned { get; internal set; } = false;
        public string Rank => GetRank(true);

        public MultiplayerStats(string name = "Player")
        {
            Name = name;
            NameLocal = new(Name);
        }

        private string GetRank(bool includingSubrank = false)
        {
            for (byte i = 0; i < Data.Rank.RanksList.Count; i++)
            {
                Rank rank = Data.Rank.RanksList[i];
                if (ELO >= rank.ELO)
                {
                    if (includingSubrank && rank.SubRanks > 0)
                    {
                        return $"{rank.Name} {Data.Rank.SubdivisionSuffixes[(int)Math.Floor((decimal)((ELO - rank.ELO) / Data.Rank.SubdivisionGap))]}";
                    }
                    else
                    {
                        return rank.Name;
                    }
                }
            }
            return Data.Rank.RanksList.Last().Name;
        }
    }
}
