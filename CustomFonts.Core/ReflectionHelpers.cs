using System.Collections.Concurrent;
using System.Reflection;

namespace CustomInspectorFonts;

/// <summary>Reflection utilities for reading/writing members across FrooxEngine types at runtime.</summary>
public static class ReflectionHelpers
{
    // ---- Caches ----

    /// <summary>
    /// Cache for TypeByName: type full name → resolved Type.
    /// FrooxEngine types are static (never unloaded), so this cache lives forever.
    /// </summary>
    private static readonly ConcurrentDictionary<string, Type?> TypeByNameCache = new();

    /// <summary>
    /// Cache for FieldInfo lookups: (type, fieldName) → FieldInfo.
    /// </summary>
    private static readonly ConcurrentDictionary<(Type Type, string Name), FieldInfo?> FieldCache = new();

    /// <summary>
    /// Cache for PropertyInfo lookups: (type, propertyName) → PropertyInfo[].
    /// We store an array because Slot has multiple properties named "Parent" (the disambiguation
    /// is done on read — we cache the full scan result so we only scan once per type).
    /// </summary>
    private static readonly ConcurrentDictionary<(Type Type, string Name), PropertyInfo[]> PropertyCache = new();

    /// <summary>
    /// Cache for MethodInfo lookups by name + param count: (type, methodName) → MethodInfo[].
    /// </summary>
    private static readonly ConcurrentDictionary<(Type Type, string Name), MethodInfo[]> MethodCache = new();

    /// <summary>Shared binding flags for all instance member lookups.</summary>
    private const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    // ---- TypeByName with caching ----

