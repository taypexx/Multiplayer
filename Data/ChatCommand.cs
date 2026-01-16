using LocalizeLib;
using Multiplayer.Managers;
using Multiplayer.Static;

namespace Multiplayer.Data
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
            TotalCommands.Add(name, this);
        }

        internal static Dictionary<string, ChatCommand> TotalCommands = new();

        internal static void CreateCommands()
        {
            TotalCommands.Clear();

            new ChatCommand("help", new(() => 
            {
                if (UIManager.ChatLobbyDisplay == null) return;

                string helpText = string.Empty;
                foreach ((string commandName, var chatCommand) in TotalCommands)
                {
                    if (helpText != string.Empty) helpText += "\n";
                    helpText = helpText + $"/{commandName} — {chatCommand.Description}";
                }

                UIManager.ChatLobbyDisplay.AddMessage(new()
                {
                    Message = helpText,
                    AuthorName = "system"
                });
            }));

            new ChatCommand("clear", new(() =>
            {
                if (UIManager.ChatLobbyDisplay == null) return;

                UIManager.ChatLobbyDisplay.ClearText(true);
                UIManager.ChatLobbyDisplay.Update();
            }));

            new ChatCommand("website", new(() =>
            {
                Utilities.OpenBrowserLink($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/home");
            }));

            new ChatCommand("mdmc", new(() =>
            {
                Utilities.OpenBrowserLink("https://mdmc.moe");
            }));

            new ChatCommand("discord", new(() =>
            {
                Utilities.OpenBrowserLink($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/discord");
            }));
        }
    }
}
