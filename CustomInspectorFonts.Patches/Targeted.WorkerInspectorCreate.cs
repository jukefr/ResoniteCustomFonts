using System.Reflection;
using HarmonyLib;

namespace CustomInspectorFonts;

public partial class CustomInspectorFonts
{
    public static partial class CustomInspectorFontsPatches
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
                TargetedBolderScopePrefixFromRootSlot(root, ref __state, CustomInspectorFonts.StyleWorkerInspectorCreate);

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }
    }
}
