using System.Reflection;
using HarmonyLib;

namespace CustomInspectorFonts;

public partial class CustomInspectorFonts
{
    public static partial class CustomInspectorFontsPatches
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
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomInspectorFonts.StyleUserInspectorOnAttach);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }
    }
}
