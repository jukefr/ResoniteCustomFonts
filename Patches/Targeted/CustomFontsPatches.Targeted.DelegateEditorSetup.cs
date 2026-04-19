using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

public partial class CustomFonts
{
	public static partial class CustomFontsPatches
	{
		[HarmonyPatch]
		private static class TargetedDelegateEditorSetupPatch
		{
			private static bool Prepare() =>
				FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.DelegateEditor"), "Setup") != null;

			[HarmonyTargetMethod]
			private static MethodInfo? TargetMethod() =>
				FindDeclaredInstanceMethod(AccessTools.TypeByName("FrooxEngine.DelegateEditor"), "Setup");

			[HarmonyPrefix]
			[HarmonyPriority(-10000)]
			private static void Prefix(object __instance, ref bool __state) =>
				TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleDelegateEditorSetup);

			[HarmonyPostfix]
			[HarmonyPriority(int.MaxValue)]
			private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
		}
	}
}
