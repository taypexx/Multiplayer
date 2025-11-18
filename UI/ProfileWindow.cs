using Il2Cpp;
using Il2CppAssets.Scripts.UI.Panels;
using LocalizeLib;
using Multiplayer.Data;
using Multiplayer.Managers;
using PopupLib.UI;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using System.Diagnostics;
using System.Net.Http.Json;
using UnityEngine;

namespace Multiplayer.UI
{
    internal sealed class ProfileWindow : BaseMultiplayerWindow
    {
        private ForumObject StatsButton;
        private ForumObject FriendsButton;
        private ForumObject AchievementsButton;
        private ForumObject HQStatsButton;
        private ForumObject MDMoeButton;
        private ForumObject FriendRequestButton;

        private PromptWindow FriendRequestPrompt;
        private PromptWindow UnfriendPrompt;

        private int FriendButtonState;
        private Dictionary<int, LocalString> FriendButtonTitles;
        private Dictionary<int, LocalString> FriendButtonResponses;

        private GameObject AvatarBox;
        private HeadItem AvatarHeadItem;

        // The Player whose stats are displayed in this window.
        internal Player Player;

        internal ProfileWindow() : base(Localization.Get("ProfileWindow","Title"), UIManager.MainMenu, "Profile.png") 
        {
            FriendButtonTitles = new()
            {
                [0] = Localization.Get("ProfileWindow", "DecideFriendRequest"),
                [1] = Localization.Get("ProfileWindow", "CancelFriendRequest"),
                [2] = Localization.Get("ProfileWindow", "RemoveFriend"),
                [3] = Localization.Get("ProfileWindow", "RequestSend"),
                [4] = null,
            };

            FriendButtonResponses = new()
            {
                [0] = Localization.Get("ProfileWindow", "AddFriendSuccess"),
                [1] = Localization.Get("ProfileWindow", "CancelFriendRequestSuccess"),
                [2] = Localization.Get("ProfileWindow", "RemoveFriendSuccess"),
                [3] = Localization.Get("ProfileWindow", "RequestSendSuccess"),
                [4] = null
            };

            FriendRequestPrompt = new(Localization.Get("ProfileWindow", "DecideFriendRequestPrompt"));
            FriendRequestPrompt.AutoReset = true;
            FriendRequestPrompt.OnCompletion += OnFriendActionDecided;

            UnfriendPrompt = new(Localization.Get("ProfileWindow", "DecideUnfriendPrompt"));
            UnfriendPrompt.AutoReset = true;
            UnfriendPrompt.OnCompletion += OnFriendActionDecided;
        }

        internal void CreateButtons()
        {
            StatsButton = AddButton(Localization.Get("ProfileWindow", "Stats"));
            FriendsButton = AddButton(Localization.Get("ProfileWindow", "Friends"), UIManager.FriendsWindow);
            AchievementsButton = AddButton(Localization.Get("ProfileWindow", "Achievements"), UIManager.AchievementsWindow);
            //HQStatsButton = AddButton(Localization.Get("ProfileWindow", "HQStats")); No support for hq for now =(
            MDMoeButton = AddButton(Localization.Get("ProfileWindow","MDMoe"));
            FriendRequestButton = AddButton(Localization.Get("ProfileWindow", "AddFriend"));
            AddRefreshButton();
            AddReturnButton();
        }

        internal void CreateAvatarBox()
        {
            AvatarBox = GameObject.Instantiate(
                GameObject.Find("UI/Forward/Tips/PnlHead").GetComponent<PnlHead>().headGridView.templateHeadItem.m_Button.gameObject,
                UIManager.WindowTitle.transform
            );
            AvatarBox.transform.localScale = new(0.8f, 0.8f, 0.8f);
            AvatarBox.transform.localPosition = new(333f, -166f, 0f);
            AvatarHeadItem = AvatarBox.GetComponent<HeadItem>();
            AvatarHeadItem.m_ImgLock.gameObject.SetActive(false);
        }

