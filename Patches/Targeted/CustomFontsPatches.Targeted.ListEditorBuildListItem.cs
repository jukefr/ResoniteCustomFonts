using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

public partial class CustomFonts
{
	public static partial class CustomFontsPatches
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
				TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleListEditorBuildListItem);

			[HarmonyPostfix]
			[HarmonyPriority(int.MaxValue)]
			private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
		}
	}
}
