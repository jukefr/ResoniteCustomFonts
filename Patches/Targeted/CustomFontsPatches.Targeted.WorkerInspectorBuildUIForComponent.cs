using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

public partial class CustomFonts
{
    public static partial class CustomFontsPatches
    {
        [HarmonyPatch]
        private static class TargetedWorkerInspectorBuildUIForComponentPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.WorkerInspector"), "BuildUIForComponent") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.WorkerInspector"), "BuildUIForComponent");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleWorkerInspectorBuildUIForComponent);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }
    }
}
