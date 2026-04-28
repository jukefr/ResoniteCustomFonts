using System;
using System.Reflection;
using HarmonyLib;
namespace CustomInspectorFonts;

public partial class CustomInspectorFonts
{
    public static partial class CustomInspectorFontsPatches
    {
        private static Type? _cachedUiBuilderType;

        internal static Type? UiBuilderType =>
            _cachedUiBuilderType ??= AccessTools.TypeByName("FrooxEngine.UIX.UIBuilder");

        internal static object? GetContextSlotFromUiBuilder(object? ui) =>
            FontResolver.GetContextSlotFromUiBuilder(ui);

        private static readonly Stack<(object World, object Font)> _inspectorBolderFontStack = new();

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
            if (!CustomInspectorFonts.ActiveEnabled())
                return false;
            if (world == null || slot == null)
            {
                if (CustomInspectorFonts.FontLoggingEnabled())
                    CustomInspectorFonts.FontLog($"bolder push skip: worldNull={world == null} slotNull={slot == null}");
                return false;
            }

            if (!FontResolver.ShouldApplyCustomFontsForUiSlot(slot))
                return false;

            var font = FontResolver.ResolveBoldFontProviderForUiSlot(slot, FontSourceSlotTag, BoldFontSourceSlotTag);
            if (font == null)
            {
                if (CustomInspectorFonts.FontLoggingEnabled())
                    CustomInspectorFonts.FontLog($"bolder push skip: ResolveBoldFontProviderForUiSlot null (slot={slot.GetType().Name})");
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

        internal static bool WorkerBelongsToThisClient(object? workerLike) =>
            FontResolver.WorkerBelongsToThisClient(workerLike);

        private static bool SlotOrAncestorsUnderLocalUser(object? slot) =>
            FontResolver.SlotOrAncestorsUnderLocalUser(slot);
    }
}
