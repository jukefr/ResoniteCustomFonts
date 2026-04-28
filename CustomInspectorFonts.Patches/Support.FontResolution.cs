using System.Reflection;

namespace CustomInspectorFonts;

public partial class CustomInspectorFonts
{
    public static partial class CustomInspectorFontsPatches
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

        /// <summary>
        /// Apply the resolved font to the UIBuilder's style. Called from UIBuilder constructor postfix,
        /// SetupDefaultStyle postfix, and SetupEditorStyle postfix. Non-inspector UIs are filtered
        /// by <see cref="ShouldApplyCustomFontsForUiSlot"/>.
        /// </summary>
        private static void TryApplyFontToUiBuilder(object? ui, bool preferBoldChainWhenBolderStackActive = false)
        {
            if (!CustomInspectorFonts.ActiveEnabled())
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
