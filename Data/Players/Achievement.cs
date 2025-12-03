using LocalizeLib;
using Multiplayer.Static;

namespace Multiplayer.Data.Players
{
    public enum AchievementDifficulty : byte
    {
        Easy, Medium, Hard, Secret
    }

    public class Achievement
    {
        private static byte IdInc = 0;

        public byte Id { get; private set; }
        public LocalString Name { get; private set; }
        public LocalString Description { get; private set; }
        public AchievementDifficulty Difficulty { get; private set; }

        public Achievement(string name, AchievementDifficulty difficulty = AchievementDifficulty.Medium)
        {
            Id = IdInc; IdInc++;
            Name = new(name);
            Description = Localization.Get("Achievements", name);
            Difficulty = difficulty;
        }
    }
}
