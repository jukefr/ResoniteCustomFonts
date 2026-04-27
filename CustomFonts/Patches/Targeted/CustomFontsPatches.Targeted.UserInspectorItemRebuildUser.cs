using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

public partial class CustomFonts
{
	public static partial class CustomFontsPatches
	{
		[HarmonyPatch]
		private static class TargetedUserInspectorItemRebuildUserPatch
		{
			private static bool Prepare() =>
				FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.UserInspectorItem"), "RebuildUser") != null;

			[HarmonyTargetMethod]
			private static MethodInfo? TargetMethod() =>
				FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.UserInspectorItem"), "RebuildUser");

			[HarmonyPrefix]
			[HarmonyPriority(-10000)]
			private static void Prefix(object __instance, ref bool __state) =>
				TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleUserInspectorItemRebuildUser);

			[HarmonyPostfix]
			[HarmonyPriority(int.MaxValue)]
			private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
		}
	}
}
