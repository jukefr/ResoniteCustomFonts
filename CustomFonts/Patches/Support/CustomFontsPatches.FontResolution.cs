using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

public partial class CustomFonts
{
	public static partial class CustomFontsPatches
	{
		/// <summary>
		/// Finds hierarchy/component inspector panels (including variants that are not <see cref="FrooxEngine.SceneInspector"/> CLR type).
		/// </summary>
		private static object? FindInspectorPanelFromSlot(object? startSlot)
		{
			if (startSlot == null)
				return null;
			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			foreach (var s in EnumerateParentSlots(startSlot))
			{
				foreach (var comp in EnumerateComponentsOnSlot(s))
				{
					if (comp == null)
						continue;
					var ct = comp.GetType();
					if (ct.GetField("_hierarchyContentRoot", flags) == null)
						continue;
					if (ct.GetField("_componentsContentRoot", flags) == null)
						continue;
					return comp;
				}
			}

			return null;
		}

		private static bool ShouldApplyCustomFontsForUiSlot(object? slot)
		{
			if (slot == null)
				return false;
			var panel = FindInspectorPanelFromSlot(slot);
			if (panel != null)
				return WorkerBelongsToThisClient(panel);

			// WorkerInspector (e.g. detached component window) has no _hierarchy/_components roots — still an IWorker inspector.
			var workerInspectorType = AccessTools.TypeByName("FrooxEngine.WorkerInspector");
			if (workerInspectorType != null)
			{
				var wi = GetComponentInParents(slot, workerInspectorType);
				if (wi != null)
					return WorkerBelongsToThisClient(wi);
			}

			// Attach Component browser lives under ComponentSelector, not SceneInspector.
			var componentSelectorType = AccessTools.TypeByName("FrooxEngine.ComponentSelector");
			if (componentSelectorType != null)
			{
				var cs = GetComponentInParents(slot, componentSelectorType);
				if (cs != null)
					return WorkerBelongsToThisClient(cs);
			}

			// Other dev-tool panels (RadiantUI under LocalUserSpace) may not report IsUnderLocalUser on every ancestor.
			if (SlotIsUnderWorldLocalUserSpace(slot))
				return true;

			return SlotOrAncestorsUnderLocalUser(slot);
		}

		/// <summary>True when <paramref name="slot"/> is <see cref="FrooxEngine.World.LocalUserSpace"/> or nested under it.</summary>
		private static bool SlotIsUnderWorldLocalUserSpace(object? slot)
		{
			if (slot == null)
				return false;
			var world = ReadMemberValue(slot, "World");
			if (world == null)
				return false;
			var lus = TryGetPropertyValueAcrossInheritance(world, "LocalUserSpace");
			if (lus == null)
				return false;
			for (var cur = slot; cur != null; cur = ReadMemberValue(cur, "Parent"))
			{
				if (ReferenceEquals(cur, lus))
					return true;
			}

			return false;
		}

		private static string? ReadSlotTagString(object? slot)
		{
			if (slot == null)
				return null;
			var raw = ReadMemberValue(slot, "Tag");
			if (raw is string s)
				return s;
			try
			{
				var vp = raw?.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				return vp?.GetValue(raw) as string;
			}
			catch
			{
				return null;
			}
		}

		/// <summary><see cref="FrooxEngine.User.Root"/> → <c>Slot</c> (avatar hierarchy root for the local user).</summary>
		private static object? ResolveLocalUserAvatarRootSlot(object? world)
		{
			if (world == null)
				return null;
			var localUser = TryGetPropertyValueAcrossInheritance(world, "LocalUser");
			if (localUser == null)
				return null;
			var root = ReadMemberValue(localUser, "Root");
			if (root == null)
				return null;
			return ReadMemberValue(root, "Slot");
		}

