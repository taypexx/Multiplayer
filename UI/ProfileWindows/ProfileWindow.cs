using Il2Cpp;
using Il2CppAssets.Scripts.Database;
using Il2CppAssets.Scripts.UI.Panels;
using Il2CppSirenix.Serialization.Utilities;
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
using UnityEngine.Events;
using UnityEngine.UI;

namespace Multiplayer.UI.ProfileWindows
{
    internal sealed class ProfileWindow : BaseMultiplayerWindow
    {
        private ForumObject StatsButton;
        private ForumObject AvatarButton;
        private ForumObject BioButton;
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

        private static bool PnlHeadWasOpened = false;
        private GameObject AvatarBox;
        private HeadItem AvatarHeadItem;

        private InputWindow BioWindow;

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

            BioWindow = new(Localization.Get("ProfileWindow", "BioDescription"));
            BioWindow.AutoReset = true;
            BioWindow.OnCompletion += (BaseWindow window) => _ = OnBioCompletion();
        }

        internal void CreateButtons()
        {
            StatsButton = AddButton(Localization.Get("ProfileWindow", "Stats"));

            if (Player == PlayerManager.LocalPlayer)
            {
                AvatarButton = AddButton(Localization.Get("ProfileWindow", "Avatar"));
                BioButton = AddButton(Localization.Get("ProfileWindow", "Bio"), BioWindow);
                FriendRequestsButton = AddButton(Localization.Get("ProfileWindow", "FriendRequests"), UIManager.FriendRequestsWindow);
            }

            FriendsButton = AddButton(Localization.Get("ProfileWindow", "Friends"));
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

        /// <summary>
        /// Creates the avatar box that shows the current avatar of a <see cref="Data.Players.Player"/>.
        /// </summary>
        internal void CreateAvatarBox()
        {
            if (AvatarBox != null) return;
            AvatarBox = UnityEngine.Object.Instantiate(
                UIManager.PnlHead.headGridView.templateHeadItem.m_Button.gameObject,
                UIManager.MainFrame.transform.Find("ImgBase")
            );
            AvatarBox.name = "AvatarBox";

            var rect = AvatarBox.GetComponent<RectTransform>();
            rect.pivot = new(1f, 1f);
            rect.anchorMin = rect.pivot;
            rect.anchorMax = rect.pivot;
            rect.anchoredPosition = new(-600f, -95f);

            var button = AvatarBox.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener((UnityAction) new Action(OpenPnlHead));

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
        /// Updates the <see cref="FriendsWindow"/> and opens it.
        /// </summary>
        private async Task OpenFriendsWindow()
        {
            UIManager.Debounce = true;
            await UIManager.FriendsWindow.Update(Player);
            UIManager.Debounce = false;

            Main.Dispatcher.Enqueue(() => UIManager.FriendsWindow.Window.Show());
        }

        /// <summary>
        /// Opens the <see href="https://musedash.moe"/> profile of the player in the browser.
        /// </summary>
        private void OpenMDMoe()
        {
            Utilities.OpenBrowserLink("https://musedash.moe/player/" + Player.Uid);
        }

        /// <summary>
        /// Opens <see cref="PnlHead"/> the right way (otherwise it breaks).
        /// </summary>
        private void OpenPnlHead()
        {
            if (UIManager.PnlHead == null) return;
            if (Window.Activated) Window.ForceClose();

            PnlHeadWasOpened = true;
            UIManager.PnlAchvOther.OnHeadClick();
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
                await player.Update();
            }

            Main.Dispatcher.Enqueue(() =>
            {
                Player localPlayer = PlayerManager.LocalPlayer;

                FriendButtonState =
                    player == localPlayer ? 4 :
                    localPlayer.MultiplayerStats.FriendRequests.TryGetValue(player.Uid, out _) ? 0 :
                    player.MultiplayerStats.FriendRequests.TryGetValue(localPlayer.Uid, out _) ? 1 :
                    player.MultiplayerStats.Friends.Contains(localPlayer.Uid) || localPlayer.MultiplayerStats.Friends.Contains(player.Uid) ? 2 :
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
                    $"[ Records ]: <color=fff700ff>{player.TotalRecords}</color>\n" +
                    $"[ APs ]: <color=fff700ff>{player.TotalAPs}</color>\n" +
                    $"[ Average Accuracy ]: <color=fff700ff>{player.TotalAverageAccuracy}%</color>"
                );
                AchievementsButton.Contents = StatsButton.Contents;
                FriendsButton.Contents = StatsButton.Contents;
                MDMoeButton.Contents = StatsButton.Contents;
                RefreshButton.Contents = StatsButton.Contents;
                ReturnButton.Contents = StatsButton.Contents;

                if (AvatarButton != null)
                {
                    AvatarButton.Contents = StatsButton.Contents;
                }

                if (BioButton != null)
                {
                    BioButton.Contents = StatsButton.Contents;
                }

                if (FriendActionButton != null)
                {
                    FriendActionButton.Contents = StatsButton.Contents;
                }

                if (FriendRequestsButton != null)
                {
                    FriendRequestsButton.Contents = StatsButton.Contents;
                }

                UIManager.FriendRequestsWindow.Update();
                UIManager.AchievementsWindow.Update(player);

                Title = (LocalString)player.MultiplayerStats.Name;
                AvatarHeadItem.m_ImgHead.sprite = PnlHead.GetSprite(player.MultiplayerStats.AvatarName);

                FriendActionButton.Titles = FriendButtonTitles[FriendButtonState];
                FriendRequestPrompt.Title = (LocalString)player.MultiplayerStats.Name;
                UnfriendPrompt.Title = (LocalString)player.MultiplayerStats.Name;
            });
        }

