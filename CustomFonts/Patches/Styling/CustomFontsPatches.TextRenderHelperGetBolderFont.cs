using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;

namespace CustomFonts;

public partial class CustomFonts
{
	public static partial class CustomFontsPatches
	{
		/// <summary>
		/// Same substitution as <see cref="FrooxEngine.RadiantUI_Constants.SetupEditorStyle"/> for code paths that call
		/// <c>GetBolderFont</c> while building UI under an inspector bolder scope.
		/// </summary>
		[HarmonyPatch]
		private static class TextRenderHelperGetBolderFontPatch
		{
			private static bool Prepare()
			{
				var t = AccessTools.TypeByName("FrooxEngine.TextRenderHelper");
				var w = AccessTools.TypeByName("FrooxEngine.World");
				return t != null && w != null && AccessTools.Method(t, "GetBolderFont", new[] { w }) != null;
			}

			[HarmonyTargetMethod]
			private static MethodInfo? TargetMethod()
			{
				var t = AccessTools.TypeByName("FrooxEngine.TextRenderHelper");
				var w = AccessTools.TypeByName("FrooxEngine.World");
				return t == null || w == null ? null : AccessTools.Method(t, "GetBolderFont", new[] { w });
			}

			[HarmonyPostfix]
			[HarmonyPriority(int.MaxValue)]
			private static void Postfix([HarmonyArgument(0)] object world, ref object __result)
			{
				if (!CustomFonts.PatchSiteEnabled(CustomFonts.StyleTextRenderHelperGetBolderFont))
					return;
				int depth;
				object? fontFromStack = null;
				bool worldMatches;
				lock (BolderFontStackLock)
				{
					depth = _inspectorBolderFontStack.Count;
					if (depth == 0)
					{
						if (CustomFonts.FontLoggingEnabled())
						{
							CustomFonts.FontLog(
								$"GetBolderFont: stack empty (no bolder scope) resultType={__result?.GetType().Name ?? "null"}");
						}

						return;
					}

					var (w, font) = _inspectorBolderFontStack.Peek();
					worldMatches = ReferenceEquals(w, world);
					if (worldMatches)
						fontFromStack = font;
					else if (CustomFonts.FontLoggingEnabled())
					{
						CustomFonts.FontLog(
							$"GetBolderFont: stack has override but World mismatch (stackDepth={depth}) resultType={__result?.GetType().Name ?? "null"}");
					}
				}

				if (fontFromStack != null)
				{
					__result = fontFromStack;
					CustomFonts.FontLog($"GetBolderFont substituted (world match, stack depth {depth}).");
				}
			}
		}
	}
}
