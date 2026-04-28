using CustomFonts;
using Xunit;

namespace CustomFonts.Tests;

public class SlotGraphWalkerTests
{
    private class FakeSlot
    {
        public object? Parent { get; set; }
        public int ChildrenCount { get; set; }
        public List<object>? Components { get; set; }
        public List<object>? _components { get; set; }
    }

    [Fact]
    public void EnumerateParentSlots_YieldsSelfThenParent()
    {
        var grandparent = new FakeSlot();
        var parent = new FakeSlot { Parent = grandparent };
        var child = new FakeSlot { Parent = parent };

        var result = SlotGraphWalker.EnumerateParentSlots(child).ToList();
        Assert.Equal(3, result.Count);
        Assert.Same(child, result[0]);
        Assert.Same(parent, result[1]);
        Assert.Same(grandparent, result[2]);
    }

    [Fact]
    public void EnumerateParentSlots_StopsAtNullParent()
    {
        var slot = new FakeSlot();
        var result = SlotGraphWalker.EnumerateParentSlots(slot).ToList();
        Assert.Single(result);
        Assert.Same(slot, result[0]);
    }

    [Fact]
    public void EnumerateComponentsOnSlot_ReturnsEmpty_ForNullComponentsList()
    {
        var slot = new FakeSlot();
        var result = SlotGraphWalker.EnumerateComponentsOnSlot(slot).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void EnumerateComponentsOnSlot_YieldsFromComponentsList()
    {
        var comp1 = new object();
        var comp2 = new object();
        var slot = new FakeSlot { Components = new List<object> { comp1, comp2 } };

        var result = SlotGraphWalker.EnumerateComponentsOnSlot(slot).ToList();
        Assert.Equal(2, result.Count);
        Assert.Contains(comp1, result);
        Assert.Contains(comp2, result);
    }

    [Fact]
    public void EnumerateComponentsOnSlot_YieldsFrom_componentsList()
    {
        var comp = new object();
        var slot = new FakeSlot { _components = new List<object> { comp } };

        var result = SlotGraphWalker.EnumerateComponentsOnSlot(slot).ToList();
        Assert.Single(result);
        Assert.Same(comp, result[0]);
    }

    [Fact]
    public void EnumerateDescendantSlots_YieldsRootOnly_WhenNoChildren()
    {
        var root = new FakeSlot { ChildrenCount = 0 };
        var result = SlotGraphWalker.EnumerateDescendantSlots(root).ToList();
        Assert.Single(result);
        Assert.Same(root, result[0]);
    }

    [Fact]
    public void EnumerateDescendantSlots_ReturnsEmpty_ForNullRoot()
    {
        var result = SlotGraphWalker.EnumerateDescendantSlots(null!).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void EnumerateComponentsInChildrenOfSlot_ReturnsEmpty_ForNullRoot()
    {
        var result = SlotGraphWalker.EnumerateComponentsInChildrenOfSlot(null!, typeof(object)).ToList();
        Assert.Empty(result);
    }
}
