using Il2CppAssets.Scripts.UI.Panels;
using Multiplayer.Data;
using Multiplayer.Managers;
using PopupLib.UI.Components;
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

        private static PnlHead PnlHead => GameObject.Find("UI/Forward/Tips/PnlHead").GetComponent<PnlHead>();

        // The Player whose stats are displayed in this window.
        internal Player Player;

        internal ProfileWindow() : base(UIManager.MainMenu) {}

        internal void CreateButtons()
        {
            RankButton = AddButton(Localization.Get("ProfileWindow", "Rank"));
            AvatarButton = AddButton(Localization.Get("ProfileWindow", "Avatar"), PnlHead);
            FriendsButton = AddButton(Localization.Get("ProfileWindow", "Friends"), UIManager.FriendsWindow);
            AchievementsButton = AddButton(Localization.Get("ProfileWindow", "Achievements"), UIManager.AchievementsWindow);
            //HQStatsButton = AddButton(Localization.Get("ProfileWindow", "HQStats")); No support for hq for now :(
            MoeStatsButton = AddButton(Localization.Get("ProfileWindow", "MoeStats"), UIManager.MoeStatsWindow);
            AddReturnButton();
        }

        /// <summary>
        /// Updates the profile window to display the information about the given <see cref="Data.Player"/>.
        /// </summary>
        /// <param name="player"><see cref="Data.Player"/> whose stats will now appear in the window.</param>
        internal void Update(Player player)
        {
            Player = player;
            RankButton.Contents = new($"ELO: {Player.MultiplayerStats.ELO}\n\nRank: {Player.MultiplayerStats.Rank}");
        }
    }
}
