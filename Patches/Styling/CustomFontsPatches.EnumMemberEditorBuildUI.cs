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
        /// EnumDropdown patches <see cref="FrooxEngine.EnumMemberEditor"/>.<c>BuildUI</c> and nests extra controls; font must match
        /// inspector root after that (same pattern as <c>RadiantUI_Constants.SetupEditorStyle</c> on spawned panels).
        /// </summary>
        [HarmonyPatch]
        private static class EnumMemberEditorBuildUIPatch
        {
            private static bool Prepare()
            {
                var t = AccessTools.TypeByName("FrooxEngine.EnumMemberEditor");
                return t != null && AccessTools.Method(t, "BuildUI") != null;
            }

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod()
            {
                var t = AccessTools.TypeByName("FrooxEngine.EnumMemberEditor");
                return t == null ? null : AccessTools.Method(t, "BuildUI");
            }

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix([HarmonyArgument("ui")] object? ui, ref bool __state)
            {
                __state = false;
            }

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix([HarmonyArgument("ui")] object? ui)
            {
                try
                {
                    if (!CustomFonts.PatchSiteEnabled(CustomFonts.StyleEnumMemberEditorBuildUI))
                        return;
                    TryApplyFontToUiBuilder(ui);
                }
                catch (Exception ex)
                {
                    Warn($"EnumMemberEditor.BuildUI postfix: {ex.GetType().Name}: {ex.Message}");
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
