using Il2Cpp;
using Il2CppAssets.Scripts.UI.Panels;
using LocalizeLib;
using Multiplayer.Data.Players;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Abstract;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using System.Net.Http.Json;
using UnityEngine;

namespace Multiplayer.UI.ProfileWindows
{
    internal sealed class ProfileWindow : BaseMultiplayerWindow
    {
        private ForumObject StatsButton;
        private ForumObject FriendsButton;
        private ForumObject FriendRequestsButton;
        private ForumObject AchievementsButton;
        private ForumObject HQStatsButton;
        private ForumObject MDMoeButton;
        private ForumObject FriendActionButton;

        private PromptWindow FriendRequestPrompt;
        private PromptWindow UnfriendPrompt;

        private int FriendButtonState;
        private Dictionary<int, LocalString> FriendButtonTitles;
        private Dictionary<int, LocalString> FriendButtonResponses;

        private GameObject AvatarBox;
        private HeadItem AvatarHeadItem;

        // The Player whose stats are displayed in this window.
        internal Player Player;

        internal ProfileWindow() : base(Localization.Get("ProfileWindow", "Title"), UIManager.MainMenu, "Profile.png")
        {
            FriendButtonTitles = new()
            {
                [0] = Localization.Get("ProfileWindow", "DecideFriendRequest"),
                [1] = Localization.Get("ProfileWindow", "CancelFriendRequest"),
                [2] = Localization.Get("ProfileWindow", "RemoveFriend"),
                [3] = Localization.Get("ProfileWindow", "RequestSend"),
                [4] = Localization.Get("Window", "Empty"),
            };

            FriendButtonResponses = new()
            {
                [0] = Localization.Get("ProfileWindow", "AddFriendSuccess"),
                [1] = Localization.Get("ProfileWindow", "CancelFriendRequestSuccess"),
                [2] = Localization.Get("ProfileWindow", "RemoveFriendSuccess"),
                [3] = Localization.Get("ProfileWindow", "RequestSendSuccess"),
                [4] = Localization.Get("Window", "Empty"),
            };

            FriendRequestPrompt = new(Localization.Get("ProfileWindow", "DecideFriendRequestPrompt"));
            FriendRequestPrompt.AutoReset = true;
            FriendRequestPrompt.OnCompletion += (BaseWindow window) => _ = OnFriendActionDecided(window);

            UnfriendPrompt = new(Localization.Get("ProfileWindow", "DecideUnfriendPrompt"));
            UnfriendPrompt.AutoReset = true;
            UnfriendPrompt.OnCompletion += (BaseWindow window) => _ = OnFriendActionDecided(window);
        }

        internal void CreateButtons()
        {
            StatsButton = AddButton(Localization.Get("ProfileWindow", "Stats"));
            FriendsButton = AddButton(Localization.Get("ProfileWindow", "Friends"), UIManager.FriendsWindow);

            if (Player == PlayerManager.LocalPlayer)
            {
                FriendRequestsButton = AddButton(Localization.Get("ProfileWindow", "FriendRequests"), UIManager.FriendRequestsWindow);
            }

            AchievementsButton = AddButton(Localization.Get("ProfileWindow", "Achievements"), UIManager.AchievementsWindow);
            //HQStatsButton = AddButton(Localization.Get("ProfileWindow", "HQStats")); No support for hq for now =(
            MDMoeButton = AddButton(Localization.Get("ProfileWindow", "MDMoe"));

            if (FriendButtonState != 4)
            {
                FriendActionButton = AddButton(Localization.Get("ProfileWindow", "AddFriend"));
            }

            AddRefreshButton();
            AddReturnButton();
        }

        internal void CreateAvatarBox()
        {
            if (AvatarBox != null) return;
            AvatarBox = UnityEngine.Object.Instantiate(
                GameObject.Find("UI/Forward/Tips/PnlHead").GetComponent<PnlHead>().headGridView.templateHeadItem.m_Button.gameObject,
                UIManager.WindowTitle.transform
            );
            AvatarBox.transform.localScale = new(0.8f, 0.8f, 0.8f);
            AvatarBox.transform.localPosition = new(333f, -166f, 0f);
            AvatarHeadItem = AvatarBox.GetComponent<HeadItem>();
            AvatarHeadItem.m_ImgLock.gameObject.SetActive(false);
        }

        /// <summary>
        /// Refreshes current <see cref="Data.Players.Player"/> and displays updated profile.
        /// </summary>
        private async Task Refresh()
        {
            UIManager.Debounce = true;

            await Update(Player, true);

            Main.Dispatcher.Enqueue(() =>
            {
                UIManager.Debounce = false;
                Window.Show();
            });
        }

        /// <summary>
        /// Opens player's profile in the browser.
        /// </summary>
        private void OpenProfilePage()
        {
            Utilities.OpenBrowserLink($"{Constants.ServerHTTPScheme}://{Constants.ServerAddress}/profile/{Player.Uid}");
        }

        /// <summary>
        /// Opens the <see href="https://musedash.moe"/> profile of the player in the browser.
        /// </summary>
        private void OpenMDMoe()
        {
            Utilities.OpenBrowserLink("https://musedash.moe/player/" + Player.Uid);
        }

