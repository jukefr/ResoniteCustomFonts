using System.Reflection;
using CustomFonts;

namespace CustomFonts.Tests;

public class BolderFontStackTests
{
    private static readonly Type PatchesType = typeof(CustomFonts.CustomFontsPatches);

    [Fact]
    public void BolderFontStack_StackField_Exists()
    {
        var field = PatchesType.GetField("_inspectorBolderFontStack",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(field);

        var value = field!.GetValue(null);
        Assert.NotNull(value);
        var stackType = value.GetType();
        Assert.True(stackType.IsGenericType);
        Assert.Equal(typeof(System.Collections.Generic.Stack<>), stackType.GetGenericTypeDefinition());
    }

    [Fact]
    public void BolderFontStack_LockObject_Exists()
    {
        var field = PatchesType.GetField("BolderFontStackLock",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(field);

        var value = field!.GetValue(null);
        Assert.NotNull(value);
        Assert.IsType<object>(value);
    }

    [Fact]
    public void ShouldUseBoldFontChainInSetupEditorStylePostfix_ReturnsFalse_ForNullUi()
    {
        var method = PatchesType.GetMethod("ShouldUseBoldFontChainInSetupEditorStylePostfix",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(method);

        var result = method!.Invoke(null, new object?[] { null });
        Assert.False((bool)result!);
    }

    [Fact]
    public void WorkerBelongsToThisClient_ReturnsFalse_ForNullWorker()
    {
        var method = PatchesType.GetMethod("WorkerBelongsToThisClient",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(method);

        var result = method!.Invoke(null, new object?[] { null });
        Assert.False((bool)result!);
    }

    [Fact]
    public void ActiveEnabled_ReturnsFalse_WhenInstanceNull()
    {
        var method = typeof(CustomFonts).GetMethod("ActiveEnabled",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(method);

        var result = method!.Invoke(null, null);
        Assert.False((bool)result!);
    }

    [Fact]
    public void FontLoggingEnabled_ReturnsFalse_WhenInstanceNull()
    {
        var method = typeof(CustomFonts).GetMethod("FontLoggingEnabled",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(method);

        var result = method!.Invoke(null, null);
        Assert.False((bool)result!);
    }

    [Fact]
    public void FontSourceSlotTag_ReturnsDefault_WhenInstanceNull()
    {
        var method = typeof(CustomFonts).GetMethod("FontSourceSlotTag",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(method);

        var result = method!.Invoke(null, null);
        Assert.Equal(CustomFonts.DefaultFontSlotTag, result);
    }

    [Fact]
    public void BoldFontSourceSlotTag_ReturnsFontSourceSlotTag_WhenInstanceNull()
    {
        var method = typeof(CustomFonts).GetMethod("BoldFontSourceSlotTag",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(method);

        var result = method!.Invoke(null, null);
        Assert.Equal(CustomFonts.DefaultFontSlotTag, result);
    }

    [Fact]
    public void DefaultFontSlotTag_IsKaytCustomFonts()
    {
        Assert.Equal("Kayt.CustomFonts", CustomFonts.DefaultFontSlotTag);
    }

    [Fact]
    public void TryGetPropertyValueAcrossInheritance_ReturnsNull_ForNullTarget()
    {
        var method = PatchesType.GetMethod("TryGetPropertyValueAcrossInheritance",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var result = method!.Invoke(null, new object?[] { null, "Name" });
        Assert.Null(result);
    }

    [Fact]
    public void TryGetPropertyValueAcrossInheritance_ReadsProperty()
    {
        var method = PatchesType.GetMethod("TryGetPropertyValueAcrossInheritance",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var obj = new { Name = "test-value", Value = 42 };
        var result = method!.Invoke(null, new object?[] { obj, "Name" });
        Assert.Equal("test-value", result);
    }
}
