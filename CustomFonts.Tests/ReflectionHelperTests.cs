using System.Reflection;
using CustomFonts;

namespace CustomFonts.Tests;

public class ReflectionHelperTests
{
    private static readonly Type PatchesType = typeof(CustomFonts.CustomFontsPatches);

    private static T? InvokeStatic<T>(string methodName, params object?[] args)
    {
        var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
        var method = PatchesType.GetMethod(methodName, flags);
        Assert.NotNull(method);
        return (T?)method!.Invoke(null, args);
    }

    // ---- SafeRead ----

    [Fact]
    public void SafeRead_ReturnsValue_WhenReaderSucceeds()
    {
        Func<object?> reader = () => 42;
        var result = InvokeStatic<object?>("SafeRead", reader);
        Assert.Equal(42, result);
    }

    [Fact]
    public void SafeRead_ReturnsNull_WhenReaderThrows()
    {
        Func<object?> throwingReader = () => throw new InvalidOperationException("expected");
        var result = InvokeStatic<object?>("SafeRead", throwingReader);
        Assert.Null(result);
    }

    [Fact]
    public void SafeRead_ReturnsNull_WhenReaderReturnsNull()
    {
        Func<object?> nullReader = () => null;
        var result = InvokeStatic<object?>("SafeRead", nullReader);
        Assert.Null(result);
    }

    // ---- FindReadableInstanceProperty ----

    private class TestProps
    {
        public string Name { get; set; } = "default";
        public int Value { get; set; }
        private string Secret { get; set; } = "secret";
        public string? Nullable { get; set; }
        public string this[int i] => i.ToString();
    }

    private class TwoCandidateProps
    {
        public object Parent { get; set; } = new object();
        public string ParentAsString { get; set; } = "slotish";
    }

    [Fact]
    public void FindReadableInstanceProperty_FindsPublicProperty()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var method = PatchesType.GetMethod("FindReadableInstanceProperty",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var prop = method.Invoke(null, new object?[] { typeof(TestProps), "Name", flags });
        Assert.NotNull(prop);
        Assert.Equal("Name", ((PropertyInfo)prop!).Name);
    }

    [Fact]
    public void FindReadableInstanceProperty_ReturnsNull_ForMissingProperty()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var method = PatchesType.GetMethod("FindReadableInstanceProperty",
            BindingFlags.Static | BindingFlags.NonPublic);

