using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace CustomInspectorFonts;

public partial class CustomInspectorFonts
{
    public static partial class CustomInspectorFontsPatches
    {
        private static IEnumerable<object> EnumerateParentSlots(object startSlot) =>
            SlotGraphWalker.EnumerateParentSlots(startSlot);

        private static IEnumerable<object> EnumerateFontChainComponentsOnSlot(object slot) =>
            SlotGraphWalker.EnumerateFontChainComponentsOnSlot(slot);

        private static IEnumerable<object> EnumerateComponentsOnSlot(object slot) =>
            SlotGraphWalker.EnumerateComponentsOnSlot(slot);

        private static bool IsLikelyFontChain(Type type) =>
            ReflectionHelpers.IsLikelyFontChain(type);

        private static object? GetComponentOnSlot(object slot, Type componentType) =>
            SlotGraphWalker.GetComponentOnSlot(slot, componentType);

        private static object? GetComponentInParents(object slot, Type componentType) =>
            SlotGraphWalker.GetComponentInParents(slot, componentType);

        internal static IEnumerable<object> EnumerateDescendantSlots(object root) =>
            SlotGraphWalker.EnumerateDescendantSlots(root);

        private static IEnumerable<object> EnumerateComponentsInChildrenOfSlot(object rootSlot, Type componentType) =>
            SlotGraphWalker.EnumerateComponentsInChildrenOfSlot(rootSlot, componentType);
    }
}
