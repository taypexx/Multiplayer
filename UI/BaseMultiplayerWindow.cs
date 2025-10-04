using Il2Cpp;
using Il2CppAssets.Scripts.UI.Tips;
using LocalizeLib;
using Multiplayer.Data;
using Multiplayer.Managers;
using PopupLib.UI.Components;
using PopupLib.UI.Windows;
using PopupLib.UI.Windows.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI
{
    internal abstract class BaseMultiplayerWindow
    {
        internal ForumWindow Window { get; private set; }
        internal static Image BannerImage => GameObject.Find("UI/Forward/Tips/PnlBulletinNew/ImgBase/ScrollView/Viewport/Content/Image").GetComponent<Image>();

        internal ForumObject ReturnButton { get; private set; }
        internal bool HasReturnButton => ReturnButton != null;
        internal BaseMultiplayerWindow ReturnWindow { get; private set; }
        internal bool HasReturnWindow => ReturnWindow != null;

        internal Dictionary<ForumObject,object> ButtonsWindows { get; private set; }

        /// <param name="returnWindow">Window to open after this one is closed.</param>
        internal BaseMultiplayerWindow(BaseMultiplayerWindow returnWindow = null) 
        {
            ReturnWindow = returnWindow;
            ButtonsWindows = new();

            Window = new();
            Window.AutoReset = true;

            Window.OnSelectionChanged += OnButtonClick;
            Window.OnCompletion += OnCompletion;
        }

        /// <summary>
        /// Adds a new button to the window.
        /// </summary>
        /// <param name="buttonName">Localized text to be displayed on the button.</param>
        /// <param name="windowToOpen">(Optional) Window to open when the button is pressed.</param>
        /// <param name="content">(Optional) Text to be displayed on the main frame.</param>
        /// <param name="bannerAssetName">(Optional) Asset name of the banner image relative to Assets.UI.</param>
        /// <returns>A new button.</returns>
        internal ForumObject AddButton(LocalString buttonName, object windowToOpen = null, LocalString content = null, string bannerAssetName = null)
        {
            if (content == null) { content = new(); }

            ForumObject button = new(buttonName, content);

            if (bannerAssetName != null)
            {
                CustomImageAsset banner = AssetManager.GetImageAsset("UI." + bannerAssetName);
                if (banner != null) 
                {
                    // Assigning it to the image component because otherwise unity GC will destroy it
                    BannerImage.sprite = banner.Sprite;
                    button.Texture = banner.Texture;
                }
            }

            Window.ForumObjects.Add(button);
            ButtonsWindows.Add(button, windowToOpen);

            return button;
        }

        /// <summary>
        /// Removes the button with the specified <paramref name="objectIndex"/>.
        /// </summary>
        /// <param name="objectIndex">Index of the button.</param>
        /// <returns><see langword="true"/> if it was removed, otherwise <see langword="false"/>.</returns>
        internal bool RemoveButton(int objectIndex)
        {
            ForumObject button = Window.ForumObjects[objectIndex];
            if (button == null) return false;

            if (button == ReturnButton)
            {
                ReturnButton = null;
            }

            return ButtonsWindows.Remove(button) && Window.ForumObjects.Remove(button);
        }

        /// <summary>
        /// Removes all buttons of the window.
        /// </summary>
        /// <param name="keepRemoveButton">Whether to keep the ReturnButton and not remove it.</param>
        /// <returns><see langword="true"/> if all buttons were successfully removed, otherwise <see langword="false"/>.</returns>
        internal bool RemoveAllButtons(bool keepRemoveButton = false)
        {
            bool success = true;
            for (byte i = 0; i < Window.ForumObjects.Count; i++)
            {
                if (keepRemoveButton && Window.ForumObjects[i] == ReturnButton) continue;
                success = RemoveButton(i) && success;
            }
            return success;
        }

        /// <summary>
        /// Adds the return button which closes the current window and opens the return window (if exists).
        /// </summary>
        /// <param name="content">(Optional) Text to be displayed on the main frame.</param>
        /// <param name="bannerAssetName">(Optional) Asset name of the banner image relative to Assets.UI.</param>
        internal void AddReturnButton(LocalString content = null, string bannerAssetName = null)
        {
            if (ReturnButton != null) return;
            ReturnButton = AddButton(Localization.Get("Window", HasReturnWindow ? "ReturnButton" : "ExitButton"),null,content,bannerAssetName);
        }

        internal virtual void OnReturnButtonClick()
        {
            Window.ForceClose();
            if (HasReturnWindow)
            {
                ReturnWindow.Window.Show();
            }
        }

        internal virtual void OnCompletion(BaseWindow window)
        {
            
        }

        internal virtual void OnButtonClick(PopupLib.UI.Windows.Interfaces.IListWindow window, int objectIndex)
        {
            ForumObject forumObject = Window.ForumObjects[objectIndex];

            if (forumObject == ReturnButton)
            {
                OnReturnButtonClick();
            } else
            {
                object windowToOpen = ButtonsWindows[forumObject];
                if (windowToOpen != null)
                {
                    Window.ForceClose();

                    if (windowToOpen is BaseMultiplayerWindow)
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
                    else return;
                }
            }
        }
    }
}
