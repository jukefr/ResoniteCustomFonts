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
            {
                CustomFonts.FontLog("TryApplyFont: slot is null");
                return;
            }
            if (!ShouldApplyCustomFontsForUiSlot(slot))
            {
                CustomFonts.FontLog($"TryApplyFont: slot {slot.GetType().Name} not eligible");
                return;
            }
            var sourceFont =
                preferBoldChainWhenBolderStackActive && ShouldUseBoldFontChainInSetupEditorStylePostfix(ui)
                    ? ResolveBoldFontProviderForUiSlot(slot)
                    : ResolveFontProviderForUiSlot(slot);
            if (sourceFont == null)
            {
                CustomFonts.FontLog("TryApplyFont: sourceFont is null");
                return;
            }

            CustomFonts.FontLog($"TryApplyFont: resolved sourceFont={sourceFont.GetType().Name}#{sourceFont.GetHashCode()}");

            var styleProp = uiBuilderType.GetProperty("Style", flags);
            var style = styleProp?.GetValue(ui);
            if (style == null)
            {
                CustomFonts.FontLog("TryApplyFont: style is null");
                return;
            }
            CustomFonts.FontLog($"TryApplyFont: setting Style.Font on uiBuilder#{ui.GetHashCode()} slot={slot.GetType().Name}#{slot.GetHashCode()}");
            if (!TrySetMemberValue(style, "Font", sourceFont))
            {
                CustomFonts.FontLog("TryApplyFont: TrySetMemberValue Style.Font failed, trying TryAssignFontChainToMemberAssetRef");
                TryAssignFontChainToMemberAssetRef(style, "Font", sourceFont);
            }

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
            {
                CustomFonts.FontLog("TryApplyFontToExistingTexts: Text type not found");
                return;
            }

            CustomFonts.FontLog($"TryApplyFontToExistingTexts: scanning slot {rootSlot.GetType().Name}#{rootSlot.GetHashCode()} for Text components");

            var foundAny = false;

            // Try GetComponentsInChildren<Text>() first (finds at any depth)
            foreach (var textComp in SlotGraphWalker.EnumerateComponentsInChildrenOfSlot(rootSlot, textType))
            {
                if (textComp == null)
                    continue;
                foundAny = true;
                CustomFonts.FontLog($"TryApplyFontToExistingTexts: found Text#{textComp.GetHashCode()} via GetComponentsInChildren, setting Font");
                TryAssignFontChainToMemberAssetRef(textComp, "Font", font);
            }

            // Fallback: walk direct children and try GetComponent<Text>() on each
            if (!foundAny)
            {
                CustomFonts.FontLog("TryApplyFontToExistingTexts: GetComponentsInChildren found nothing, trying direct children fallback");
                var childrenCount = ReadMemberValue(rootSlot, "ChildrenCount");
                if (childrenCount is int cnt && cnt > 0)
                {
                    var indexerType = AccessTools.TypeByName("FrooxEngine.Slot");
                    if (indexerType != null)
                    {
                        PropertyInfo? childIndexer = null;
                        foreach (var p in indexerType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                        {
                            if (!p.CanRead || p.GetIndexParameters().Length != 1)
                                continue;
                            if (p.GetIndexParameters()[0].ParameterType != typeof(int))
                                continue;
                            childIndexer = p;
                            break;
                        }
                        if (childIndexer != null)
                        {
                            for (var i = 0; i < cnt; i++)
                            {
                                var child = SafeRead(() => childIndexer.GetValue(rootSlot, [i]));
                                if (child == null)
                                    continue;
                                var textComp = GetComponentOnSlot(child, textType);
                                if (textComp != null)
                                {
                                    foundAny = true;
                                    CustomFonts.FontLog($"TryApplyFontToExistingTexts: found Text#{textComp.GetHashCode()} via direct child, setting Font");
                                    TryAssignFontChainToMemberAssetRef(textComp, "Font", font);
                                }
                                // Also recurse into this child's children
                                var grandchildCount = ReadMemberValue(child, "ChildrenCount");
                                if (grandchildCount is int gc && gc > 0)
                                {
                                    for (var j = 0; j < gc; j++)
                                    {
                                        var grandchild = SafeRead(() => childIndexer.GetValue(child, [j]));
                                        if (grandchild == null)
                                            continue;
                                        var gcText = GetComponentOnSlot(grandchild, textType);
                                        if (gcText != null)
                                        {
                                            foundAny = true;
                                            CustomFonts.FontLog($"TryApplyFontToExistingTexts: found Text#{gcText.GetHashCode()} via grandchild, setting Font");
                                            TryAssignFontChainToMemberAssetRef(gcText, "Font", font);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!foundAny)
            {
                CustomFonts.FontLog("TryApplyFontToExistingTexts: NO Text components found under this slot (any method)");
            }
        }
    }
}
