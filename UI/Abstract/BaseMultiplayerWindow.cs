using Il2CppAssets.Scripts.UI.Tips;
using LocalizeLib;
using Multiplayer.Data;
using Multiplayer.Managers;
using Multiplayer.Static;
using Multiplayer.UI.Extensions;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI.Abstract
{
    internal abstract class BaseMultiplayerWindow
    {
        internal LocalString Title { get; set; }
        internal ForumWindow Window { get; private set; }

        internal static Image BannerImageComponent => GameObject.Find("UI/Forward/Tips/PnlBulletinNew/ImgBase/ScrollView/Viewport/Content/Image").GetComponent<Image>();
        internal CustomImageAsset Banner { get; private set; }

        internal BaseMultiplayerWindow ReturnWindow { get; set; }
        internal bool HasReturnWindow => ReturnWindow != null;

        internal Dictionary<ForumObject, object> ButtonsWindows { get; private set; }

        /// <param name="title">Title of the window.</param>
        /// <param name="returnWindow">(Optional) Window to open after this one is closed.</param>
        /// <param name="bannerAssetName">(Optional) Path to the image asset relative to "Multiplayer.Assets.UI.Banners".</param>
        internal BaseMultiplayerWindow(LocalString title, BaseMultiplayerWindow returnWindow = null, string bannerAssetName = null)
        {
            Title = title;
            if (bannerAssetName != null) Banner = AssetManager.GetImageAsset("UI.Banners." + bannerAssetName);

            ReturnWindow = returnWindow;
            ButtonsWindows = new();

            Window = new();
            Window.AutoReset = true;

            Window.OnSelectionChanged += OnButtonClick;
            Window.OnInternalShow += OnShow;
            Window.OnCompletion += OnCompletion;
        }

        /// <summary>
        /// Adds a new button to the window.
        /// </summary>
        /// <param name="buttonName">Localized text to be displayed on the button.</param>
        /// <param name="windowToOpen">(Optional) Window to open when the button is pressed.</param>
        /// <param name="content">(Optional) Text to be displayed on the main frame.</param>
        /// <returns>A new button.</returns>
        protected ForumObject AddButton(LocalString buttonName, object windowToOpen = null, LocalString content = null)
        {
            if (content == null) content = new();

            ForumObject button = new(buttonName, content);
            button.Texture = Banner?.Texture;

            Window.ForumObjects.Add(button);
            ButtonsWindows.Add(button, windowToOpen);

            return button;
        }

        /// <summary>
        /// Adds an empty button saying there is nothing to view.
        /// </summary>
        /// <param name="description">(Optional) Description of the <see cref="ForumObject"/> button.</param>
        /// <returns>A new button.</returns>
        internal ForumObject AddEmptyButton(LocalString description = null)
        {
            return AddButton(Localization.Get("Window", "Empty"), ReturnWindow ?? this, description ?? Localization.Get("Window", "EmptyDescription"));
        }

        /// <summary>
        /// Removes the <see cref="ForumObject"/> <paramref name="button"/>.
        /// </summary>
        /// <returns><see langword="true"/> if it was removed, otherwise <see langword="false"/>.</returns>
        protected bool RemoveButton(ForumObject button)
        {
            if (button is null) return false;

            return ButtonsWindows.Remove(button) && Window.ForumObjects.Remove(button);
        }

        /// <summary>
        /// Removes all buttons of the window.
        /// </summary>
        /// <param name="keepButtons">An array of buttons which will be kept.</param>
        /// <returns><see langword="true"/> if all buttons were successfully removed, otherwise <see langword="false"/>.</returns>
        protected bool RemoveAllButtons(ForumObject[] keepButtons = null)
        {
            bool success = true;
            List<ForumObject> toRemove = new();
            foreach (ForumObject button in Window.ForumObjects)
            {
                if (keepButtons != null && button != null && keepButtons.Contains(button)) continue;
                toRemove.Add(button);
            }
            foreach (ForumObject button in toRemove)
            {
                success = RemoveButton(button) && success;
            }
            toRemove.Clear();
            return success;
        }

        /// <summary>
        /// Calls every time the back button gets pressed.
        /// </summary>
        internal void OnReturn()
        {
            Window.ForceClose();
            if (HasReturnWindow) ReturnWindow.Window.Show();
        }

        /// <summary>
        /// Closes and then opens the window back.
        /// </summary>
        internal virtual void OnRefresh()
        {
            Window.ForceClose();
            Window.Show();
        }

        protected virtual void OnButtonClick(PopupLib.UI.Windows.Interfaces.IListWindow _, int objectIndex)
        {
            if (!Window.Activated) return;
            ForumObject button = Window.ForumObjects[objectIndex];

            Window.ForceClose();

            object windowToOpen = ButtonsWindows[button];
            if (windowToOpen is null)
            {
                return;
            }
            else if (windowToOpen is MainMenu)
            {
                ((MainMenu)windowToOpen).Open();
            }
            else if (windowToOpen is BaseMultiplayerWindow)
            {
                ((BaseMultiplayerWindow)windowToOpen).Window.Show();
            }
            else if (windowToOpen is BaseWindow)
            {
                ((BaseWindow)windowToOpen).Show();
            }
            else if (windowToOpen is AbstractMessageBox)
            {
                ((AbstractMessageBox)windowToOpen).Show();
            }
        }

        protected virtual void OnShow(BaseWindow _)
        {
            if (Title is null) return;
            BulletinExtension.CurrentWindow = this;
            BulletinExtension.WindowTitle.text = Title.ToString();
            BulletinExtension.Toggle(true);
        }

        protected virtual void OnCompletion(BaseWindow _)
        {
            BulletinExtension.CurrentWindow = null;
            BulletinExtension.Toggle(false);
        }
    }
}