        var prop = method!.Invoke(null, new object?[] { typeof(TestProps), "NonExistent", flags });
        Assert.Null(prop);
    }

    [Fact]
    public void FindReadableInstanceProperty_SkipsIndexer()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var method = PatchesType.GetMethod("FindReadableInstanceProperty",
            BindingFlags.Static | BindingFlags.NonPublic);

        var prop = method!.Invoke(null, new object?[] { typeof(TestProps), "Item", flags });
        Assert.Null(prop);
    }

    [Fact]
    public void FindReadableInstanceProperty_PrefersExactMatch_WhenMultiple()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var method = PatchesType.GetMethod("FindReadableInstanceProperty",
            BindingFlags.Static | BindingFlags.NonPublic);

        var prop = method!.Invoke(null, new object?[] { typeof(TwoCandidateProps), "Parent", flags });
        Assert.NotNull(prop);
        Assert.Equal("Parent", ((PropertyInfo)prop!).Name);
    }

    // ---- ReadMemberValue ----

    [Fact]
    public void ReadMemberValue_ReadsPublicProperty()
    {
        var obj = new { Name = "test", Value = 1 };
        var result = InvokeStatic<object?>("ReadMemberValue", obj, "Name");
        Assert.Equal("test", result);
    }

    [Fact]
    public void ReadMemberValue_ReadsPublicField()
    {
        var obj = new WithField { Field = "fieldValue" };
        var result = InvokeStatic<object?>("ReadMemberValue", obj, "Field");
        Assert.Equal("fieldValue", result);
    }

    private class WithField
    {
        public string Field = "default";
    }

    [Fact]
    public void ReadMemberValue_ReturnsNull_ForMissingMember()
    {
        var obj = new { X = 1 };
        var result = InvokeStatic<object?>("ReadMemberValue", obj, "Y");
        Assert.Null(result);
    }

    // ---- TrySetMemberValue ----

    [Fact]
    public void TrySetMemberValue_SetsPublicProperty()
    {
        var obj = new TestProps();
        var result = InvokeStatic<bool>("TrySetMemberValue", obj, "Name", "updated");
        Assert.True(result);
        Assert.Equal("updated", obj.Name);
    }

    [Fact]
    public void TrySetMemberValue_SetsPublicField()
    {
        var obj = new WithField();
        var result = InvokeStatic<bool>("TrySetMemberValue", obj, "Field", "updated");
        Assert.True(result);
        Assert.Equal("updated", obj.Field);
    }

    // ---- TrySetAssetRefTarget ----

    private class AssetRefBox
    {
        public object? Target { get; set; }
    }

    private class AssetRefBoxField
    {
#pragma warning disable CS0649
        public object? Target;
#pragma warning restore CS0649
    }

    [Fact]
    public void TrySetAssetRefTarget_SetsViaProperty()
    {
        var box = new AssetRefBox();
        var target = new object();
        var result = InvokeStatic<bool>("TrySetAssetRefTarget", box, target);
        Assert.True(result);
        Assert.Same(target, box.Target);
    }

    [Fact]
    public void TrySetAssetRefTarget_SetsViaField()
    {
        var box = new AssetRefBoxField();
        var target = new object();
        var result = InvokeStatic<bool>("TrySetAssetRefTarget", box, target);
        Assert.True(result);
        Assert.Same(target, box.Target);
    }

    [Fact]
    public void TrySetAssetRefTarget_ReturnsFalse_WhenNoTargetMember()
    {
        var box = new { Id = 1 };
        var result = InvokeStatic<bool>("TrySetAssetRefTarget", box, new object());
        Assert.False(result);
    }

    [Fact]
    public void TrySetAssetRefTarget_ReturnsFalse_ForNullBox()
    {
        var result = InvokeStatic<bool>("TrySetAssetRefTarget", null, new object());
        Assert.False(result);
    }

    [Fact]
    public void TrySetAssetRefTarget_ReturnsFalse_ForNullValue()
    {
        var box = new AssetRefBox();
        var result = InvokeStatic<bool>("TrySetAssetRefTarget", box, null);
        Assert.False(result);
    }

    // ---- TryAssignFontChainToMemberAssetRef ----

    private class OwnerWithRefBox
    {
        public AssetRefBox? Font { get; set; }
    }

    private class OwnerWithRefBoxField
    {
        public AssetRefBoxField? Font;
    }

    [Fact]
    public void TryAssignFontChainToMemberAssetRef_AssignsViaProperty()
    {
        var owner = new OwnerWithRefBox { Font = new AssetRefBox() };
        var font = new object();
        var result = InvokeStatic<bool>("TryAssignFontChainToMemberAssetRef", owner, "Font", font);
        Assert.True(result);
        Assert.Same(font, owner.Font!.Target);
    }

    [Fact]
    public void TryAssignFontChainToMemberAssetRef_AssignsViaField()
    {
        var owner = new OwnerWithRefBoxField { Font = new AssetRefBoxField() };
        var font = new object();
        var result = InvokeStatic<bool>("TryAssignFontChainToMemberAssetRef", owner, "Font", font);
        Assert.True(result);
    }

    [Fact]
    public void TryAssignFontChainToMemberAssetRef_ReturnsFalse_WhenNoFontMember()
    {
        var owner = new { Id = 1 };
        var result = InvokeStatic<bool>("TryAssignFontChainToMemberAssetRef", owner, "Font", new object());
        Assert.False(result);
    }

    [Fact]
    public void TryAssignFontChainToMemberAssetRef_ReturnsFalse_WhenOwnerNull()
    {
        var result = InvokeStatic<bool>("TryAssignFontChainToMemberAssetRef", null, "Font", new object());
        Assert.False(result);
    }

    [Fact]
    public void TryAssignFontChainToMemberAssetRef_ReturnsFalse_WhenFontNull()
    {
        var owner = new OwnerWithRefBox { Font = new AssetRefBox() };
        var result = InvokeStatic<bool>("TryAssignFontChainToMemberAssetRef", owner, "Font", null);
        Assert.False(result);
    }

    // ---- IsLikelyFontChain ----

    private class FontChainDummy { }

    [Fact]
    public void IsLikelyFontChain_ReturnsTrue_ForTypeWithFontChainInName()
    {
        var result = InvokeStatic<bool>("IsLikelyFontChain", typeof(FontChainDummy));
        Assert.True(result);
    }

    [Fact]
    public void IsLikelyFontChain_ReturnsFalse_ForNonFontChainType()
    {
        var result = InvokeStatic<bool>("IsLikelyFontChain", typeof(string));
        Assert.False(result);
    }

    [Fact]
    public void IsLikelyFontChain_ReturnsTrue_ForFullyQualifiedFontChainName()
    {
        var result = InvokeStatic<bool>("IsLikelyFontChain", typeof(FontChainDummy));
        Assert.True(result);
    }

    // ---- FindWritableInstanceProperty ----

    private class WriteOnlyProps
    {
        private string _value = "";
        public string ReadWrite { get => _value; set => _value = value; }
        public string WriteOnly { set => _value = value; }
        public int this[int i]
        {
            get => i;
            set { }
        }
    }

    [Fact]
    public void FindWritableInstanceProperty_FindsReadWriteProperty()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var method = PatchesType.GetMethod("FindWritableInstanceProperty",
            BindingFlags.Static | BindingFlags.NonPublic);

        var prop = method!.Invoke(null, new object?[] { typeof(WriteOnlyProps), "ReadWrite", flags });
        Assert.NotNull(prop);
        Assert.Equal("ReadWrite", ((PropertyInfo)prop!).Name);
    }

    [Fact]
    public void FindWritableInstanceProperty_ReturnsNull_ForMissing()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var method = PatchesType.GetMethod("FindWritableInstanceProperty",
            BindingFlags.Static | BindingFlags.NonPublic);

        var prop = method!.Invoke(null, new object?[] { typeof(WriteOnlyProps), "NonExistent", flags });
        Assert.Null(prop);
    }

    [Fact]
    public void FindWritableInstanceProperty_SkipsIndexer()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var method = PatchesType.GetMethod("FindWritableInstanceProperty",
            BindingFlags.Static | BindingFlags.NonPublic);

        var prop = method!.Invoke(null, new object?[] { typeof(WriteOnlyProps), "Item", flags });
        Assert.Null(prop);
    }
}