		/// <summary>
		/// Font source: first <see cref="FrooxEngine.FontChain"/> on a descendant of <see cref="FrooxEngine.User.Root"/>.<c>Slot</c>
		/// whose slot tag equals <paramref name="tag"/>.
		/// </summary>
		private static object? ResolveFontProviderForUiSlotWithTag(object slot, string tag)
		{
			var world = ReadMemberValue(slot, "World");
			var avatarRoot = ResolveLocalUserAvatarRootSlot(world);
			if (avatarRoot == null)
			{
				if (CustomFonts.FontLoggingEnabled())
					CustomFonts.FontLog("ResolveFont: World.LocalUser or Root.Slot missing — cannot search avatar for tagged FontChain.");
				return null;
			}

			foreach (var s in EnumerateDescendantSlots(avatarRoot))
			{
				if (s == null)
					continue;
				var slotTag = ReadSlotTagString(s);
				if (!string.Equals(slotTag, tag, StringComparison.Ordinal))
					continue;
				var chain = EnumerateFontChainComponentsOnSlot(s).FirstOrDefault();
				if (chain != null)
				{
					if (CustomFonts.FontLoggingEnabled())
						CustomFonts.FontLog($"ResolveFont: using FontChain on avatar slot with tag \"{tag}\".");
					return chain;
				}

				if (CustomFonts.FontLoggingEnabled())
					CustomFonts.FontLog($"ResolveFont: found tag \"{tag}\" but no FontChain on that slot — continuing search.");
				continue;
			}

			if (CustomFonts.FontLoggingEnabled())
				CustomFonts.FontLog($"ResolveFont: no slot with tag \"{tag}\" under LocalUser.Root.Slot.");
			return null;
		}

		/// <summary>Primary (normal weight) FontChain from <see cref="CustomFonts.FontSourceSlotTag"/>.</summary>
		private static object? ResolveFontProviderForUiSlot(object slot) =>
			ResolveFontProviderForUiSlotWithTag(slot, CustomFonts.FontSourceSlotTag());

		/// <summary>Bolder FontChain from <see cref="CustomFonts.BoldFontSourceSlotTag"/>; falls back to primary when the bold tag yields nothing.</summary>
		private static object? ResolveBoldFontProviderForUiSlot(object slot)
		{
			var boldTag = CustomFonts.BoldFontSourceSlotTag();
			var primaryTag = CustomFonts.FontSourceSlotTag();
			var bold = ResolveFontProviderForUiSlotWithTag(slot, boldTag);
			if (bold != null)
				return bold;
			if (!string.Equals(boldTag, primaryTag, StringComparison.Ordinal) && CustomFonts.FontLoggingEnabled())
				CustomFonts.FontLog($"ResolveBoldFont: no FontChain for bold tag \"{boldTag}\" — falling back to primary tag \"{primaryTag}\".");
			return ResolveFontProviderForUiSlot(slot);
		}

		/// <param name="preferBoldChainWhenBolderStackActive">
		/// For <see cref="FrooxEngine.RadiantUI_Constants.SetupEditorStyle"/>: when a targeted bolder scope is active for this UI world,
		/// assign <c>Style.Font</c> from the bold-tag FontChain so we do not clobber it with the primary chain (and we still apply when GetBolderFont substitution is off).
		/// </param>
		private static void TryApplyFontToUiBuilder(object? ui, bool preferBoldChainWhenBolderStackActive = false)
		{
			if (!CustomFonts.ActiveEnabled())
				return;
			if (ui == null)
				return;
			var uiBuilderType = UiBuilderType;
			if (uiBuilderType == null || !uiBuilderType.IsInstanceOfType(ui))
				return;
			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var slot = GetContextSlotFromUiBuilder(ui);
			if (slot == null)
				return;
			if (!ShouldApplyCustomFontsForUiSlot(slot))
				return;
			var sourceFont =
				preferBoldChainWhenBolderStackActive && ShouldUseBoldFontChainInSetupEditorStylePostfix(ui)
					? ResolveBoldFontProviderForUiSlot(slot)
					: ResolveFontProviderForUiSlot(slot);
			if (sourceFont == null)
				return;
			var styleProp = uiBuilderType.GetProperty("Style", flags);
			var style = styleProp?.GetValue(ui);
			if (style == null)
				return;
			if (!TrySetMemberValue(style, "Font", sourceFont))
				TryAssignFontChainToMemberAssetRef(style, "Font", sourceFont);
		}
	}
}
