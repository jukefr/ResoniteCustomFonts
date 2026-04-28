using System;
using System.Reflection;

namespace CustomInspectorFonts;

/// <summary>
/// Reflection utilities for reading/writing members across FrooxEngine types at runtime.
/// These delegate to <see cref="ReflectionHelpers"/> in <c>CustomInspectorFonts.Core</c>.
/// </summary>
public partial class CustomInspectorFonts
{
    public static partial class CustomInspectorFontsPatches
    {
        private static object? SafeRead(Func<object?> reader) =>
            ReflectionHelpers.SafeRead(reader);

        private static PropertyInfo? FindReadableInstanceProperty(Type type, string name, BindingFlags flags) =>
            ReflectionHelpers.FindReadableInstanceProperty(type, name, flags);

        private static PropertyInfo? FindWritableInstanceProperty(Type type, string name, BindingFlags flags) =>
            ReflectionHelpers.FindWritableInstanceProperty(type, name, flags);

        private static object? ReadMemberValue(object? instance, string name) =>
            ReflectionHelpers.ReadMemberValue(instance, name);

        private static bool TrySetMemberValue(object? instance, string name, object? value) =>
            ReflectionHelpers.TrySetMemberValue(instance, name, value);

        private static bool TrySetAssetRefTarget(object? assetRefBox, object? value) =>
            ReflectionHelpers.TrySetAssetRefTarget(assetRefBox, value);

        private static bool TryAssignFontChainToMemberAssetRef(object owner, string memberName, object fontChain) =>
            ReflectionHelpers.TryAssignFontChainToMemberAssetRef(owner, memberName, fontChain);

        private static object? TryGetPropertyValueAcrossInheritance(object? target, string propertyName) =>
            ReflectionHelpers.TryGetPropertyValueAcrossInheritance(target, propertyName);
    }
}
