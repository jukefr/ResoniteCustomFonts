using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace CustomFonts;

public partial class CustomFonts
{
    public static partial class CustomFontsPatches
    {
        private static Type? _cachedTextType;

        private static Type? TextType =>
            _cachedTextType ??= AccessTools.TypeByName("FrooxEngine.UIX.Text");

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
        /// Apply the resolved font to the UIBuilder's style AND to any existing Text components
        /// on the builder's slot (which were created before SetupEditorStyle ran, e.g. the title
        /// text from RadiantUI_Panel.SetupPanel).
        /// </summary>
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

            // Update pre-existing Text components on the builder's slot hierarchy.
            // RadiantUI_Panel.SetupPanel creates the title text BEFORE SetupEditorStyle
            // runs, so setting Style.Font alone doesn't affect it — the Text component's
            // own Font field still points to the default Radiant font.
            TryApplyFontToExistingTexts(slot, sourceFont);
        }

        /// <summary>Find Text components under the builder slot and set their Font to match our source.</summary>
        private static void TryApplyFontToExistingTexts(object? rootSlot, object? font)
        {
            if (rootSlot == null || font == null)
                return;
            var textType = TextType;
            if (textType == null)
                return;
            // Use GetComponentsInChildren<Text>() to find ALL Text components at any depth
            foreach (var textComp in SlotGraphWalker.EnumerateComponentsInChildrenOfSlot(rootSlot, textType))
            {
                if (textComp == null)
                    continue;
                // Text.Font is AssetRef<FontSet> — set its .Target to our font
                if (!TrySetMemberValue(textComp, "Font", font))
                    TryAssignFontChainToMemberAssetRef(textComp, "Font", font);
            }
        }
    }
}
