using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

public partial class CustomFonts
{
    public static partial class CustomFontsPatches
    {
        [HarmonyPatch]
        private static class TargetedUserInspectorOnAttachPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.UserInspector"), "OnAttach") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.UserInspector"), "OnAttach");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleUserInspectorOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }
    }
}
