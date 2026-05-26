using LocalizeLib;

namespace Multiplayer.Data.Settings
{
    public interface ISetting
    {
        public string Name { get; set; }
        public LocalString LocalName { get; set; }
        public LocalString Description { get; set; }
        public SettingCategory Category { get; set; }

        public void Load();
        public void Save();
    }
}
