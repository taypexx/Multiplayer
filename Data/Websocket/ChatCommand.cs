using LocalizeLib;
using Multiplayer.Static;

namespace Multiplayer.Data.Websocket
{
    internal class ChatCommand
    {
        internal string Name { get; private set; }
        internal LocalString Description { get; private set; }
        private Action Action { get; set; }

        internal void Run() => Action?.Invoke();

        internal ChatCommand(string name, Action action)
        {
            Name = name;
            Description = Localization.Get("CommandDescriptions", name);
            Action = action;
        }
    }
}
