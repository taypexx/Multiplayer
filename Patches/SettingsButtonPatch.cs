using System;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppAssets.Scripts.UI.Panels;
using Il2CppAccount;

namespace Multiplayer.Patches
{
    internal static class SettingsButtonPatch
    {
        private const string MultiplayerButtonName = "ModMultiplayerButton";
        internal static GameObject _multiplayerBtn = null;

        [HarmonyPatch(typeof(PnlMenu), nameof(PnlMenu.OnEnable))]
        internal static class PnlMenuOnEnablePatch
        {
            private static void Postfix(PnlMenu __instance)
            {
                try
                {
                    var pnlOption = __instance.transform.Find("Panels/PnlOption");
                    var togglesParent = pnlOption?.Find("Toggles");
                    
                    if (togglesParent == null) return;

                    var templateBtn = togglesParent.Find("BtnFeedback");
                    if (templateBtn == null) return;

                    if (togglesParent.Find(MultiplayerButtonName) != null) return;

                    // Create multiplayer settings button
                    _multiplayerBtn = GameObject.Instantiate(templateBtn.gameObject, togglesParent);
                    _multiplayerBtn.name = MultiplayerButtonName;
                    _multiplayerBtn.SetActive(true);
                    _multiplayerBtn.transform.SetAsLastSibling();

                    // Clean up unneeded components
                    var keyBinding = _multiplayerBtn.GetComponent("InputKeyBinding");
                    if (keyBinding != null) GameObject.DestroyImmediate(keyBinding);

                    var localizations = _multiplayerBtn.GetComponentsInChildren<Component>(true);
                    if (localizations != null)
                    {
                        foreach (var loc in localizations)
                        {
                            if (loc != null && loc.GetIl2CppType().Name == "Localization")
                                GameObject.DestroyImmediate(loc);
                        }
                    }

                    // Modify button text
                    var txtChild = _multiplayerBtn.transform.Find("TxtFeedback");
                    if (txtChild != null)
                    {
                        var txt = txtChild.GetComponent<Text>();
                        if (txt != null) txt.text = "MDMP";
                    }

                    // Bind click event
                    var button = _multiplayerBtn.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener((UnityAction)new Action(() =>
                        {
                            bool current = Multiplayer.Static.Settings.Get<bool>("JailbreakMode");
                            bool newValue = !current;
                            var setting = Multiplayer.Static.Settings.Config.First(s => s.Name == "JailbreakMode") as Multiplayer.Data.Settings.Setting<bool>;
                            if (setting != null)
                            {
                                setting.Value = newValue;
                            }
                            string msg = newValue ? "MDMP: Jailbreak Mode Enabled (Score upload blocked)" : "MDMP: Jailbreak Mode Disabled";
                            Il2CppAssets.Scripts.UI.Controls.ShowText.ShowInfo(msg);
                        }));
                    }

                    // Add canvas group
                    if (_multiplayerBtn.GetComponent<CanvasGroup>() == null)
                        _multiplayerBtn.AddComponent<CanvasGroup>();
                    
                    var cg = _multiplayerBtn.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 0f;

                    // Listen to back button
                    if (__instance.backBtn != null)
                    {
                        __instance.backBtn.onClick.AddListener((UnityAction)new Action(() =>
                        {
                            StartFadeOut();
                        }));
                    }

                    // Refresh navigation
                    var optionSelect = pnlOption.GetComponent<OptionSelect>();
                    if (optionSelect != null) optionSelect.SetSelectableObj();

                    MelonLogger.Msg("[Multiplayer] Settings button setup complete.");
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"[Multiplayer] Settings button injection failed: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(PnlMenu), "OnBackClicked")]
        internal static class PnlMenuOnBackClickedPatch
        {
            private static void Prefix()
            {
                StartFadeOut();
            }
        }

        [HarmonyPatch(typeof(OptionSelect), "OnSelect")]
        internal static class OptionSelectOnSelectPatch
        {
            private static void Postfix(GameObject currentObj)
            {
                if (_multiplayerBtn != null && currentObj == _multiplayerBtn)
                {
                    var sel = currentObj.transform.Find("ImgSelected");
                    if (sel != null)
                    {
                        sel.gameObject.SetActive(true);
                        if (sel.childCount > 0) sel.GetChild(0).gameObject.SetActive(true);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(OptionSelect), "OnEnablePnl")]
        internal static class OptionSelectOnEnablePatch
        {
            private static void Postfix()
            {
                if (_multiplayerBtn != null)
                {
                    _multiplayerBtn.SetActive(true);
                    MelonCoroutines.Start(DelayedEntrance());
                }
            }

            private static IEnumerator DelayedEntrance()
            {
                var cg = _multiplayerBtn.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 0f;
                yield return new WaitForSeconds(0.3f);
                var animator = _multiplayerBtn.GetComponent<Animator>();
                if (animator != null) animator.Play("Show", 0, 0f);
                float elapsed = 0f;
                while (elapsed < 0.2f)
                {
                    elapsed += Time.deltaTime;
                    if (cg != null) cg.alpha = elapsed / 0.2f;
                    yield return null;
                }
                if (cg != null) cg.alpha = 1f;
            }
        }

        [HarmonyPatch(typeof(OptionSelect), "SetSelectableObj")]
        internal static class OptionSelectSetSelectablePatch
        {
            private static void Postfix(ref Il2CppSystem.Collections.Generic.List<GameObject> __result)
            {
                if (_multiplayerBtn != null && !__result.Contains(_multiplayerBtn))
                {
                    __result.Add(_multiplayerBtn);
                }
            }
        }

        internal static void StartFadeOut()
        {
            if (_multiplayerBtn != null && _multiplayerBtn.activeSelf)
            {
                MelonCoroutines.Start(FadeOutRoutine());
            }
        }

        private static IEnumerator FadeOutRoutine()
        {
            var cg = _multiplayerBtn.GetComponent<CanvasGroup>();
            if (cg == null) yield break;

            var animator = _multiplayerBtn.GetComponent<Animator>();
            if (animator != null) animator.Play("Hide", 0, 0f);

            float elapsed = 0f;
            while (elapsed < 0.2f)
            {
                elapsed += Time.deltaTime;
                cg.alpha = 1f - (elapsed / 0.2f);
                yield return null;
            }
            cg.alpha = 0f;
            _multiplayerBtn.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(GameAccountSystem), nameof(GameAccountSystem.UploadScore))]
    internal static class OfficialUploadScorePatch
    {
        private static bool Prefix()
        {
            if (Multiplayer.Static.Settings.Get<bool>("JailbreakMode"))
            {
                MelonLogger.Msg("Jailbreak Mode active: blocking official score upload.");
                return false;
            }
            return true;
        }
    }
}
