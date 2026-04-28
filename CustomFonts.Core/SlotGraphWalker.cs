using System.Collections;
using System.Reflection;

namespace CustomFonts;

/// <summary>Slot graph traversal utilities — walks parent/child/compoent hierarchies via reflection.</summary>
public static class SlotGraphWalker
{
    public static IEnumerable<object> EnumerateParentSlots(object startSlot)
    {
        var current = startSlot;
        var guard = 0;
        while (current != null && guard < 128)
        {
            yield return current;
            current = ReflectionHelpers.ReadMemberValue(current, "Parent");
            guard++;
        }
    }

    public static IEnumerable<object> EnumerateComponentsOnSlot(object slot)
    {
        var slotType = slot.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var getComponentsNoArg = slotType.GetMethods(flags).FirstOrDefault(m =>
            m.Name == "GetComponents"
            && m.GetParameters().Length == 0);
        if (getComponentsNoArg != null)
        {
            var result = ReflectionHelpers.SafeRead(() => getComponentsNoArg.Invoke(slot, null));
            if (result is IEnumerable enumerable)
            {
                foreach (var c in enumerable)
                {
                    if (c != null)
                        yield return c;
                }
                yield break;
            }
        }

        var getComponentsType = slotType.GetMethods(flags).FirstOrDefault(m =>
            m.Name == "GetComponents"
            && m.GetParameters().Length == 1
            && m.GetParameters()[0].ParameterType == typeof(Type));
        if (getComponentsType != null)
        {
            var fontChainType = ReflectionHelpers.TypeByName("FrooxEngine.FontChain");
            if (fontChainType != null)
            {
                var arr = ReflectionHelpers.SafeRead(() => getComponentsType.Invoke(slot, [fontChainType]));
                if (arr is IEnumerable e)
                {
                    foreach (var c in e)
                    {
                        if (c != null)
                            yield return c;
                    }
                }
            }

            var textType = ReflectionHelpers.TypeByName("FrooxEngine.UIX.Text");
            if (textType != null)
            {
                var arr = ReflectionHelpers.SafeRead(() => getComponentsType.Invoke(slot, [textType]));
                if (arr is IEnumerable e)
                {
                    foreach (var c in e)
                    {
                        if (c != null)
                            yield return c;
                    }
                }
            }

            yield break;
        }

        foreach (var fieldName in new[] { "_components", "components", "Components" })
        {
            var list = ReflectionHelpers.ReadMemberValue(slot, fieldName);
            if (list is IEnumerable enumerable)
            {
                foreach (var c in enumerable)
                {
                    if (c != null)
                        yield return c;
                }
                yield break;
            }
        }
    }

    public static IEnumerable<object> EnumerateFontChainComponentsOnSlot(object slot)
    {
        foreach (var comp in EnumerateComponentsOnSlot(slot))
        {
            if (comp != null && ReflectionHelpers.IsLikelyFontChain(comp.GetType()))
                yield return comp;
        }
    }

    public static object? GetComponentOnSlot(object slot, Type componentType)
    {
        var slotType = slot.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var getByType = slotType.GetMethods(flags).FirstOrDefault(m =>
            m.Name == "GetComponent"
            && m.GetParameters().Length == 1
            && m.GetParameters()[0].ParameterType == typeof(Type));
        if (getByType != null)
            return ReflectionHelpers.SafeRead(() => getByType.Invoke(slot, [componentType]));
        foreach (var m in slotType.GetMethods(flags))
        {
            if (m.Name != "GetComponent" || !m.IsGenericMethodDefinition)
                continue;
            if (m.GetParameters().Length != 0)
                continue;
            try
            {
                return m.MakeGenericMethod(componentType).Invoke(slot, null);
            }
            catch
            {
                // try next overload
            }
        }
        return null;
    }

    public static object? GetComponentInParents(object slot, Type componentType)
    {
        if (slot == null)
            return null;
        var slotType = slot.GetType();
        foreach (var m in slotType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (m.Name != "GetComponentInParents" || !m.IsGenericMethodDefinition)
                continue;
            var ps = m.GetParameters();
            try
            {
                if (ps.Length == 0)
                    return m.MakeGenericMethod(componentType).Invoke(slot, null);
                if (ps.Length == 1 && ps[0].ParameterType == typeof(bool))
                {
                    var r = m.MakeGenericMethod(componentType).Invoke(slot, [false]);
                    if (r != null)
                        return r;
                    return m.MakeGenericMethod(componentType).Invoke(slot, [true]);
                }
            }
            catch
            {
                // try next overload
            }
        }
        return null;
    }

    private static PropertyInfo? _slotIntIndexer;

    /// <summary><see cref="FrooxEngine.Slot"/> child accessor <c>this[int]</c>.</summary>
    private static PropertyInfo? SlotIntIndexerProperty
    {
        get
        {
            if (_slotIntIndexer != null)
                return _slotIntIndexer;
            var slotType = ReflectionHelpers.TypeByName("FrooxEngine.Slot");
            if (slotType == null)
                return null;
            foreach (var p in slotType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!p.CanRead || p.GetIndexParameters().Length != 1)
                    continue;
                if (p.GetIndexParameters()[0].ParameterType != typeof(int))
                    continue;
                _slotIntIndexer = p;
                break;
            }

            return _slotIntIndexer;
        }
    }

    /// <summary>Depth-first walk of <paramref name="root"/> and all descendant slots (bounded).</summary>
    public static IEnumerable<object> EnumerateDescendantSlots(object root)
    {
        if (root == null)
            yield break;
        var stack = new Stack<object>();
        stack.Push(root);
        var steps = 0;
        const int maxSteps = 65536;
        var indexer = SlotIntIndexerProperty;
        while (stack.Count > 0 && steps++ < maxSteps)
        {
            var s = stack.Pop();
            yield return s;
            var countObj = ReflectionHelpers.ReadMemberValue(s, "ChildrenCount");
            if (countObj is not int cnt || cnt <= 0 || indexer == null)
                continue;
            for (var i = 0; i < cnt; i++)
            {
                var idx = i;
                var child = ReflectionHelpers.SafeRead(() => indexer.GetValue(s, [idx]));
                if (child != null)
                    stack.Push(child);
            }
        }
    }

    public static IEnumerable<object> EnumerateComponentsInChildrenOfSlot(object rootSlot, Type componentType)
    {
        if (rootSlot == null || componentType == null)
            yield break;
        var slotType = rootSlot.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        MethodInfo? match = null;
        foreach (var m in slotType.GetMethods(flags))
        {
            if (m.Name != "GetComponentsInChildren" || !m.IsGenericMethodDefinition)
                continue;
            if (!m.ReturnType.IsGenericType || m.ReturnType.GetGenericTypeDefinition() != typeof(List<>))
                continue;
            if (m.GetParameters().Length != 4)
                continue;
            match = m;
            break;
        }

        if (match == null)
            yield break;
        MethodInfo constructed;
        try
        {
            constructed = match.MakeGenericMethod(componentType);
        }
        catch
        {
            yield break;
        }

        var list = ReflectionHelpers.SafeRead(() => constructed.Invoke(rootSlot, [null, false, false, null]));
        if (list is not IEnumerable enumerable)
            yield break;
        foreach (var item in enumerable)
        {
            if (item != null)
                yield return item;
        }
    }
}
