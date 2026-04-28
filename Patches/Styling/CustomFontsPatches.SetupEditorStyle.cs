using System;
using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;

namespace CustomFonts;

public partial class CustomFonts
{
    public static partial class CustomFontsPatches
    {
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
                    if (!CustomFonts.PatchSiteEnabled(CustomFonts.StyleRadiantUiSetupEditorStyle))
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
