using System.Reflection;
using CustomInspectorFonts;
using Xunit;

namespace CustomInspectorFonts.Tests;

public class ReflectionHelperTests
{
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
        var prop = ReflectionHelpers.FindReadableInstanceProperty(typeof(TestProps), "Name", flags);
        Assert.NotNull(prop);
        Assert.Equal("Name", prop!.Name);
    }

    [Fact]
    public void FindReadableInstanceProperty_ReturnsNull_ForMissingProperty()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var prop = ReflectionHelpers.FindReadableInstanceProperty(typeof(TestProps), "NonExistent", flags);
        Assert.Null(prop);
    }

    [Fact]
    public void FindReadableInstanceProperty_SkipsIndexer()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var prop = ReflectionHelpers.FindReadableInstanceProperty(typeof(TestProps), "Item", flags);
        Assert.Null(prop);
    }

    [Fact]
    public void FindReadableInstanceProperty_PrefersExactMatch_WhenMultiple()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var prop = ReflectionHelpers.FindReadableInstanceProperty(typeof(TwoCandidateProps), "Parent", flags);
        Assert.NotNull(prop);
        Assert.Equal("Parent", prop!.Name);
    }

    // ---- ReadMemberValue ----

    [Fact]
    public void ReadMemberValue_ReadsPublicProperty()
    {
        var obj = new { Name = "test", Value = 1 };
        var result = ReflectionHelpers.ReadMemberValue(obj, "Name");
        Assert.Equal("test", result);
    }

    [Fact]
    public void ReadMemberValue_ReadsPublicField()
    {
        var obj = new WithField { Field = "fieldValue" };
        var result = ReflectionHelpers.ReadMemberValue(obj, "Field");
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
        var result = ReflectionHelpers.ReadMemberValue(obj, "Y");
        Assert.Null(result);
    }

    // ---- TrySetMemberValue ----

    [Fact]
    public void TrySetMemberValue_SetsPublicProperty()
    {
        var obj = new TestProps();
        var result = ReflectionHelpers.TrySetMemberValue(obj, "Name", "updated");
        Assert.True(result);
        Assert.Equal("updated", obj.Name);
    }

    [Fact]
    public void TrySetMemberValue_SetsPublicField()
    {
        var obj = new WithField();
        var result = ReflectionHelpers.TrySetMemberValue(obj, "Field", "updated");
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
        var result = ReflectionHelpers.TrySetAssetRefTarget(box, target);
        Assert.True(result);
        Assert.Same(target, box.Target);
    }

    [Fact]
    public void TrySetAssetRefTarget_SetsViaField()
    {
        var box = new AssetRefBoxField();
        var target = new object();
        var result = ReflectionHelpers.TrySetAssetRefTarget(box, target);
        Assert.True(result);
        Assert.Same(target, box.Target);
    }

    [Fact]
    public void TrySetAssetRefTarget_ReturnsFalse_WhenNoTargetMember()
    {
        var box = new { Id = 1 };
        var result = ReflectionHelpers.TrySetAssetRefTarget(box, new object());
        Assert.False(result);
    }

    [Fact]
    public void TrySetAssetRefTarget_ReturnsFalse_ForNullBox()
    {
        var result = ReflectionHelpers.TrySetAssetRefTarget(null, new object());
        Assert.False(result);
    }

    [Fact]
    public void TrySetAssetRefTarget_ReturnsFalse_ForNullValue()
    {
        var box = new AssetRefBox();
        var result = ReflectionHelpers.TrySetAssetRefTarget(box, null);
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
        var result = ReflectionHelpers.TryAssignFontChainToMemberAssetRef(owner, "Font", font);
        Assert.True(result);
        Assert.Same(font, owner.Font!.Target);
    }

    [Fact]
    public void TryAssignFontChainToMemberAssetRef_AssignsViaField()
    {
        var owner = new OwnerWithRefBoxField { Font = new AssetRefBoxField() };
        var font = new object();
        var result = ReflectionHelpers.TryAssignFontChainToMemberAssetRef(owner, "Font", font);
        Assert.True(result);
    }

    [Fact]
    public void TryAssignFontChainToMemberAssetRef_WithNullOwner_ReturnsFalse()
    {
        var result = ReflectionHelpers.TryAssignFontChainToMemberAssetRef(null!, "Font", new object());
        Assert.False(result);
    }

    [Fact]
    public void TryAssignFontChainToMemberAssetRef_WithNullFont_ReturnsFalse()
    {
        var owner = new OwnerWithRefBox { Font = new AssetRefBox() };
        var result = ReflectionHelpers.TryAssignFontChainToMemberAssetRef(owner, "Font", null);
        Assert.False(result);
    }

    [Fact]
    public void TryAssignFontChainToMemberAssetRef_ReturnsFalse_WhenNoFontMember()
    {
        var owner = new { Id = 1 };
        var result = ReflectionHelpers.TryAssignFontChainToMemberAssetRef(owner, "Font", new object());
        Assert.False(result);
    }

    // ---- IsLikelyFontChain ----

    private class FontChainDummy { }

    [Fact]
    public void IsLikelyFontChain_ReturnsTrue_ForTypeWithFontChainInName()
    {
        var result = ReflectionHelpers.IsLikelyFontChain(typeof(FontChainDummy));
        Assert.True(result);
    }

    [Fact]
    public void IsLikelyFontChain_ReturnsFalse_ForNonFontChainType()
    {
        var result = ReflectionHelpers.IsLikelyFontChain(typeof(string));
        Assert.False(result);
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
        var prop = ReflectionHelpers.FindWritableInstanceProperty(typeof(WriteOnlyProps), "ReadWrite", flags);
        Assert.NotNull(prop);
        Assert.Equal("ReadWrite", prop!.Name);
    }

    [Fact]
    public void FindWritableInstanceProperty_ReturnsNull_ForMissing()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var prop = ReflectionHelpers.FindWritableInstanceProperty(typeof(WriteOnlyProps), "NonExistent", flags);
        Assert.Null(prop);
    }

    [Fact]
    public void FindWritableInstanceProperty_SkipsIndexer()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var prop = ReflectionHelpers.FindWritableInstanceProperty(typeof(WriteOnlyProps), "Item", flags);
        Assert.Null(prop);
    }

    // ---- TryGetPropertyValueAcrossInheritance ----

    private class BaseClass
    {
        public string BaseProp { get; set; } = "base";
    }

    private class DerivedClass : BaseClass
    {
        public string DerivedProp { get; set; } = "derived";
    }

    [Fact]
    public void TryGetPropertyValueAcrossInheritance_FindsInheritedProperty()
    {
        var obj = new DerivedClass();
        var result = ReflectionHelpers.TryGetPropertyValueAcrossInheritance(obj, "BaseProp");
        Assert.Equal("base", result);
    }

    [Fact]
    public void TryGetPropertyValueAcrossInheritance_ReturnsNull_ForNullTarget()
    {
        var result = ReflectionHelpers.TryGetPropertyValueAcrossInheritance(null, "Name");
        Assert.Null(result);
    }

    [Fact]
    public void TryGetPropertyValueAcrossInheritance_ReturnsNull_ForMissingProperty()
    {
        var obj = new { Name = "test" };
        var result = ReflectionHelpers.TryGetPropertyValueAcrossInheritance(obj, "NonExistent");
        Assert.Null(result);
    }

    // ---- TypeByName ----

    [Fact]
    public void TypeByName_FindsLoadedType()
    {
        var result = ReflectionHelpers.TypeByName("System.String");
        Assert.NotNull(result);
        Assert.Equal(typeof(string), result);
    }

    [Fact]
    public void TypeByName_ReturnsNull_ForNonExistentType()
    {
        var result = ReflectionHelpers.TypeByName("Some.NonExistent.Type");
        Assert.Null(result);
    }

    [Fact]
    public void TypeByName_FindsTypeInThisAssembly()
    {
        var result = ReflectionHelpers.TypeByName("CustomInspectorFonts.ReflectionHelpers");
        Assert.NotNull(result);
        Assert.Equal(typeof(ReflectionHelpers), result);
    }
}
