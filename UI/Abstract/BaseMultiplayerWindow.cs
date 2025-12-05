using Il2CppAssets.Scripts.UI.Tips;
using LocalizeLib;
using Multiplayer.Data;
using Multiplayer.Managers;
using Multiplayer.Static;
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

        internal ForumObject ReturnButton { get; private set; }
        internal bool HasReturnButton => ReturnButton != null;
        internal BaseMultiplayerWindow ReturnWindow { get; set; }
        internal bool HasReturnWindow => ReturnWindow != null;
        internal ForumObject RefreshButton { get; private set; }
        internal bool HasRefreshButton => RefreshButton != null;

        internal Dictionary<ForumObject, object> ButtonsWindows { get; private set; }

        /// <param name="title">Title of the window.</param>
        /// <param name="returnWindow">(Optional) Window to open after this one is closed.</param>
        /// <param name="bannerAssetName">(Optional) Path to the image asset relative to "Multiplayer.Assets.UI".</param>
        internal BaseMultiplayerWindow(LocalString title, BaseMultiplayerWindow returnWindow = null, string bannerAssetName = null)
        {
            Title = title;
            if (bannerAssetName != null) Banner = AssetManager.GetImageAsset("UI." + bannerAssetName);

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
        /// Removes the button with the specified <paramref name="objectIndex"/>.
        /// </summary>
        /// <param name="objectIndex">Index of the button.</param>
        /// <returns><see langword="true"/> if it was removed, otherwise <see langword="false"/>.</returns>
        protected bool RemoveButton(ForumObject button)
        {
            if (button == null) return false;

            if (button == ReturnButton)
            {
                ReturnButton = null;
            }
            else if (button == RefreshButton)
            {
                RefreshButton = null;
            }

            return ButtonsWindows.Remove(button) && Window.ForumObjects.Remove(button);
        }

        /// <summary>
        /// Removes all buttons of the window.
        /// </summary>
        /// <param name="keepCoreButtons">Whether to keep the ReturnButton and the RefreshButton and not remove it.</param>
        /// <param name="keepButtons">An array of buttons which need to be kept.</param>
        /// <returns><see langword="true"/> if all buttons were successfully removed, otherwise <see langword="false"/>.</returns>
        protected bool RemoveAllButtons(bool keepCoreButtons = false, ForumObject[] keepButtons = null)
        {
            bool success = true;
            List<ForumObject> toRemove = new();
            foreach (ForumObject button in Window.ForumObjects)
            {
                if (keepButtons != null && button != null && keepButtons.Contains(button)) continue;
                if (keepCoreButtons && (button == ReturnButton || button == RefreshButton)) continue;
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
        /// Adds the refresh button which updates the current window (Refreshing logic must be implemented separately).
        /// </summary>
        protected void AddRefreshButton()
        {
            if (RefreshButton != null) return;
            RefreshButton = AddButton(Localization.Get("Window", "RefreshButton"), Window);
        }

        /// <summary>
        /// Adds the return button which closes the current window and opens the return window (if exists).
        /// </summary>
        /// <param name="content">(Optional) Text to be displayed on the main frame.</param>
        protected void AddReturnButton(LocalString content = null)
        {
            if (ReturnButton != null) return;
            ReturnButton = AddButton(Localization.Get("Window", HasReturnWindow ? "ReturnButton" : "ExitButton"), null, content);
        }

        /// <summary>
        /// Closes and then opens the window back.
        /// </summary>
        protected void RefreshWindow()
        {
            Window.ForceClose();
            Window.Show();
        }

        protected virtual void OnButtonClick(PopupLib.UI.Windows.Interfaces.IListWindow window, int objectIndex)
        {
            if (!Window.Activated) return;
            ForumObject button = Window.ForumObjects[objectIndex];

            Window.ForceClose();

            if (HasReturnWindow && button == ReturnButton)
            {
                ReturnWindow.Window.Show();
            }
            else
            {
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
        }

        protected virtual void OnShow(BaseWindow window)
        {
            if (Title is null) return;
            UIManager.WindowTitle.text = Title.ToString();
            UIManager.WindowTitle.gameObject.SetActive(true);
        }

        protected virtual void OnCompletion(BaseWindow window)
        {
            UIManager.WindowTitle.gameObject.SetActive(false);
        }
    }
}
