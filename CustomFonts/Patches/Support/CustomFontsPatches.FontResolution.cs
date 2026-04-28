using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

public partial class CustomFonts
{
    public static partial class CustomFontsPatches
    {
        private static object? FindInspectorPanelFromSlot(object? startSlot) =>
            FontResolver.FindInspectorPanelFromSlot(startSlot);

        private static bool ShouldApplyCustomFontsForUiSlot(object? slot) =>
            FontResolver.ShouldApplyCustomFontsForUiSlot(slot);

        private static bool SlotIsUnderWorldLocalUserSpace(object? slot) =>
            FontResolver.SlotIsUnderWorldLocalUserSpace(slot);

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

        private static object? ResolveLocalUserAvatarRootSlot(object? world) =>
            FontResolver.ResolveLocalUserAvatarRootSlot(world);

        private static object? ResolveFontProviderForUiSlotWithTag(object slot, string tag) =>
            FontResolver.ResolveFontProviderForUiSlotWithTag(slot, tag, FontSourceSlotTag, BoldFontSourceSlotTag);

        private static object? ResolveFontProviderForUiSlot(object slot) =>
            FontResolver.ResolveFontProviderForUiSlot(slot, FontSourceSlotTag, BoldFontSourceSlotTag);

        private static object? ResolveBoldFontProviderForUiSlot(object slot) =>
            FontResolver.ResolveBoldFontProviderForUiSlot(slot, FontSourceSlotTag, BoldFontSourceSlotTag);

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
