using System.Reflection;
using Elements.Core;
using HarmonyLib;

namespace CustomFonts;

/// <summary>
/// <see cref="FrooxEngine.SlotInspector.OnChanges"/> calls <see cref="FrooxEngine.RadiantUI_Constants.SetupEditorStyle"/> and UI builders;
/// the SetupEditorStyle finalizer pops the stack before sibling calls unless we scope the whole <c>OnChanges</c>.
/// </summary>
public partial class CustomFonts
{
    public static partial class CustomFontsPatches
    {
        [HarmonyPatch]
        private static class SlotInspectorOnChangesBolderScopePatch
        {
            private static bool Prepare()
            {
                var t = AccessTools.TypeByName("FrooxEngine.SlotInspector");
                return t != null && AccessTools.Method(t, "OnChanges") != null;
            }

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod()
            {
                var t = AccessTools.TypeByName("FrooxEngine.SlotInspector");
                return t == null ? null : AccessTools.Method(t, "OnChanges");
            }

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state)
            {
                __state = false;
                try
                {
                    if (__instance == null || !CustomFonts.PatchSiteEnabled(CustomFonts.StyleSlotInspectorOnChanges))
                        return;
                    if (!WorkerBelongsToThisClient(__instance))
                        return;
                    __state = TryPushInspectorBolderFontOverrideFromSlot(ReadMemberValue(__instance, "Slot"));
                }
                catch (Exception ex)
                {
                    UniLog.Log("[CustomFonts] SlotInspector.OnChanges Prefix: " + ex, false);
                }
            }

            [HarmonyFinalizer]
            private static void Finalizer(ref bool __state)
            {
                try
                {
                    TryPopInspectorBolderFontOverride(ref __state);
                }
                catch (Exception ex)
                {
                    UniLog.Log("[CustomFonts] SlotInspector.OnChanges Finalizer: " + ex, false);
                }
            }
        }
    }
}