        /// <summary>
        /// Updates the <see cref="ProfileWindow"/> to display the information about the given <see cref="Data.Players.Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Data.Players.Player"/> whose stats will now appear in the window.</param>
        /// <param name="updatePlayer">Whether to update the <see cref="Data.Players.Player"/> as well.</param>
        internal async Task Update(Player player, bool updatePlayer = true)
        {
            Player = player;
            if (updatePlayer)
            {
                await player.Update(true);
            }

            Main.Dispatcher.Enqueue(() =>
            {
                Player localPlayer = PlayerManager.LocalPlayer;

                FriendButtonState =
                    player == localPlayer ? 4 :
                    localPlayer.MultiplayerStats.FriendRequests.TryGetValue(player.Uid, out _) ? 0 :
                    player.MultiplayerStats.FriendRequests.TryGetValue(localPlayer.Uid, out _) ? 1 :
                    player.MultiplayerStats.Friends.Contains(localPlayer) || localPlayer.MultiplayerStats.Friends.Contains(player) ? 2 :
                    3;

                RemoveAllButtons();
                CreateButtons();

                StatsButton.Contents = new
                (
                    $"{player.MultiplayerStats.Bio}\n\n" +

                    $"[ LVL ]: <color=1eff00ff>{player.MultiplayerStats.Level}</color>\n" +
                    $"[ RL ]: <color=1eff00ff>{player.MoeStats.RL}</color>\n" +
                    $"[ ELO ]: <color=1eff00ff>{player.MultiplayerStats.ELO}</color>\n" +
                    $"[ Rank ]: <color=fff700ff>{player.MultiplayerStats.Rank}</color>\n" +
                    $"[ Records ]: <color=fff700ff>{player.MoeStats.Records}</color>\n" +
                    $"[ APs ]: <color=fff700ff>{player.MoeStats.APs}</color>\n" +
                    $"[ Average Accuracy ]: <color=fff700ff>{player.MoeStats.AverageAccuracy}%</color>"
                );
                AchievementsButton.Contents = StatsButton.Contents;
                FriendsButton.Contents = StatsButton.Contents;
                MDMoeButton.Contents = StatsButton.Contents;
                RefreshButton.Contents = StatsButton.Contents;
                ReturnButton.Contents = StatsButton.Contents;

                if (FriendActionButton != null)
                {
                    FriendActionButton.Contents = StatsButton.Contents;
                }

                if (FriendRequestsButton != null)
                {
                    FriendRequestsButton.Contents = StatsButton.Contents;
                }

                UIManager.FriendsWindow.Update(player);
                UIManager.FriendRequestsWindow.Update();
                UIManager.AchievementsWindow.Update(player);

                Title = player.MultiplayerStats.NameLocal;
                AvatarHeadItem.m_ImgHead.sprite = PnlHead.GetSprite(player.MultiplayerStats.AvatarName);

                FriendActionButton.Titles = FriendButtonTitles[FriendButtonState];
                FriendRequestPrompt.Title = player.MultiplayerStats.NameLocal;
                UnfriendPrompt.Title = player.MultiplayerStats.NameLocal;
            });
        }

        private async Task OnFriendActionDecided(BaseWindow window = null)
        {
            if (window == null || window == FriendRequestPrompt && FriendRequestPrompt.Result == true || window == UnfriendPrompt && UnfriendPrompt.Result == true)
            {
                UIManager.Debounce = true;

                var payload = new
                {
                    PlayerManager.LocalPlayer.Uid,
                    Client.Token,
                    FriendUid = Player.Uid
                };

                LocalString msg;
                var response = await Client.PostAsync("friendRequest", payload);

                if (response != null)
                {
                    var actionDid = await response.Content.ReadFromJsonAsync<int>();
                    msg = FriendButtonResponses[FriendButtonState];
                }
                else
                {
                    msg = Localization.Get("Warning", "Unknown");
                }

                if (Player != PlayerManager.LocalPlayer)
                {
                    await PlayerManager.LocalPlayer.Update();
                }
                await Update(Player, true);

                PopupUtils.ShowInfo(msg);

                UIManager.Debounce = false;
            }

            Window.Show();
        }

        protected override void OnButtonClick(PopupLib.UI.Windows.Interfaces.IListWindow window, int objectIndex)
        {
            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == FriendActionButton && FriendButtonState == 4) return;
            else if (button == StatsButton)
            {
                OpenProfilePage();
                return;
            }
            else if (button == MDMoeButton)
            {
                OpenMDMoe();
                return;
            }

            base.OnButtonClick(window, objectIndex);

            if (button == FriendActionButton)
            {
                if (FriendButtonState == 0)
                {
                    FriendRequestPrompt.Show();
                }
                else if (FriendButtonState == 2)
                {
                    UnfriendPrompt.Show();
                }
                else if (FriendButtonState != 4)
                {
                    _ = OnFriendActionDecided();
                }
            }
            else if (button == RefreshButton)
            {
                _ = Refresh();
            }
        }

        protected override void OnShow(BaseWindow window)
        {
            base.OnShow(window);
            AvatarBox.SetActive(true);
        }

        protected override void OnCompletion(BaseWindow window)
        {
            base.OnCompletion(window);
            AvatarBox.SetActive(false);
        }
    }
}
