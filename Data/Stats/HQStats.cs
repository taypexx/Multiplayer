namespace Multiplayer.Data.Stats
{
    public class HQStats
    {
        public uint Uid { get; private set; }
        public ushort MelonPoints { get; private set; }
        public ushort Records { get; private set; }
        public ushort APs { get; private set; }
        public float AverageAccuracy { get; private set; }
        public uint Top { get; private set; }
        public string Biography { get; private set; }

        public HQStats(uint uid = 0)
        {
            Uid = uid;
            MelonPoints = 0;
            Records = 0;
            APs = 0;
            AverageAccuracy = 0;
            Top = 100;
            Biography = "This user does not have anything interesting to say.";
        }

        internal void Update()
        {
            // TODO: Actually get the data from mdmc api and update the fields
        }
    }
}
