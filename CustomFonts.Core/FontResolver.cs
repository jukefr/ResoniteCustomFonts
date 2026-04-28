using System.Reflection;

namespace CustomFonts;

/// <summary>Font resolution logic — finds FontChain components on avatar slots via tag matching.</summary>
public static class FontResolver
{
    /// <summary>
    /// <see cref="FrooxEngine.UIX.UIBuilder"/> has <c>Root</c> and <c>Canvas</c>, not a <c>Slot</c> property — resolving context from <c>Root</c> is required for style/font patches.
    /// </summary>
    public static object? GetContextSlotFromUiBuilder(object? ui)
    {
        var uiBuilderType = UiBuilderType;
        if (ui == null || uiBuilderType == null || !uiBuilderType.IsInstanceOfType(ui))
            return null;
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var rootProp = uiBuilderType.GetProperty("Root", flags);
        if (rootProp != null)
        {
            var root = ReflectionHelpers.SafeRead(() => rootProp.GetValue(ui));
            if (root != null)
                return root;
        }

        var slotProp = uiBuilderType.GetProperty("Slot", flags);
        if (slotProp != null)
        {
            var slot = ReflectionHelpers.SafeRead(() => slotProp.GetValue(ui));
            if (slot != null)
                return slot;
        }

        var canvasProp = uiBuilderType.GetProperty("Canvas", flags);
        var canvas = canvasProp == null ? null : ReflectionHelpers.SafeRead(() => canvasProp.GetValue(ui));
        return canvas == null ? null : ReflectionHelpers.ReadMemberValue(canvas, "Slot");
    }

    private static Type? _cachedUiBuilderType;

    internal static Type? UiBuilderType =>
        _cachedUiBuilderType ??= Type.GetType("FrooxEngine.UIX.UIBuilder");

    /// <summary>Font source: first <see cref="FrooxEngine.FontChain"/> on a descendant of <see cref="FrooxEngine.User.Root"/>.<c>Slot</c> whose slot tag matches <paramref name="tag"/>.</summary>
    public static object? ResolveFontProviderForUiSlotWithTag(object slot, string tag, Func<string> fontSourceSlotTag, Func<string> boldFontSourceSlotTag)
    {
        var world = ReflectionHelpers.ReadMemberValue(slot, "World");
        var avatarRoot = ResolveLocalUserAvatarRootSlot(world);
        if (avatarRoot == null)
            return null;

        foreach (var s in SlotGraphWalker.EnumerateDescendantSlots(avatarRoot))
        {
            if (s == null)
                continue;
            var slotTag = ReadSlotTagString(s);
            if (!string.Equals(slotTag, tag, StringComparison.Ordinal))
                continue;
            var chain = SlotGraphWalker.EnumerateFontChainComponentsOnSlot(s).FirstOrDefault();
            if (chain != null)
                return chain;
            continue;
        }

        return null;
    }

    /// <summary>Primary (normal weight) FontChain from <paramref name="fontSourceSlotTag"/>.</summary>
    public static object? ResolveFontProviderForUiSlot(object slot, Func<string> fontSourceSlotTag, Func<string> boldFontSourceSlotTag) =>
        ResolveFontProviderForUiSlotWithTag(slot, fontSourceSlotTag(), fontSourceSlotTag, boldFontSourceSlotTag);

    /// <summary>Bolder FontChain from <paramref name="boldFontSourceSlotTag"/>; falls back to primary when the bold tag yields nothing.</summary>
    public static object? ResolveBoldFontProviderForUiSlot(object slot, Func<string> fontSourceSlotTag, Func<string> boldFontSourceSlotTag)
    {
        var boldTag = boldFontSourceSlotTag();
        var primaryTag = fontSourceSlotTag();
        var bold = ResolveFontProviderForUiSlotWithTag(slot, boldTag, fontSourceSlotTag, boldFontSourceSlotTag);
        if (bold != null)
            return bold;
        return ResolveFontProviderForUiSlot(slot, fontSourceSlotTag, boldFontSourceSlotTag);
    }

    /// <summary><see cref="FrooxEngine.User.Root"/> → <c>Slot</c> (avatar hierarchy root for the local user).</summary>
    public static object? ResolveLocalUserAvatarRootSlot(object? world)
    {
        if (world == null)
            return null;
        var localUser = ReflectionHelpers.TryGetPropertyValueAcrossInheritance(world, "LocalUser");
        if (localUser == null)
            return null;
        var root = ReflectionHelpers.ReadMemberValue(localUser, "Root");
        if (root == null)
            return null;
        return ReflectionHelpers.ReadMemberValue(root, "Slot");
    }

