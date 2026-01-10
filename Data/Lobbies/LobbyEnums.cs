namespace Multiplayer.Data.Lobbies
{
    public enum LobbyGoal : byte
    {
        Accuracy,
        Score,
        Custom
    }

    public enum LobbyPlayType : byte
    {
        All,
        VanillaOnly,
        CustomOnly
    }

    public enum LobbyChartSelection : byte
    {
        HostPlaylist,
        Playlist,
        Random
    }
}
