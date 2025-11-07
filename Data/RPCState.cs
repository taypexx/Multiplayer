namespace Multiplayer.Data
{
    public enum RPCState : byte
    {
        Idle,
        InLobby,
        InPrivateLobby,
        PlayingSolo,
        PlayingFriends,
        PlayingQueued
    }
}
