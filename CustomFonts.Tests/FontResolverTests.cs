using CustomFonts;
using Xunit;

namespace CustomFonts.Tests;

public class FontResolverTests
{
    private class FakeSlot
    {
        public string Tag { get; set; } = "";
        public object? World { get; set; }
        public object? Parent { get; set; }
        public int ChildrenCount { get; set; }
    }

    [Fact]
    public void GetContextSlotFromUiBuilder_ReturnsNull_ForNullUi()
    {
        var result = FontResolver.GetContextSlotFromUiBuilder(null);
        Assert.Null(result);
    }

    [Fact]
    public void GetContextSlotFromUiBuilder_ReturnsNull_ForNonUiBuilder()
    {
        var result = FontResolver.GetContextSlotFromUiBuilder("not a UIBuilder");
        Assert.Null(result);
    }

    [Fact]
    public void ResolveLocalUserAvatarRootSlot_ReturnsNull_ForNullWorld()
    {
        var result = FontResolver.ResolveLocalUserAvatarRootSlot(null);
        Assert.Null(result);
    }

    [Fact]
    public void SlotIsUnderWorldLocalUserSpace_ReturnsFalse_ForNullSlot()
    {
        var result = FontResolver.SlotIsUnderWorldLocalUserSpace(null);
        Assert.False(result);
    }

    [Fact]
    public void SlotOrAncestorsUnderLocalUser_ReturnsFalse_ForSimpleSlot()
    {
        var slot = new FakeSlot();
        var result = FontResolver.SlotOrAncestorsUnderLocalUser(slot);
        Assert.False(result);
    }

    [Fact]
    public void WorkerBelongsToThisClient_ReturnsFalse_ForNullWorker()
    {
        var result = FontResolver.WorkerBelongsToThisClient(null);
        Assert.False(result);
    }

    [Fact]
    public void FindInspectorPanelFromSlot_ReturnsNull_ForNullSlot()
    {
        var result = FontResolver.FindInspectorPanelFromSlot(null);
        Assert.Null(result);
    }

    [Fact]
    public void ShouldApplyCustomFontsForUiSlot_ReturnsFalse_ForNullSlot()
    {
        var result = FontResolver.ShouldApplyCustomFontsForUiSlot(null);
        Assert.False(result);
    }
}
