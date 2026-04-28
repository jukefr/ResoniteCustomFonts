using System.Reflection;
using HarmonyLib;

namespace CustomInspectorFonts;

public partial class CustomInspectorFonts
{
    public static partial class CustomInspectorFontsPatches
    {
        [HarmonyPatch]
        private static class TargetedListEditorBuildListItemPatch
        {
            private static bool Prepare() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ListEditor"), "BuildListItem") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.ListEditor"), "BuildListItem");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object __instance, ref bool __state) =>
                TargetedBolderScopePrefixComponent(__instance, ref __state, CustomInspectorFonts.StyleListEditorBuildListItem);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }
    }
}