    private static string? ReadSlotTagString(object? slot)
    {
        if (slot == null)
            return null;
        var raw = ReflectionHelpers.ReadMemberValue(slot, "Tag");
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

    /// <summary>True when the slot is under <see cref="FrooxEngine.World.LocalUserSpace"/>.</summary>
    public static bool SlotIsUnderWorldLocalUserSpace(object? slot)
    {
        if (slot == null)
            return false;
        var world = ReflectionHelpers.ReadMemberValue(slot, "World");
        if (world == null)
            return false;
        var lus = ReflectionHelpers.TryGetPropertyValueAcrossInheritance(world, "LocalUserSpace");
        if (lus == null)
            return false;
        for (var cur = slot; cur != null; cur = ReflectionHelpers.ReadMemberValue(cur, "Parent"))
        {
            if (ReferenceEquals(cur, lus))
                return true;
        }

        return false;
    }

    /// <summary>True when any ancestor reports <c>IsUnderLocalUser</c>.</summary>
    public static bool SlotOrAncestorsUnderLocalUser(object? slot)
    {
        for (var cur = slot; cur != null; cur = ReflectionHelpers.ReadMemberValue(cur, "Parent"))
        {
            if (TrySlotIsUnderLocalUser(cur, out var ok) && ok)
                return true;
        }
        return false;
    }

    private static bool TrySlotIsUnderLocalUser(object? slot, out bool isUnder)
    {
        isUnder = false;
        if (slot == null)
            return false;
        var world = ReflectionHelpers.ReadMemberValue(slot, "World");
        var worldLu = world == null ? null : ReflectionHelpers.TryGetPropertyValueAcrossInheritance(world, "LocalUser");
        foreach (var m in slot.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (m.Name != "IsUnderLocalUser" || m.ReturnType != typeof(bool))
                continue;
            var ps = m.GetParameters();
            try
            {
                if (ps.Length == 0)
                {
                    if (ReflectionHelpers.SafeRead(() => m.Invoke(slot, null)) is bool b)
                    {
                        isUnder = b;
                        return true;
                    }
                }
                else if (ps.Length == 1 && worldLu != null && ps[0].ParameterType.IsInstanceOfType(worldLu))
                {
                    if (ReflectionHelpers.SafeRead(() => m.Invoke(slot, [worldLu])) is bool b2)
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

    /// <summary>FrooxEngine inspector workers expose <c>LocalUser</c> (who owns the dev tool). Only that user's inspector UI is modified on this client.</summary>
    public static bool WorkerBelongsToThisClient(object? workerLike)
    {
        if (workerLike == null)
            return false;
        var worldLu = ReflectionHelpers.TryGetPropertyValueAcrossInheritance(ReflectionHelpers.ReadMemberValue(workerLike, "World"), "LocalUser");
        if (worldLu == null)
            return false;
        var compLu = ReflectionHelpers.TryGetPropertyValueAcrossInheritance(workerLike, "LocalUser");
        if (compLu != null)
            return ReferenceEquals(compLu, worldLu);
        return SlotOrAncestorsUnderLocalUser(ReflectionHelpers.ReadMemberValue(workerLike, "Slot"));
    }

    /// <summary>Finds hierarchy/component inspector panels (including variants that are not <see cref="FrooxEngine.SceneInspector"/> CLR type).</summary>
    public static object? FindInspectorPanelFromSlot(object? startSlot)
    {
        if (startSlot == null)
            return null;
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        foreach (var s in SlotGraphWalker.EnumerateParentSlots(startSlot))
        {
            foreach (var comp in SlotGraphWalker.EnumerateComponentsOnSlot(s))
            {
                if (comp == null)
                    continue;
                try
                {
                    var ct = comp.GetType();
                    if (ct.GetField("_hierarchyContentRoot", flags) == null)
                        continue;
                    if (ct.GetField("_componentsContentRoot", flags) == null)
                        continue;
                    return comp;
                }
                catch
                {
                    continue;
                }
            }
        }

        return null;
    }

    /// <summary>Determines whether custom fonts should apply for a given UI slot (inspector panels, worker inspector, component selector, local user space).</summary>
    public static bool ShouldApplyCustomFontsForUiSlot(object? slot)
    {
        if (slot == null)
            return false;
        var panel = FindInspectorPanelFromSlot(slot);
        if (panel != null)
            return WorkerBelongsToThisClient(panel);

        // WorkerInspector (e.g. detached component window) has no _hierarchy/_components roots — still an IWorker inspector.
        var workerInspectorType = Type.GetType("FrooxEngine.WorkerInspector");
        if (workerInspectorType != null)
        {
            var wi = SlotGraphWalker.GetComponentInParents(slot, workerInspectorType);
            if (wi != null)
                return WorkerBelongsToThisClient(wi);
        }

        // Attach Component browser lives under ComponentSelector, not SceneInspector.
        var componentSelectorType = Type.GetType("FrooxEngine.ComponentSelector");
        if (componentSelectorType != null)
        {
            var cs = SlotGraphWalker.GetComponentInParents(slot, componentSelectorType);
            if (cs != null)
                return WorkerBelongsToThisClient(cs);
        }

        // Other dev-tool panels (RadiantUI under LocalUserSpace) may not report IsUnderLocalUser on every ancestor.
        if (SlotIsUnderWorldLocalUserSpace(slot))
            return true;

        return SlotOrAncestorsUnderLocalUser(slot);
    }
}