    /// <summary>
    /// Searches all loaded assemblies for a type by name — equivalent to Harmony's <c>AccessTools.TypeByName</c>
    /// but without a HarmonyLib dependency. Results are cached per type name.
    /// </summary>
    public static Type? TypeByName(string fullName)
    {
        return TypeByNameCache.GetOrAdd(fullName, name =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(name))
                .FirstOrDefault(t => t != null));
    }

    // ---- SafeRead ----

    /// <summary>Invoke <paramref name="reader"/>; return null on any exception.</summary>
    public static object? SafeRead(Func<object?> reader)
    {
        try { return reader(); } catch { return null; }
    }

    // ---- Field access with caching ----

    private static FieldInfo? GetCachedField(Type type, string name)
    {
        return FieldCache.GetOrAdd((type, name), key =>
        {
            // Walk the type hierarchy to find the field
            for (var t = key.Type; t != null; t = t.BaseType)
            {
                var f = t.GetField(key.Name, InstanceFlags);
                if (f != null)
                    return f;
            }
            return null;
        });
    }

    // ---- Property access with caching ----

    /// <summary>
    /// Returns all properties with the given name (a type can have multiple via inheritance).
    /// Cached per (Type, Name) so we only scan once.
    /// </summary>
    private static PropertyInfo[] GetCachedProperties(Type type, string name)
    {
        return PropertyCache.GetOrAdd((type, name), key =>
        {
            var results = new List<PropertyInfo>();
            for (var t = key.Type; t != null; t = t.BaseType)
            {
                foreach (var prop in t.GetProperties(InstanceFlags))
                {
                    if (prop.Name == key.Name)
                        results.Add(prop);
                }
            }
            return results.ToArray();
        });
    }

    /// <summary>
    /// <see cref="FrooxEngine.Slot"/> exposes more than one instance property named <c>Parent</c>; <c>Type.GetProperty(name)</c>
    /// throws <see cref="AmbiguousMatchException"/> and breaks every parent walk (huge FPS hit + no font resolution).
    /// </summary>
    public static PropertyInfo? FindReadableInstanceProperty(Type type, string name, BindingFlags flags)
    {
        PropertyInfo? preferred = null;
        PropertyInfo? fallback = null;
        foreach (var prop in GetCachedProperties(type, name))
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length != 0)
                continue;
            if (name.Equals("Parent", StringComparison.Ordinal)
                && string.Equals(prop.PropertyType.FullName, "FrooxEngine.Slot", StringComparison.Ordinal))
                preferred = prop;
            fallback ??= prop;
        }

        return preferred ?? fallback;
    }

    public static PropertyInfo? FindWritableInstanceProperty(Type type, string name, BindingFlags flags)
    {
        PropertyInfo? preferred = null;
        PropertyInfo? fallback = null;
        foreach (var prop in GetCachedProperties(type, name))
        {
            if (!prop.CanWrite || prop.GetIndexParameters().Length != 0)
                continue;
            if (name.Equals("Parent", StringComparison.Ordinal)
                && string.Equals(prop.PropertyType.FullName, "FrooxEngine.Slot", StringComparison.Ordinal))
                preferred = prop;
            fallback ??= prop;
        }

        return preferred ?? fallback;
    }

    public static object? ReadMemberValue(object? instance, string name)
    {
        if (instance == null)
            return null;
        var type = instance.GetType();

        var field = GetCachedField(type, name);
        if (field != null)
            return SafeRead(() => field.GetValue(instance));

        var prop = FindReadableInstanceProperty(type, name, InstanceFlags);
        if (prop != null)
            return SafeRead(() => prop.GetValue(instance));

        return null;
    }

    public static bool TrySetMemberValue(object? instance, string name, object? value)
    {
        if (instance == null)
            return false;
        var type = instance.GetType();

        var field = GetCachedField(type, name);
        if (field != null)
        {
            try
            {
                field.SetValue(instance, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        var prop = FindWritableInstanceProperty(type, name, InstanceFlags);
        if (prop != null)
        {
            try
            {
                prop.SetValue(instance, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    // ---- TryGetPropertyValueAcrossInheritance with caching ----

    public static object? TryGetPropertyValueAcrossInheritance(object? target, string propertyName)
    {
        if (target == null)
            return null;
        var type = target.GetType();

        // Use the cached property lookup — GetCachedProperties scans the hierarchy
        var props = GetCachedProperties(type, propertyName);
        foreach (var p in props)
        {
            if (p.GetIndexParameters().Length == 0)
                return SafeRead(() => p.GetValue(target));
        }

        return null;
    }

    // ---- Uncacheable (type varies at runtime) ----

    public static bool TrySetAssetRefTarget(object? assetRefBox, object? value)
    {
        if (assetRefBox == null || value == null)
            return false;
        var rt = assetRefBox.GetType();
        // AssetRef types vary — can't cache by (Type) because different AssetRef<T> types
        // But we CAN cache by (Type, "Target", "Property"/"Field")
        foreach (var prop in rt.GetProperties(InstanceFlags))
        {
            if (prop.Name != "Target" || !prop.CanWrite)
                continue;
            try
            {
                prop.SetValue(assetRefBox, value);
                return true;
            }
            catch
            {
            }
        }

        foreach (var field in rt.GetFields(InstanceFlags))
        {
            if (field.Name != "Target" || field.IsInitOnly)
                continue;
            try
            {
                field.SetValue(assetRefBox, value);
                return true;
            }
            catch
            {
            }
        }

        return false;
    }

    public static bool TryAssignFontChainToMemberAssetRef(object owner, string memberName, object fontChain)
    {
        if (owner == null || fontChain == null)
            return false;
        var ot = owner.GetType();
        var field = GetCachedField(ot, memberName);
        object? box = field != null ? SafeRead(() => field.GetValue(owner)) : null;
        if (box == null)
        {
            var prop = FindReadableInstanceProperty(ot, memberName, InstanceFlags);
            if (prop != null)
                box = SafeRead(() => prop.GetValue(owner));
        }

        return TrySetAssetRefTarget(box, fontChain);
    }

    /// <summary>
    /// <see cref="FrooxEngine.FontChain"/> or any assembly type whose name contains "FontChain".
    /// </summary>
    public static bool IsLikelyFontChain(Type type)
    {
        var name = type.FullName ?? type.Name;
        return name.IndexOf("FontChain", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    // ---- Method cache ----

    /// <summary>Cached GetMethods scan by (Type, methodName) — avoids repeated full-scan.</summary>
    internal static MethodInfo[] GetCachedMethods(Type type, string name)
    {
        return MethodCache.GetOrAdd((type, name), key =>
        {
            return key.Type.GetMethods(InstanceFlags)
                .Where(m => m.Name == key.Name)
                .ToArray();
        });
    }
}
