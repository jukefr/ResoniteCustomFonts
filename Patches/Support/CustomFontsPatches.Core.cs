using System;
using System.Reflection;
using HarmonyLib;
namespace CustomFonts;

public partial class CustomFonts
{
	public static partial class CustomFontsPatches
	{
		private static Type? _cachedUiBuilderType;

		internal static Type? UiBuilderType =>
			_cachedUiBuilderType ??= AccessTools.TypeByName("FrooxEngine.UIX.UIBuilder");

		/// <summary>
		/// <see cref="FrooxEngine.UIX.UIBuilder"/> has <c>Root</c> and <c>Canvas</c>, not a <c>Slot</c> property — resolving context from <c>Root</c> is required for style/font patches.
		/// </summary>
		internal static object? GetContextSlotFromUiBuilder(object? ui)
		{
			var uiBuilderType = UiBuilderType;
			if (ui == null || uiBuilderType == null || !uiBuilderType.IsInstanceOfType(ui))
				return null;
			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var rootProp = uiBuilderType.GetProperty("Root", flags);
			if (rootProp != null)
			{
				var root = SafeRead(() => rootProp.GetValue(ui));
				if (root != null)
					return root;
			}

			var slotProp = uiBuilderType.GetProperty("Slot", flags);
			if (slotProp != null)
			{
				var slot = SafeRead(() => slotProp.GetValue(ui));
				if (slot != null)
					return slot;
			}

			var canvasProp = uiBuilderType.GetProperty("Canvas", flags);
			var canvas = canvasProp == null ? null : SafeRead(() => canvasProp.GetValue(ui));
			return canvas == null ? null : ReadMemberValue(canvas, "Slot");
		}

		/// <summary>
		/// <see cref="FrooxEngine.RadiantUI_Constants.SetupEditorStyle"/> assigns <c>ui.Style.Font = world.GetBolderFont()</c>.
		/// While that runs, we push the inspector <c>FontChain</c> so <c>GetBolderFont</c> returns it at the real assignment site.
		/// </summary>
		private static readonly Stack<(object World, object Font)> _inspectorBolderFontStack = new();

		/// <summary>GetBolderFont / inspector UI can run across worker threads; the stack must not be accessed concurrently.</summary>
		private static readonly object BolderFontStackLock = new();

		private static bool TryPushInspectorBolderFontOverrideFromSlot(object? slot)
		{
			if (slot == null)
				return false;
			var world = ReadMemberValue(slot, "World");
			return TryPushInspectorBolderFontWorldAndFont(world, slot);
		}

		private static bool TryPushInspectorBolderFontWorldAndFont(object? world, object? slot)
		{
			if (!CustomFonts.ActiveEnabled())
				return false;
			if (world == null || slot == null)
			{
				if (CustomFonts.FontLoggingEnabled())
					CustomFonts.FontLog($"bolder push skip: worldNull={world == null} slotNull={slot == null}");
				return false;
			}

			if (!ShouldApplyCustomFontsForUiSlot(slot))
				return false;

			var font = ResolveBoldFontProviderForUiSlot(slot);
			if (font == null)
			{
				if (CustomFonts.FontLoggingEnabled())
					CustomFonts.FontLog($"bolder push skip: ResolveBoldFontProviderForUiSlot null (slot={slot.GetType().Name})");
				return false;
			}

			lock (BolderFontStackLock)
				_inspectorBolderFontStack.Push((world, font));
			return true;
		}

		private static bool TryPushInspectorBolderFontOverrideFromUiBuilder(object? ui)
		{
			if (ui == null)
				return false;
			var uiBuilderType = UiBuilderType;
			if (uiBuilderType == null || !uiBuilderType.IsInstanceOfType(ui))
				return false;
			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var canvasProp = uiBuilderType.GetProperty("Canvas", flags);
			var slot = GetContextSlotFromUiBuilder(ui);
			if (slot == null)
				return false;
			var canvas = canvasProp?.GetValue(ui);
			var world = canvas == null ? null : ReadMemberValue(canvas, "World");
			world ??= ReadMemberValue(slot, "World");
			return TryPushInspectorBolderFontWorldAndFont(world, slot);
		}

		private static void TryPopInspectorBolderFontOverride(ref bool __state)
		{
			if (!__state)
				return;
			lock (BolderFontStackLock)
			{
				if (_inspectorBolderFontStack.Count > 0)
					_inspectorBolderFontStack.Pop();
			}

			__state = false;
		}

		/// <summary>
		/// True when <see cref="FrooxEngine.RadiantUI_Constants.SetupEditorStyle"/> postfix should assign <c>Style.Font</c> from the **bold** tag resolver (stack matches this UI world).
		/// </summary>
		internal static bool ShouldUseBoldFontChainInSetupEditorStylePostfix(object? ui)
		{
			if (ui == null)
				return false;
			var slot = GetContextSlotFromUiBuilder(ui);
			var world = slot == null ? null : ReadMemberValue(slot, "World");
			if (world == null)
				return false;
			lock (BolderFontStackLock)
			{
				if (_inspectorBolderFontStack.Count == 0)
					return false;
				var (w, _) = _inspectorBolderFontStack.Peek();
				return ReferenceEquals(w, world);
			}
		}

		private static object? TryGetPropertyValueAcrossInheritance(object? target, string propertyName)
		{
			if (target == null)
				return null;
			for (var t = target.GetType(); t != null; t = t.BaseType)
			{
				var p = t.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (p != null && p.GetIndexParameters().Length == 0)
					return SafeRead(() => p.GetValue(target));
			}

			return null;
		}

		private static bool TrySlotIsUnderLocalUser(object? slot, out bool isUnder)
		{
			isUnder = false;
			if (slot == null)
				return false;
			var world = ReadMemberValue(slot, "World");
			var worldLu = world == null ? null : TryGetPropertyValueAcrossInheritance(world, "LocalUser");
			foreach (var m in slot.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (m.Name != "IsUnderLocalUser" || m.ReturnType != typeof(bool))
					continue;
				var ps = m.GetParameters();
				try
				{
					if (ps.Length == 0)
					{
						if (SafeRead(() => m.Invoke(slot, null)) is bool b)
						{
							isUnder = b;
							return true;
						}
					}
					else if (ps.Length == 1 && worldLu != null && ps[0].ParameterType.IsInstanceOfType(worldLu))
					{
						if (SafeRead(() => m.Invoke(slot, new[] { worldLu })) is bool b2)
						{
							isUnder = b2;
							return true;
						}
					}
				}
				catch
				{
					// try next overload
				}
			}

			return false;
		}

		private static bool SlotOrAncestorsUnderLocalUser(object? slot)
		{
			for (var cur = slot; cur != null; cur = ReadMemberValue(cur, "Parent"))
			{
				if (TrySlotIsUnderLocalUser(cur, out var ok) && ok)
					return true;
			}

			return false;
		}

		/// <summary>
		/// FrooxEngine inspector workers expose <c>LocalUser</c> (who owns the dev tool). Only that user's inspector UI is modified on this client.
		/// </summary>
		internal static bool WorkerBelongsToThisClient(object? workerLike)
		{
			if (workerLike == null)
				return false;
			var worldLu = TryGetPropertyValueAcrossInheritance(ReadMemberValue(workerLike, "World"), "LocalUser");
			if (worldLu == null)
				return true;
			var compLu = TryGetPropertyValueAcrossInheritance(workerLike, "LocalUser");
			if (compLu != null)
				return ReferenceEquals(compLu, worldLu);
			return SlotOrAncestorsUnderLocalUser(ReadMemberValue(workerLike, "Slot"));
		}
	}
}
