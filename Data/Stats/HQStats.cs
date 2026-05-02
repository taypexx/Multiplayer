using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using System.Net.Http.Json;
using System.Text.Json;

namespace Multiplayer.Data.Stats
{
    public class HQStats
    {
        public Player Player { get; private set; }
        public int Uid => Player.MultiplayerStats.HQUid;
        public bool LoggedIn => Uid > 0;
        public string DiscordId { get; private set; }

        public string Name { get; private set; }
        public string Bio { get; private set; }
        public string[] Badges { get; private set; }

        public CustomImageAsset Avatar { get; private set; }
        public CustomImageAsset Banner { get; private set; }

        internal string AvatarString { get; private set; }
        internal string BannerString { get; private set; }

        public ushort MelonPoints { get; private set; }
        public int Top { get; private set; }

        public int Records { get; private set; }
        public int APs { get; private set; }
        public float AverageAccuracy { get; private set; }

        public HQStats(Player player)
        {
            Player = player;

            Name = PlayerManager.LocalPlayerName ?? $"Player{Uid}";
            Bio = "This user does not have anything interesting to say.";

            MelonPoints = 0;
            Top = -1;

            Records = 0;
            APs = 0;
            AverageAccuracy = 0;
        }

        internal async Task UpdateImages(string avatar, string banner, bool ignoreCache = false)
        {
            if (AvatarString != avatar)
            {
                AvatarString = avatar;
                Avatar = await AssetManager.GetImageAssetFromWeb($"https://cdn.mdmc.moe/avatars/{DiscordId}.{AvatarString}.webp", ignoreCache);
            }

            if (BannerString != banner)
            {
                BannerString = banner;
                Banner = await AssetManager.GetImageAssetFromWeb($"https://cdn.mdmc.moe/banners/{DiscordId}.{BannerString}.webp", ignoreCache);
            }
        }

        /// <summary>
        /// Updates fields of the <see cref="HQStats"/> by the given <paramref name="topData"/> JSON dictionary.
        /// </summary>
        /// <param name="topData">JSON dictionary containing fields as keys and their values.</param>
        internal void UpdateTopData(Dictionary<string, JsonElement> topData)
        {
            Records = topData["totalScores"].GetInt32();
            APs = topData["perfectScores"].GetInt32();
            AverageAccuracy = topData["averageAccuracy"].GetSingle();
        }

        /// <summary>
        /// Updates fields of the <see cref="HQStats"/> by the given <paramref name="userData"/> JSON dictionary.
        /// </summary>
        /// <param name="userData">JSON dictionary containing fields as keys and their values.</param>
        internal void UpdateUserData(Dictionary<string, JsonElement> userData)
        {
            DiscordId = userData["discordId"].GetString();

            var profile = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userData["profile"]);
            if (profile == null) return;

            Bio = profile["bio"].ToString();
            Badges = JsonSerializer.Deserialize<string[]>(profile["badges"]);

            var avatar = profile["avatar"].ToString();
            var banner = profile["banner"].ToString();

            _ = UpdateImages(avatar, banner);

            var ranking = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userData["ranking"]);
            MelonPoints = (ushort)ranking["melonPoints"].GetSingle();
            Top = ranking["rank"].GetInt32();
        }

        /// <summary>
        /// Synchronizes stats with <see href="https://mdmc.moe"/>.
        /// </summary>
        internal async Task Update()
        {
            if (!LoggedIn) return;

            var responseUser = await Client.GetAsync("https://api.mdmc.moe/v3/users/" + Uid, true, false);
            if (responseUser is null) return;
            UpdateUserData(await responseUser.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>());

            var responseTop = await Client.GetAsync($"https://api.mdmc.moe/v3/users/{Uid}/stats", true, false);
            if (responseTop is null) return;
            UpdateTopData(await responseTop.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>());
        }
    }
}
