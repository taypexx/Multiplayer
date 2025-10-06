using LocalizeLib;

namespace Multiplayer.Data
{
    public class Achievement
    {
        private static byte IdInc = 0;

        public byte Id { get; private set; }
        public LocalString Name { get; private set; }
        public LocalString Description { get; private set; }

        public Achievement(string name, LocalString description) 
        {
            Id = IdInc; IdInc++;
            Name = new(name);
            Description = description;
        }
    }
}
