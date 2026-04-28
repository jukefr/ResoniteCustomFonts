using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;

namespace CustomInspectorFonts;

public partial class CustomInspectorFonts
{
    public static partial class CustomInspectorFontsPatches
    {
        /// <summary>
        /// Set custom font on every new UIBuilder at construction time. This is the most comprehensive
        /// approach — every UIBuilder created anywhere gets our font set on its initial style, which
        /// means ALL subsequent elements created with that builder use our font, including titles,
        /// slot bars, headers, etc. Non-inspector UIs are skipped by <c>ShouldApplyCustomFontsForUiSlot</c>.
        /// </summary>
        [HarmonyPatch]
        private static class UiBuilderConstructorPatch
        {
            [HarmonyTargetMethod]
            private static MethodBase? TargetMethod()
            {
                var uiBuilderType = UiBuilderType;
                if (uiBuilderType == null)
                    return null;
                // Patch the main constructor (UIBuilder(Slot root, Slot forceNext = null))
                // which all other constructors delegate to
                return uiBuilderType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();
            }

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(object __instance)
            {
                try
                {
                    if (!CustomInspectorFonts.ActiveEnabled())
                        return;
                    TryApplyFontToUiBuilder(__instance, preferBoldChainWhenBolderStackActive: false);
                }
                catch (Exception ex)
                {
                    Warn($"UIBuilder ctor postfix: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Set custom font early — before any elements are created — so that titles and other
        /// pre-<c>SetupEditorStyle</c> elements (like the title in <c>RadiantUI_Panel.SetupPanel</c>)
        /// use our font from the start. Runs as a postfix on <c>SetupDefaultStyle</c> which is
        /// called at the top of every panel setup, before any UI elements are laid out.
        /// </summary>
        [HarmonyPatch]
        private static class SetupDefaultStylePatch
        {
            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod()
            {
                var t = AccessTools.TypeByName("FrooxEngine.RadiantUI_Constants");
                var uiBuilder = UiBuilderType;
                if (t == null || uiBuilder == null)
                    return null;
                return AccessTools.Method(t, "SetupDefaultStyle", new[] { uiBuilder, typeof(bool) })
                    ?? AccessTools.Method(t, "SetupDefaultStyle", new[] { uiBuilder });
            }

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(object ui)
            {
                try
                {
                    if (!CustomInspectorFonts.ActiveEnabled())
                        return;
                    // Set font early — no bolder stack active here, so use primary font
                    TryApplyFontToUiBuilder(ui, preferBoldChainWhenBolderStackActive: false);
                }
                catch (Exception ex)
                {
                    Warn($"SetupDefaultStyle postfix: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Editor UI sets style font here; postfix sets <c>Style.Font</c> from the inspector root chain when available.
        /// </summary>
        [HarmonyPatch]
        private static class SetupEditorStylePatch
        {
            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod()
            {
                var t = AccessTools.TypeByName("FrooxEngine.RadiantUI_Constants");
                var uiBuilder = UiBuilderType;
                if (t == null || uiBuilder == null)
                    return null;
                return AccessTools.Method(t, "SetupEditorStyle", new[] { uiBuilder, typeof(bool) })
                    ?? AccessTools.Method(t, "SetupEditorStyle", new[] { uiBuilder });
            }

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(ref bool __state)
            {
                // Bolder stack is pushed only from targeted creation entry points (see Patches/Targeted/).
                // Do not use a dummy parameter named "_" — Harmony treats it as an original-parameter name and throws.
                __state = false;
            }

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(object ui)
            {
                try
                {
                    if (!CustomInspectorFonts.PatchSiteEnabled(CustomInspectorFonts.StyleRadiantUiSetupEditorStyle))
                        return;
                    // Engine sets Style.Font from GetBolderFont first; postfix must not always force primary (clobbers bold slot).
                    // When bolder stack is active, pass true so Style.Font is set from bold-tag FontChain explicitly.
                    TryApplyFontToUiBuilder(ui, preferBoldChainWhenBolderStackActive: true);
                }
                catch (Exception ex)
                {
                    Warn($"SetupEditorStyle postfix: {ex.GetType().Name}: {ex.Message}");
                }
            }

            [HarmonyFinalizer]
            private static void Finalizer(ref bool __state)
            {
                TryPopInspectorBolderFontOverride(ref __state);
            }
        }
    }
}
