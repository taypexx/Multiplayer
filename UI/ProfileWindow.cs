using Il2CppAssets.Scripts.UI.Panels;
using LocalizeLib;
using Multiplayer.Data;
using Multiplayer.Managers;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using System.Net.Http.Json;
using UnityEngine;

namespace Multiplayer.UI
{
    internal sealed class ProfileWindow : BaseMultiplayerWindow
    {
        private ForumObject RankButton;
        private ForumObject AvatarButton;
        private ForumObject FriendsButton;
        private ForumObject AchievementsButton;
        private ForumObject HQStatsButton;
        private ForumObject MoeStatsButton;
        private ForumObject FriendRequestButton;

        private PromptWindow FriendRequestPrompt;
        private PromptWindow UnfriendPrompt;

        private int FriendButtonState;
        private Dictionary<int, LocalString> FriendButtonTitles;
        private Dictionary<int, LocalString> FriendButtonResponses;

        private static PnlHead PnlHead => GameObject.Find("UI/Forward/Tips/PnlHead").GetComponent<PnlHead>();

        // The Player whose stats are displayed in this window.
        internal Player Player;

        internal ProfileWindow() : base(Localization.Get("ProfileWindow", "Title"), UIManager.MainMenu) 
        {
            FriendButtonTitles = new()
            {
                [0] = Localization.Get("ProfileWindow", "DecideFriendRequest"),
                [1] = Localization.Get("ProfileWindow", "CancelFriendRequest"),
                [2] = Localization.Get("ProfileWindow", "RemoveFriend"),
                [3] = Localization.Get("ProfileWindow", "AddFriend"),
                [4] = new(),
            };

            FriendButtonResponses = new()
            {
                [0] = Localization.Get("ProfileWindow", "AddFriendSuccess"),
                [1] = Localization.Get("ProfileWindow", "CancelFriendRequestSuccess"),
                [2] = Localization.Get("ProfileWindow", "RemoveFriendSuccess"),
                [3] = Localization.Get("ProfileWindow", "AddedFriend"),
                [4] = FriendButtonTitles[4]
            };

            //Window.OnSelectionChanged += OnButtonClick;

            FriendRequestPrompt = new(Localization.Get("ProfileWindow", "DecideFriendRequestPrompt"));
            FriendRequestPrompt.OnCompletion += OnFriendActionDecided;

            UnfriendPrompt = new(Localization.Get("ProfileWindow", "DecideUnfriendPrompt"));
            UnfriendPrompt.OnCompletion += OnFriendActionDecided;
        }

        internal void CreateButtons()
        {
            RankButton = AddButton(Localization.Get("ProfileWindow", "Rank"));
            AvatarButton = AddButton(Localization.Get("ProfileWindow", "Avatar"), PnlHead);
            FriendsButton = AddButton(Localization.Get("ProfileWindow", "Friends"), UIManager.FriendsWindow);
            AchievementsButton = AddButton(Localization.Get("ProfileWindow", "Achievements"), UIManager.AchievementsWindow);
            //HQStatsButton = AddButton(Localization.Get("ProfileWindow", "HQStats")); No support for hq for now =(
            MoeStatsButton = AddButton(Localization.Get("ProfileWindow", "MoeStats"));
            FriendRequestButton = AddButton(Localization.Get("ProfileWindow", "AddFriend"));
            AddReturnButton();
        }

        private async void OnFriendActionDecided(BaseWindow window = null)
        {
            if (window == null || (window == FriendRequestPrompt && FriendRequestPrompt.Result == true) || (window == UnfriendPrompt && UnfriendPrompt.Result == true))
            {
                UIManager.Debounce = true;

                var payload = new
                {
                    Uid = PlayerManager.LocalPlayer.Uid,
                    Token = Client.TOKEN,
                    FriendUid = Player.Uid
                };

                var response = await Client.PostAsync("friendRequest", payload);
                LocalString msg;

                if (response != null)
                {
                    var data = await response.Content.ReadFromJsonAsync<Dictionary<string, int?>>();
                    data.TryGetValue("Action", out int? actionDid);
                    if (actionDid == null) { actionDid = FriendButtonState; }

                    msg = FriendButtonResponses[FriendButtonState];
                } else
                {
                    msg = Localization.Get("Warning","Offline");
                }

                PopupUtils.ShowInfoAndLog(msg);

                UIManager.Debounce = false;
            }

            Window.Show();
        }

        internal override void OnButtonClick(PopupLib.UI.Windows.Interfaces.IListWindow window, int objectIndex)
        {
            base.OnButtonClick(window, objectIndex);

            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == FriendRequestButton) 
            {
                if (FriendButtonState == 0)
                {
                    Window.ForceClose();
                    FriendRequestPrompt.Show();
                } else if (FriendButtonState == 2)
                {
                    Window.ForceClose();
                    UnfriendPrompt.Show();
                } else if (FriendButtonState != 4)
                {
                    OnFriendActionDecided();
                }
            }
        }

        /// <summary>
        /// Updates the profile window to display the information about the given <see cref="Data.Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Data.Player"/> whose stats will now appear in the window.</param>
        internal void Update(Player player)
        {
            Player = player;

            RankButton.Contents = new
            (
                $"ELO: {Player.MultiplayerStats.ELO}\n\n" +
                $"Rank: {Player.MultiplayerStats.Rank}"
            );

            UIManager.FriendsWindow.Update(player);
            UIManager.AchievementsWindow.Update(player);

            MoeStatsButton.Contents = new
            (
                $"RL: {Player.MoeStats.RL}\n\n" +
                $"Records: {Player.MoeStats.Records}\n" +
                $"APs: {Player.MoeStats.APs}\n" +
                $"Average Accuracy: {Player.MoeStats.AverageAccuracy}%"
            );

            Player localPlayer = PlayerManager.LocalPlayer;

            Title = Player == localPlayer ? Localization.Get("ProfileWindow", "TitleMyProfile") : Localization.Get("ProfileWindow", "Title");

            FriendButtonState =
                Player == localPlayer ? 4 :
                localPlayer.MultiplayerStats.FriendRequests.TryGetValue(Player.Uid, out _) ? 0 :
                Player.MultiplayerStats.FriendRequests.TryGetValue(localPlayer.Uid, out _) ? 1 :
                Player.MultiplayerStats.Friends.Contains(localPlayer) || localPlayer.MultiplayerStats.Friends.Contains(Player) ? 2 :
                3;

            FriendRequestButton.Titles = FriendButtonTitles[FriendButtonState];
            FriendRequestPrompt.Title = Player.MultiplayerStats.NameLocal;
            UnfriendPrompt.Title = Player.MultiplayerStats.NameLocal;
        }
    }
}
