using System.Linq;
using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;

namespace CustomFonts;

/// <summary>
/// Patches in this folder push the GetBolderFont stack at inspector UI <em>creation</em> entry points; <see cref="CustomFontsPatches.SetupEditorStylePatch"/> Prefix does not push (style still postfixed).
/// See <c>PATCH_TARGETS.txt</c>.
/// </summary>
public partial class CustomFonts
{
	public static partial class CustomFontsPatches
	{
		private static MethodInfo? FindDeclaredInstanceMethod(Type? type, string name)
		{
			if (type == null)
				return null;
			return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
				.FirstOrDefault(m => m.Name == name);
		}

		private static MethodInfo? FindDeclaredStaticMethod(Type? type, string name)
		{
			if (type == null)
				return null;
			return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
				.FirstOrDefault(m => m.Name == name);
		}

		private static void TargetedBolderScopePrefixComponent(object? __instance, ref bool __state, ModConfigurationKey<bool> siteKey)
		{
			__state = false;
			if (!CustomFonts.PatchSiteEnabled(siteKey))
				return;
			if (__instance == null)
				return;
			if (!WorkerBelongsToThisClient(__instance))
				return;
			__state = TryPushInspectorBolderFontOverrideFromSlot(ReadMemberValue(__instance, "Slot"));
		}

		private static void TargetedBolderScopePostfix(ref bool __state)
		{
			TryPopInspectorBolderFontOverride(ref __state);
		}

		/// <summary>Static or non-component entry points whose first contextual argument is the UI root <c>Slot</c>.</summary>
		private static void TargetedBolderScopePrefixFromRootSlot(object? rootSlot, ref bool __state, ModConfigurationKey<bool> siteKey)
		{
			__state = false;
			if (!CustomFonts.PatchSiteEnabled(siteKey))
				return;
			if (rootSlot == null || !ShouldApplyCustomFontsForUiSlot(rootSlot))
				return;
			__state = TryPushInspectorBolderFontOverrideFromSlot(rootSlot);
		}
	}
}
