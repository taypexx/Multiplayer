using Multiplayer.Managers;

namespace Multiplayer.Data
{
    public class Lobby
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public ushort MaxPlayers { get; private set; }
        public List<Player> Players { get; private set; }
        public Player Host { get; private set; }
        public bool IsPrivate { get; private set; }

        internal Lobby(int id, string name, ushort maxPlayers, List<string> playerUids, Player host, bool isPrivate)
        {
            Id = id;
            Name = name;
            MaxPlayers = maxPlayers;
            Host = host;
            IsPrivate = isPrivate;

            Players = new();
            foreach (string playerUid in playerUids)
            {
                Player player = PlayerManager.GetPlayer(playerUid);
                Players.Add(player);
            }
        }

        internal bool IsMember(Player player)
        {
            return Players.Contains(player);
        }
    }
}
