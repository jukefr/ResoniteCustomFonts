using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace CustomFonts;

/// <summary>Slot graph traversal utilities — walks parent/child/compoent hierarchies via reflection.</summary>
public static class SlotGraphWalker
{
    // ---- Cached member info for common slot operations ----

    private static readonly ConcurrentDictionary<(Type SlotType, string Name), MethodInfo?> MethodByParamCountCache = new();

    /// <summary>Cache for GetComponents method lookups per slot type.</summary>
    private static readonly ConcurrentDictionary<Type, (MethodInfo? NoArg, MethodInfo? WithTypeArg)> ComponentsMethodCache = new();

    /// <summary>Cache for GetComponent method lookups per slot type.</summary>
    private static readonly ConcurrentDictionary<Type, (MethodInfo? ByType, MethodInfo? Generic)> GetComponentMethodCache = new();

    /// <summary>Cache for GetComponentInParents method lookups per slot type.</summary>
    private static readonly ConcurrentDictionary<Type, MethodInfo?> GetComponentInParentsMethodCache = new();

    /// <summary>Cache for GetComponentsInChildren method lookups per slot type.</summary>
    private static readonly ConcurrentDictionary<Type, MethodInfo?> GetComponentsInChildrenMethodCache = new();

    internal const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

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
        var (getComponentsNoArg, getComponentsWithType) = GetComponentsMethods(slotType);

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

        if (getComponentsWithType != null)
        {
            var fontChainType = ReflectionHelpers.TypeByName("FrooxEngine.FontChain");
            if (fontChainType != null)
            {
                var arr = ReflectionHelpers.SafeRead(() => getComponentsWithType.Invoke(slot, [fontChainType]));
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
                var arr = ReflectionHelpers.SafeRead(() => getComponentsWithType.Invoke(slot, [textType]));
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

        // Fallback: check known field names for component storage
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

    private static (MethodInfo? NoArg, MethodInfo? WithTypeArg) GetComponentsMethods(Type slotType)
    {
        return ComponentsMethodCache.GetOrAdd(slotType, type =>
        {
            var methods = ReflectionHelpers.GetCachedMethods(type, "GetComponents");
            MethodInfo? noArg = null;
            MethodInfo? withTypeArg = null;
            foreach (var m in methods)
            {
                var ps = m.GetParameters();
                if (ps.Length == 0)
                    noArg = m;
                else if (ps.Length == 1 && ps[0].ParameterType == typeof(Type))
                    withTypeArg = m;
                if (noArg != null && withTypeArg != null)
                    break;
            }
            return (noArg, withTypeArg);
        });
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
        var methods = GetComponentMethods(slotType);

        // Try the Type-parameter overload first (non-generic)
        if (methods.ByType != null)
        {
            var result = ReflectionHelpers.SafeRead(() => methods.ByType.Invoke(slot, [componentType]));
            if (result != null)
                return result;
        }

        // Fallback: try generic overload with 0 parameters
        if (methods.Generic != null)
        {
            try
            {
                return methods.Generic.MakeGenericMethod(componentType).Invoke(slot, null);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static (MethodInfo? ByType, MethodInfo? Generic) GetComponentMethods(Type slotType)
    {
        return GetComponentMethodCache.GetOrAdd(slotType, type =>
        {
            var methods = ReflectionHelpers.GetCachedMethods(type, "GetComponent");
            MethodInfo? byType = null;
            MethodInfo? generic = null;
            foreach (var m in methods)
            {
                var ps = m.GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType == typeof(Type))
                    byType = m;
                else if (m.IsGenericMethodDefinition && ps.Length == 0)
                    generic = m;
                if (byType != null && generic != null)
                    break;
            }
            return (byType, generic);
        });
    }

    public static object? GetComponentInParents(object slot, Type componentType)
    {
        if (slot == null)
            return null;
        var slotType = slot.GetType();
        var method = GetComponentInParentsCached(slotType);
        if (method == null)
            return null;

        var ps = method.GetParameters();
        try
        {
            var constructed = method.MakeGenericMethod(componentType);
            if (ps.Length == 0)
                return constructed.Invoke(slot, null);
            if (ps.Length == 1 && ps[0].ParameterType == typeof(bool))
            {
                var r = constructed.Invoke(slot, [false]);
                if (r != null)
                    return r;
                return constructed.Invoke(slot, [true]);
            }
        }
        catch
        {
        }
        return null;
    }

    private static MethodInfo? GetComponentInParentsCached(Type slotType)
    {
        return GetComponentInParentsMethodCache.GetOrAdd(slotType, type =>
        {
            foreach (var m in ReflectionHelpers.GetCachedMethods(type, "GetComponentInParents"))
            {
                if (m.IsGenericMethodDefinition)
                    return m;
            }
            return null;
        });
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
        var match = GetComponentsInChildrenCached(slotType);
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

    private static MethodInfo? GetComponentsInChildrenCached(Type slotType)
    {
        return GetComponentsInChildrenMethodCache.GetOrAdd(slotType, type =>
        {
            foreach (var m in ReflectionHelpers.GetCachedMethods(type, "GetComponentsInChildren"))
            {
                if (!m.IsGenericMethodDefinition)
                    continue;
                if (!m.ReturnType.IsGenericType || m.ReturnType.GetGenericTypeDefinition() != typeof(List<>))
                    continue;
                if (m.GetParameters().Length != 4)
                    continue;
                return m;
            }
            return null;
        });
    }
}
