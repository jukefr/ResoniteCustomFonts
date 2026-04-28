using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

public partial class CustomFonts
{
    public static partial class CustomFontsPatches
    {
        [HarmonyPatch]
        private static class TargetedWorkerInspectorCreatePatch
        {
            private static bool Prepare() =>
                FindDeclaredStaticMethod(AccessTools.TypeByName("FrooxEngine.WorkerInspector"), "Create") != null;

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod() =>
                FindDeclaredStaticMethod(AccessTools.TypeByName("FrooxEngine.WorkerInspector"), "Create");

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix(object root, ref bool __state) =>
                TargetedBolderScopePrefixFromRootSlot(root, ref __state, CustomFonts.StyleWorkerInspectorCreate);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }
    }
}
