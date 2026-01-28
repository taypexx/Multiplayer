using LocalizeLib;
using Multiplayer.Managers;
using Multiplayer.Static;

namespace Multiplayer.Data.Chat
{
    internal class ChatCommand
    {
        internal string Name { get; private set; }
        internal LocalString Description { get; private set; }
        internal bool Hidden { get; private set; }
        private Action<string[]> Action { get; set; }

        internal void Run(string[] p) 
        {
            if (UIManager.ChatLobbyDisplay == null) return;
            Action.Invoke(p);
        }

        internal ChatCommand(string name, Action<string[]> action, bool hidden = false)
        {
            Name = name;
            Description = Localization.Get("CommandDescriptions", name);
            Hidden = hidden;
            Action = action;
        }
    }
}