        /// <summary>
        /// Calls every time the bio window gets closed.
        /// </summary>
        private async Task OnBioCompletion()
        {
            if (BioWindow.Result.IsNullOrWhitespace()) goto Show;

            if (BioWindow.Result.Length > Constants.BioCharactersMax)
            {
                PopupUtils.ShowInfo(String.Format(Localization.Get("ProfileWindow", "BioTooLong").ToString(), Constants.BioCharactersMax));
                goto Show;
            }

            PlayerManager.LocalPlayer.MultiplayerStats.Bio = BioWindow.Result;
            PlayerManager.SyncProfile();
            await Update(Player, false);

            Show:
            Main.Dispatcher.Enqueue(() => Window.Show());
        }

        /// <summary>
        /// Calls every time <see cref="PnlHead"/> gets closed.
        /// </summary>
        internal async Task OnPnlHeadClose()
        {
            if (PlayerManager.LocalPlayer is null) return;

            string newAvatarName = "head_" + DataHelper.selectedHeadIndex.ToString();
            if (PlayerManager.LocalPlayer.MultiplayerStats.AvatarName != newAvatarName)
            {
                PlayerManager.LocalPlayer.MultiplayerStats.AvatarName = newAvatarName;
                PlayerManager.SyncProfile();
                await Update(Player, false);
            }

            if (PnlHeadWasOpened)
            {
                PnlHeadWasOpened = false;
                Window.Show();
            }
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

                if (PnlHomeExtension.Visible) PnlHomeExtension.UpdateCurrentPage();

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
                UIManager.OpenProfilePage(Player.Uid); return;
            }
            else if (button == MDMoeButton)
            {
                OpenMDMoe(); return;
            }

            base.OnButtonClick(window, objectIndex);

            if (button == AvatarButton)
            {
                OpenPnlHead();
            }
            else if (button == FriendActionButton)
            {
                switch (FriendButtonState)
                {
                    case 0:
                        FriendRequestPrompt.Show();
                        break;
                    case 2:
                        UnfriendPrompt.Show();
                        break;
                    case 4:
                        break;
                    default:
                        _ = OnFriendActionDecided();
                        break;
                }
            } 
            else if (button == FriendsButton) _ = OpenFriendsWindow();
            else if (button == RefreshButton) _ = Refresh();
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
