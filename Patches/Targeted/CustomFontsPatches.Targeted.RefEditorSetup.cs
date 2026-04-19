using System.Collections.Generic;
using System.Linq;
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
				return t != null
					&& t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Any(m => m.Name == "Setup");
			}

			[HarmonyTargetMethods]
			private static IEnumerable<MethodInfo> TargetMethods()
			{
				var t = AccessTools.TypeByName("FrooxEngine.RefEditor");
				if (t == null)
					yield break;
				foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
				{
					if (m.Name == "Setup" && m.ReturnType == typeof(void))
						yield return m;
				}
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
