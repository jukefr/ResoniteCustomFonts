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

            /// <summary>
            /// MyInspectors and other mods may call <c>OnChanges</c> via reflection from coroutines
            /// where the SceneInspector is not fully initialized, throwing NRE inside the combined
            /// Harmony wrapper. We suppress the exception (return true) so it doesn't cascade back
            /// into MyInspectors' coroutine and spam the log — no functional impact from the NRE.
            /// </summary>
            [HarmonyFinalizer]
            private static bool Finalizer(Exception __exception, ref bool __state)
            {
                if (__exception != null)
                {
                    UniLog.Log("[CustomFonts] SceneInspector.OnChanges suppressed: " + __exception.GetType().Name + ": " + __exception.Message, false);
                }

                try
                {
                    TryPopInspectorBolderFontOverride(ref __state);
                }
                catch (Exception ex)
                {
                    UniLog.Log("[CustomFonts] SceneInspector.OnChanges Finalizer: " + ex, false);
                }

                return true; // suppress exception — original OnChanges failure is non-fatal
            }
        }
    }
}
