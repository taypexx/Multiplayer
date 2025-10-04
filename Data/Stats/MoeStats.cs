namespace Multiplayer.Data.Stats
{
    public class MoeStats
    {
        public string Uid { get; private set; } = string.Empty;
        public float RL { get; private set; } = 0;
        public ushort Records { get; private set; } = 0;
        public ushort APs { get; private set; } = 0;
        public float AverageAccuracy { get; private set; } = 0;

        public MoeStats(string uid)
        {
            Uid = uid;
        }

        internal void Update()
        {
            // TODO: Actually get the data from md moe api and update the fields
        }
    }
}
