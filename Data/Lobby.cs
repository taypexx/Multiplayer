namespace Multiplayer.Data
{
    public class Lobby
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public ushort MaxPlayers { get; private set; }
        public List<string> PlayerUids { get; private set; }
        public Player Host { get; private set; }
        public bool IsPrivate { get; private set; }
        public string Hash { get; private set; }

        internal Lobby(int id, string name, ushort maxPlayers, List<string> playerUids, Player host, bool isPrivate)
        {
            Id = id;
            Name = name;
            MaxPlayers = maxPlayers;
            PlayerUids = playerUids;
            Host = host;
            IsPrivate = isPrivate;
            Hash = Client.ComputeSha256Hash(Id + "poopfart" + MaxPlayers);
        }

        internal bool IsMember(Player player)
        {
            return PlayerUids.Contains(player.Uid);
        }
    }
}
