using Multiplayer.Data.Players;
using Multiplayer.Managers;

namespace Multiplayer.Data.Stats
{
    public class HQStats
    {
        public Player Player { get; private set; }

        public string Name { get; private set; }
        public string Bio { get; private set; }
        public string[] Badges { get; private set; }

        public CustomImageAsset Avatar { get; private set; }
        public CustomImageAsset Banner { get; private set; }

        public ushort MelonPoints { get; private set; }
        public int Top { get; private set; }

        public ushort Records { get; private set; }
        public ushort APs { get; private set; }
        public float AverageAccuracy { get; private set; }

        public HQStats(Player player)
        {
            Player = player;

            Name = PlayerManager.LocalPlayerName ?? player.Uid;
            Bio = "This user does not have anything interesting to say.";

            MelonPoints = 0;
            Top = -1;

            Records = 0;
            APs = 0;
            AverageAccuracy = 0;
        }

        /// <summary>
        /// Synchronizes stats with <see href="https://mdmc.moe"/>.
        /// </summary>
        internal async Task Update()
        {
            if (Player.HQUid < 1) return;

            if (Player == PlayerManager.LocalPlayer)
            {

            }
            else
            {

            }
        }
    }
}
