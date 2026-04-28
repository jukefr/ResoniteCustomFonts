using System.Reflection;

namespace CustomFonts;

/// <summary>Reflection utilities for reading/writing members across FrooxEngine types at runtime.</summary>
public static class ReflectionHelpers
{
    /// <summary>
    /// Searches all loaded assemblies for a type by name — equivalent to Harmony's <c>AccessTools.TypeByName</c>
    /// but without a HarmonyLib dependency. Used in Core (embeddable) instead of <c>Type.GetType()</c> which
    /// only searches the calling assembly and corelib.
    /// </summary>
    public static Type? TypeByName(string fullName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(fullName))
            .FirstOrDefault(t => t != null);
    }
    /// <summary>Invoke <paramref name="reader"/>; return null on any exception.</summary>
    public static object? SafeRead(Func<object?> reader)
    {
        try { return reader(); } catch { return null; }
    }

    /// <summary>
    /// <see cref="FrooxEngine.Slot"/> exposes more than one instance property named <c>Parent</c>; <c>Type.GetProperty(name)</c>
    /// throws <see cref="AmbiguousMatchException"/> and breaks every parent walk (huge FPS hit + no font resolution).
    /// </summary>
    public static PropertyInfo? FindReadableInstanceProperty(Type type, string name, BindingFlags flags)
    {
        PropertyInfo? preferred = null;
        PropertyInfo? fallback = null;
        foreach (var prop in type.GetProperties(flags))
        {
            if (prop.Name != name || !prop.CanRead || prop.GetIndexParameters().Length != 0)
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
        foreach (var prop in type.GetProperties(flags))
        {
            if (prop.Name != name || !prop.CanWrite || prop.GetIndexParameters().Length != 0)
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
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var field = type.GetField(name, flags);
        if (field != null)
            return SafeRead(() => field.GetValue(instance));

        var prop = FindReadableInstanceProperty(type, name, flags);
        if (prop != null)
            return SafeRead(() => prop.GetValue(instance));

        return null;
    }

    public static bool TrySetMemberValue(object? instance, string name, object? value)
    {
        if (instance == null)
            return false;
        var type = instance.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var field = type.GetField(name, flags);
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

        var prop = FindWritableInstanceProperty(type, name, flags);
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

    public static bool TrySetAssetRefTarget(object? assetRefBox, object? value)
    {
        if (assetRefBox == null || value == null)
            return false;
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var rt = assetRefBox.GetType();
        foreach (var prop in rt.GetProperties(flags))
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

        foreach (var field in rt.GetFields(flags))
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
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var ot = owner.GetType();
        var field = ot.GetField(memberName, flags);
        object? box = field != null ? SafeRead(() => field.GetValue(owner)) : null;
        if (box == null)
        {
            var prop = FindReadableInstanceProperty(ot, memberName, flags);
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

    public static object? TryGetPropertyValueAcrossInheritance(object? target, string propertyName)
    {
        if (target == null)
            return null;
        for (var t = target.GetType(); t != null; t = t.BaseType)
        {
            try
            {
                var p = t.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null && p.GetIndexParameters().Length == 0)
                    return SafeRead(() => p.GetValue(target));
            }
            catch
            {
                continue;
            }
        }

        return null;
    }
}
