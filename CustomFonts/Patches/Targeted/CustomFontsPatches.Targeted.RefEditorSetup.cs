using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

public partial class CustomFonts
{
	public static partial class CustomFontsPatches
	{
		[HarmonyPatch]
		private static class TargetedRefEditorSetupPatch
		{
			private static bool Prepare()
			{
				var t = AccessTools.TypeByName("FrooxEngine.RefEditor");
				return t != null && FindDeclaredInstanceMethod(t, "Setup") != null;
			}

			[HarmonyTargetMethod]
			private static MethodInfo? TargetMethod()
			{
				var t = AccessTools.TypeByName("FrooxEngine.RefEditor");
				return t == null ? null : FindDeclaredInstanceMethod(t, "Setup");
			}

			[HarmonyPrefix]
			[HarmonyPriority(-10000)]
			private static void Prefix(object __instance, ref bool __state) =>
				TargetedBolderScopePrefixComponent(__instance, ref __state, CustomFonts.StyleRefEditorSetup);

			[HarmonyPostfix]
			[HarmonyPriority(int.MaxValue)]
			private static void Postfix(ref bool __state) => TargetedBolderScopePostfix(ref __state);
		}
	}
}
