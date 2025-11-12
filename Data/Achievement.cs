using LocalizeLib;

namespace Multiplayer.Data
{
    public class Achievement
    {
        private static byte IdInc = 0;

        public byte Id { get; private set; }
        public LocalString Name { get; private set; }
        public LocalString Description { get; private set; }

        public Achievement(string name) 
        {
            Id = IdInc; IdInc++;
            Name = new(name);
            Description = Localization.Get("Achievements", name);
        }
    }
}
