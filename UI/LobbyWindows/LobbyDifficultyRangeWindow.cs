using LocalizeLib;
using Multiplayer.Data.Lobbies;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using PopupLib.UI.Windows.Interfaces;

namespace Multiplayer.UI.LobbyWindows
{
    internal sealed class LobbyDifficultyRangeWindow : BaseMultiplayerWindow
    {
        internal byte ValueLowest { get; private set; } = 1;
        internal byte ValueHighest { get; private set; } = 13;

        private byte LowestTemp = 1;

        private List<Tuple<byte,byte>> PresetValues = new()
        {
            new(1, 5),
            new(6, 8),
            new(9, 10),
            new(11, 13)
        };

        private Dictionary<ForumObject, Tuple<byte, byte>> ButtonsRanges = new();
        private InputWindow LowestInput;
        private InputWindow HighestInput;

        private LocalString MainDescription => Localization.Get("LobbyCreation", "DifficultyRangeDescription");

        internal LobbyDifficultyRangeWindow() : base(Localization.Get("LobbyCreation", "DifficultyRange"), UIManager.LobbyCreationWindow, "Lobbies.png")
        {
            foreach (var range in PresetValues)
            {
                ForumObject button = AddButton(new($"{range.Item1} ~ {range.Item2}"), null, MainDescription);
                ButtonsRanges.Add(button, range);
            }

            HighestInput = new(new("To:"));
            HighestInput.AutoReset = true;
            HighestInput.OnCompletion += (BaseWindow _) =>
            {
                if (byte.TryParse(HighestInput.Result, out byte highest))
                {
                    Apply(LowestTemp, highest);
                }
                else
                {
                    PopupUtils.ShowInfo(Localization.Get("LobbyCreation", "InvalidDifficultyRange"));
                    Window.Show();
                }
            };

            LowestInput = new(new("From:"));
            LowestInput.AutoReset = true;
            LowestInput.OnCompletion += (BaseWindow _) =>
            {
                if (byte.TryParse(LowestInput.Result, out byte lowest))
                {
                    LowestTemp = lowest;
                    HighestInput.Show();
                }
                else
                {
                    PopupUtils.ShowInfo(Localization.Get("LobbyCreation", "InvalidDifficultyRange"));
                    Window.Show();
                }
            };

            AddButton(Localization.Get("LobbyCreation", "CustomDifficultyRange"), LowestInput, MainDescription);
        }

        private void Apply(byte lowest, byte highest)
        {
            if (lowest > highest)
            {
                PopupUtils.ShowInfo(Localization.Get("LobbyCreation", "InvalidDifficultyRange"));
            }
            else
            {
                ValueLowest = lowest;
                ValueHighest = highest;
            }

            UIManager.LobbyCreationWindow.UpdateDescription();
            UIManager.LobbyCreationWindow.Window.Show();
        }

        protected override void OnButtonClick(IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];
            if (ButtonsRanges.ContainsKey(button))
            {
                Apply(ButtonsRanges[button].Item1, ButtonsRanges[button].Item2);
            }
        }
    }
}