        /// <summary>
        /// Refreshes current <see cref="Data.Player"/> and displays updated profile.
        /// </summary>
        private async void Refresh()
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
        /// Opens the <see href="https://musedash.moe"/> profile of the player in the browser.
        /// </summary>
        private void OpenMDMoe()
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://musedash.moe/player/" + Player.Uid) { UseShellExecute = true });
            }
            catch (Exception e)
            {
                Main.Logger.Warning(e.ToString());
            }
        }

        /// <summary>
        /// Updates the <see cref="ProfileWindow"/> to display the information about the given <see cref="Data.Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Data.Player"/> whose stats will now appear in the window.</param>
        /// <param name="updatePlayer">Whether to update the <see cref="Data.Player"/> as well.</param>
        internal async Task Update(Player player, bool updatePlayer = true)
        {
            Player = player;
            if (updatePlayer)
            {
                await player.Update(true);
            }

            Main.Dispatcher.Enqueue(() =>
            {
                StatsButton.Contents = new
                (
                    $"{Player.MultiplayerStats.Bio}\n\n" +

                    $"[ LVL ]: <color=1eff00ff>{Player.MultiplayerStats.Level}</color>\n" +
                    $"[ RL ]: <color=1eff00ff>{Player.MoeStats.RL}</color>\n" +
                    $"[ ELO ]: <color=1eff00ff>{Player.MultiplayerStats.ELO}</color>\n" +
                    $"[ Rank ]: <color=fff700ff>{Player.MultiplayerStats.Rank}</color>\n" +
                    $"[ Records ]: <color=fff700ff>{Player.MoeStats.Records}</color>\n" +
                    $"[ APs ]: <color=fff700ff>{Player.MoeStats.APs}</color>\n" +
                    $"[ Average Accuracy ]: <color=fff700ff>{Player.MoeStats.AverageAccuracy}%</color>"
                );
                AchievementsButton.Contents = StatsButton.Contents;
                FriendsButton.Contents = StatsButton.Contents;
                FriendRequestButton.Contents = StatsButton.Contents;
                MDMoeButton.Contents = StatsButton.Contents;
                RefreshButton.Contents = StatsButton.Contents;
                ReturnButton.Contents = StatsButton.Contents;

                UIManager.FriendsWindow.Update(player);
                UIManager.AchievementsWindow.Update(player);

                Title = Player.MultiplayerStats.NameLocal;
                AvatarHeadItem.m_ImgHead.sprite = PnlHead.GetSprite(Player.MultiplayerStats.AvatarName);

                Player localPlayer = PlayerManager.LocalPlayer;

                FriendButtonState =
                    Player == localPlayer ? 4 :
                    localPlayer.MultiplayerStats.FriendRequests.TryGetValue(Player.Uid, out _) ? 0 :
                    Player.MultiplayerStats.FriendRequests.TryGetValue(localPlayer.Uid, out _) ? 1 :
                    Player.MultiplayerStats.Friends.Contains(localPlayer) || localPlayer.MultiplayerStats.Friends.Contains(Player) ? 2 :
                    3;

                FriendRequestButton.Titles = FriendButtonTitles[FriendButtonState];
                FriendRequestPrompt.Title = Player.MultiplayerStats.NameLocal;
                UnfriendPrompt.Title = Player.MultiplayerStats.NameLocal;
            });
        }

        private async void OnFriendActionDecided(BaseWindow window = null)
        {
            if (window == null || (window == FriendRequestPrompt && FriendRequestPrompt.Result == true) || (window == UnfriendPrompt && UnfriendPrompt.Result == true))
            {
                UIManager.Debounce = true;

                var payload = new
                {
                    Uid = PlayerManager.LocalPlayer.Uid,
                    Token = Client.Token,
                    FriendUid = Player.Uid
                };

                LocalString msg;
                var response = await Client.PostAsync("friendRequest", payload);

                if (response != null)
                {
                    var actionDid = await response.Content.ReadFromJsonAsync<int>();
                    msg = FriendButtonResponses[FriendButtonState];
                } else
                {
                    msg = Localization.Get("Warning", "Unknown");
                }

                if (Player != PlayerManager.LocalPlayer)
                {
                    await PlayerManager.LocalPlayer.Update();
                }
                await Update(Player, true);

                PopupUtils.ShowInfoAndLog(msg);

                UIManager.Debounce = false;
            }

            Window.Show();
        }

        internal override void OnButtonClick(PopupLib.UI.Windows.Interfaces.IListWindow window, int objectIndex)
        {
            ForumObject button = Window.ForumObjects[objectIndex];

            if (button == StatsButton) return;
            else if (button == MDMoeButton)
            {
                OpenMDMoe(); 
                return;
            }

            base.OnButtonClick(window, objectIndex);

            if (button == FriendRequestButton) 
            {
                if (FriendButtonState == 0)
                {
                    FriendRequestPrompt.Show();
                } else if (FriendButtonState == 2)
                {
                    UnfriendPrompt.Show();
                } else if (FriendButtonState != 4)
                {
                    OnFriendActionDecided();
                }
            } else if (button == RefreshButton)
            {
                Refresh();
            }
        }

        internal override void OnShow(BaseWindow window)
        {
            base.OnShow(window);
            AvatarBox.SetActive(true);
        }

        internal override void OnCompletion(BaseWindow window)
        {
            base.OnCompletion(window);
            AvatarBox.SetActive(false);
        }
    }
}
