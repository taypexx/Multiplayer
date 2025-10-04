namespace Multiplayer.Data
{
    public class Achievement
    {
        public byte Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public Achievement(byte id, string name = "Achievement", string description = "") 
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}
