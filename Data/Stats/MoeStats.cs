namespace Multiplayer.Data.Stats
{
    public class MoeStats
    {
        public string Uid { get; private set; }
        public float RL { get; private set; }
        public ushort Records { get; private set; }
        public ushort APs { get; private set; }
        public float AverageAccuracy { get; private set; }

        public MoeStats(string uid)
        {
            Uid = uid;
            RL = 0;
            Records = 0;
            APs = 0;
            AverageAccuracy = 0;
        }

        internal void Update()
        {
            // TODO: Actually get the data from md moe api and update the fields
        }
    }
}
