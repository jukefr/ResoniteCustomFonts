using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

public partial class CustomFonts
{
    public static partial class CustomFontsPatches
    {
        [HarmonyPatch]
        private static class TargetedSyncMemberEditorBuilderBuildPatch
        {
            private static bool Prepare()
            {
                var t = AccessTools.TypeByName("FrooxEngine.SyncMemberEditorBuilder");
                return t != null
                    && FindDeclaredStaticMethod(t, "Build") != null;
            }

            [HarmonyTargetMethod]
            private static MethodInfo? TargetMethod()
            {
                var t = AccessTools.TypeByName("FrooxEngine.SyncMemberEditorBuilder");
                if (t == null)
                    return null;
                var fi = typeof(FieldInfo);
                var ui = UiBuilderType;
                if (ui == null)
                    return null;
                return t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(m => m.Name == "Build" && m.GetParameters().Length >= 4
                        && m.GetParameters()[0].ParameterType.Name.Contains("ISyncMember")
                        && m.GetParameters()[2].ParameterType == fi
                        && m.GetParameters()[3].ParameterType == ui);
            }

            [HarmonyPrefix]
            [HarmonyPriority(-10000)]
            private static void Prefix([HarmonyArgument(0)] object member, [HarmonyArgument(3)] object ui, ref bool __state)
            {
                __state = false;
                try
                {
                    if (!CustomFonts.PatchSiteEnabled(CustomFonts.StyleSyncMemberEditorBuilder))
                        return;
                    var world = ReadMemberValue(member, "World");
                    var slot = GetContextSlotFromUiBuilder(ui);
                    if (slot == null || !ShouldApplyCustomFontsForUiSlot(slot))
                        return;
                    __state = TryPushInspectorBolderFontWorldAndFont(world, slot);
                }
                catch (Exception ex)
                {
                    Warn($"SyncMemberEditorBuilder.Build prefix: {ex.GetType().Name}: {ex.Message}");
                }
            }

            [HarmonyPostfix]
            [HarmonyPriority(int.MaxValue)]
            private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
        }
    }
}
