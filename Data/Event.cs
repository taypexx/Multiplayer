using System.Text.Json;

namespace Multiplayer.Data
{
    public enum EventType : byte
    {
        Invite, Announcement, FriendRequest
    }

    public class Event
    {
        public EventType Type { get; private set; }
        public Dictionary<string, JsonElement> Data { get; private set; }
        public DateTime Time { get; private set; }

        internal Event(EventType type, Dictionary<string, JsonElement> data, DateTime time)
        {
            Type = type;
            Data = data;
            Time = time;
        }
    }
}
