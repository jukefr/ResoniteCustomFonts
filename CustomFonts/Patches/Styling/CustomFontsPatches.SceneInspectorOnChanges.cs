using System.Reflection;
using Elements.Core;
using HarmonyLib;

namespace CustomFonts;

/// <summary>
/// Most hierarchy/component UI is built or rebuilt from <see cref="FrooxEngine.SceneInspector.OnChanges"/>; the bolder stack
/// must wrap that path, not only <see cref="FrooxEngine.SceneInspector.OnAttach"/> (stack is already popped after OnAttach returns).
/// </summary>
public partial class CustomFonts
{
    public static partial class CustomFontsPatches
    {
        [HarmonyPatch]
        private static class SceneInspectorOnChangesBolderScopePatch
        {
            private static bool Prepare()
            {
                var t = AccessTools.TypeByName("FrooxEngine.SceneInspector");
                return t != null && AccessTools.Method(t, "OnChanges") != null;
            }

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod()
            {
                var t = AccessTools.TypeByName("FrooxEngine.SceneInspector");
                return t == null ? null : AccessTools.Method(t, "OnChanges");
            }

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state)
            {
                __state = false;
                try
                {
                    if (__instance == null || !CustomFonts.PatchSiteEnabled(CustomFonts.StyleSceneInspectorOnChanges))
                        return;
                    if (!WorkerBelongsToThisClient(__instance))
                        return;
                    var slot = ReadMemberValue(__instance, "Slot");
                    if (slot == null)
                        return;
                    __state = TryPushInspectorBolderFontOverrideFromSlot(slot);
                }
                catch (Exception ex)
                {
                    UniLog.Log("[CustomFonts] SceneInspector.OnChanges Prefix: " + ex, false);
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
                    UniLog.Log("[CustomFonts] SceneInspector.OnChanges Finalizer: " + ex, false);
                }
            }
        }
    }
}
