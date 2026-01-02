using Multiplayer.Data.Players;
using Multiplayer.Static;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Data.Stats
{
    public class MoeStats
    {
        public Player Player { get; private set; }
        public float RL { get; private set; }
        public ushort Records { get; private set; }
        public ushort APs { get; private set; }
        public float AverageAccuracy { get;
            private set
            {
                if (float.IsNaN(value)) { field = 100f; return; }
                field = (float)Math.Round((decimal)value * 100) / 100f;
            }
        }

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
        /// <returns><see langword="true"/> if update was successful, otherwise <see langword="false"/>.</returns>
        internal async Task<bool> Update()
        {
            var response = await Client.GetAsync("https://api.musedash.moe/player/" + Player.Uid, true, false, false);
            if (response == null) return false;

            var updatedData = await response.Content.ReadFromJsonAsync<Dictionary<string,JsonElement>>();
            var plays = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(updatedData["plays"].GetRawText());

            unchecked
            {
                float newRL = 0;
                updatedData["rl"].TryGetSingle(out newRL);
                RL = newRL;
                Records = (ushort)plays.Count;
            }
            APs = 0;

            float totalAcc = 0f;
            foreach (var play in plays)
            {
                float acc = 0;
                play["acc"].TryGetSingle(out acc);
                if (acc == 100) APs++;
                totalAcc += acc;
            }
            AverageAccuracy = totalAcc / Records;

            return true;
        }
    }
}
