using Multiplayer.Data.Players;
using Multiplayer.Static;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Data.Stats
{
    public class MoeStats
    {
        public Player Player { get; private set; }
        public float RL { get; private set { field = Utilities.RoundFloat(value); } }
        public ushort Records { get; private set; }
        public ushort APs { get; private set; }
        public float AverageAccuracy { get; private set { field = Utilities.RoundFloat(value); }}

        public MoeStats(Player player)
        {
            Player = player;
            RL = 0;
            Records = 0;
            APs = 0;
            AverageAccuracy = 0;
        }

        /// <summary>
        /// Synchronizes stats with <see href="https://musedash.moe"/>.
        /// </summary>
        internal async Task Update()
        {
            var response = await Client.GetAsync("https://api.musedash.moe/player/" + Player.Uid, true, false);
            if (response == null) return;

            var updatedData = await response.Content.ReadFromJsonAsync<Dictionary<string,JsonElement>>();
            var plays = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(updatedData["plays"].GetRawText());

            unchecked
            {
                float newRL = 0f;
                updatedData["rl"].TryGetSingle(out newRL);
                RL = newRL;
                Records = (ushort)plays.Count;
            }
            APs = 0;

            float totalAcc = 0f;
            foreach (var play in plays)
            {
                float acc = 0f;
                play["acc"].TryGetSingle(out acc);
                if (acc == 100f) APs++;
                totalAcc += acc;
            }
            AverageAccuracy = totalAcc / Records;
        }
    }
}
