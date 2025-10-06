using LocalizeLib;

namespace Multiplayer.Data.Stats
{
    public class MultiplayerStats
    {
        public string Name { get; internal set; }
        public LocalString NameLocal { get; internal set; }
        public string AvatarName { get; internal set; }

        public List<Player> Friends { get; internal set; }
        public Dictionary<DateTime, Achievement> Achievements { get; internal set; }

        public ushort ELO { get; internal set; }
        public bool Banned { get; internal set; }
        public string Rank => GetRank(true);

        public MultiplayerStats(string name = "Player")
        {
            Name = name;
            NameLocal = new(Name);
            AvatarName = "default";
            Friends = new();
            Achievements = new();
            ELO = 1500;
            Banned = false;
        }

        private string GetRank(bool includingSubrank = false)
        {
            for (int i = 0; i < Data.Rank.RanksList.Count; i++)
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
