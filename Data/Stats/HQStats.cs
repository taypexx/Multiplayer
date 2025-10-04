namespace Multiplayer.Data.Stats
{
    public class HQStats
    {
        public uint Uid { get; private set; } = 0;
        public ushort MelonPoints { get; private set; } = 0;
        public ushort Records { get; private set; } = 0;
        public ushort APs { get; private set; } = 0;
        public float AverageAccuracy { get; private set; } = 0;
        public uint Top { get; private set; } = 100;
        public string Biography { get; private set; } = "This user does not have anything interesting to say.";

        public HQStats(uint uid = 0)
        {
            Uid = uid;
        }

        internal void Update()
        {
            // TODO: Actually get the data from mdmc api and update the fields
        }
    }
}
