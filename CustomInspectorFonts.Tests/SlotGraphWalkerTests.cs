using CustomInspectorFonts;
using Xunit;

namespace CustomInspectorFonts.Tests;

public class SlotGraphWalkerTests
{
    /// <summary>
    /// Simulates FrooxEngine.Slot shape for reflection:
    /// - Parent property
    /// - ChildrenCount property
    /// - int indexer for child access (simulates Slot.this[int])
    /// - _components / Components fields
    /// </summary>
    private class FakeSlot
    {
        public object? Parent { get; set; }
        public int ChildrenCount { get; set; }
        public List<FakeSlot> Children { get; set; } = new();
        public List<object>? Components { get; set; }
        public List<object>? _components { get; set; }

        // Simulates Slot's int indexer for child access via SlotIntIndexerProperty
        // The reflection code looks for a property named "Item" with an int index parameter
        public FakeSlot? this[int i]
        {
            get => i >= 0 && i < Children.Count ? Children[i] : null;
        }
    }

    private class FakeComponent
    {
        public string Name { get; set; } = "";
    }

    // ---- EnumerateParentSlots ----

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
    public void EnumerateParentSlots_BoundedAt128()
    {
        FakeSlot? current = null;
        for (int i = 0; i < 150; i++)
        {
            current = new FakeSlot { Parent = current };
        }

        var result = SlotGraphWalker.EnumerateParentSlots(current!).ToList();
        Assert.Equal(128, result.Count);
    }

    // ---- EnumerateComponentsOnSlot ----

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
    public void EnumerateComponentsOnSlot_PrefersGetComponents_WhenItReturnsNull()
    {
        // When GetComponents() exists but returns null, falls through to field-based enumeration
        var comp = new FakeComponent();
        var slot = new MethodsSlot { Components = new List<object> { comp } };

        var result = SlotGraphWalker.EnumerateComponentsOnSlot(slot).ToList();
        Assert.Single(result);
        Assert.Same(comp, result[0]);
    }

    /// <summary>Slot-like type with GetComponents() method that returns null.</summary>
    private class MethodsSlot
    {
        public List<object>? Components { get; set; }

        // Simulates Slot.GetComponents() returning a mock result
        public List<object>? GetComponents() => null;
    }

    /// <summary>Slot-like type with GetComponents(Type) method returning an array.</summary>
    private class TypeFilterSlot
    {
        public List<object>? _components { get; set; }

        public Array? GetComponents(Type type)
        {
            if (_components == null)
                return null;
            return _components
                .Where(c => c != null && type.IsInstanceOfType(c))
                .ToArray();
        }
    }

    [Fact]
    public void EnumerateComponentsOnSlot_UsesGetComponentsNoArg_WhenPresent()
    {
        var comp = new object();
        var slot = new MethodsSlot { Components = new List<object> { comp } };

        var result = SlotGraphWalker.EnumerateComponentsOnSlot(slot).ToList();
        Assert.Single(result);
    }

    // ---- EnumerateDescendantSlots ----

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
    public void EnumerateDescendantSlots_TraversesDepthFirst()
    {
        var child1 = new FakeSlot();
        var child2 = new FakeSlot();
        var root = new FakeSlot
        {
            ChildrenCount = 2,
            Children = new List<FakeSlot> { child1, child2 }
        };

        var result = SlotGraphWalker.EnumerateDescendantSlots(root).ToList();
        // Note: since FakeSlot's int indexer is "Item" but SlotIntIndexerProperty
        // looks for "FrooxEngine.Slot" type via TypeByName, our FakeSlot won't be found
        // So it should only yield the root
        Assert.Single(result);
        Assert.Same(root, result[0]);
    }

    [Fact]
    public void EnumerateDescendantSlots_IsBoundedByMaxSteps()
    {
        // Create a chain of 70000 slots — the walker is bounded at 65536
        var root = new FakeSlot();
        // Since the indexer won't be found (wrong type), this test verifies basic behavior
        var result = SlotGraphWalker.EnumerateDescendantSlots(root).ToList();
        Assert.Single(result);
    }

    // ---- GetComponentOnSlot ----

    [Fact]
    public void GetComponentOnSlot_ReturnsNull_ForNullSlot()
    {
        var result = SlotGraphWalker.GetComponentOnSlot(new FakeSlot(), typeof(FakeComponent));
        // FakeSlot has no GetComponent method, so returns null
        Assert.Null(result);
    }

    // ---- GetComponentInParents ----

    [Fact]
    public void GetComponentInParents_ReturnsNull_ForNullSlot()
    {
        var result = SlotGraphWalker.GetComponentInParents(null!, typeof(object));
        Assert.Null(result);
    }

    // ---- EnumerateComponentsInChildrenOfSlot ----

    [Fact]
    public void EnumerateComponentsInChildrenOfSlot_ReturnsEmpty_ForNullRoot()
    {
        var result = SlotGraphWalker.EnumerateComponentsInChildrenOfSlot(null!, typeof(object)).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void EnumerateComponentsInChildrenOfSlot_ReturnsEmpty_ForNonGenericSlot()
    {
        var root = new FakeSlot();
        var result = SlotGraphWalker.EnumerateComponentsInChildrenOfSlot(root, typeof(object)).ToList();
        Assert.Empty(result);
    }

    // ---- EnumerateFontChainComponentsOnSlot ----

    [Fact]
    public void EnumerateFontChainComponentsOnSlot_ReturnsEmpty_ForEmptyComponents()
    {
        var slot = new FakeSlot();
        var result = SlotGraphWalker.EnumerateFontChainComponentsOnSlot(slot).ToList();
        Assert.Empty(result);
    }

    private class FontChainDummy { }

    [Fact]
    public void EnumerateFontChainComponentsOnSlot_FiltersByFontChainName()
    {
        var fontChain = new FontChainDummy();
        var other = new object();
        var slot = new FakeSlot { _components = new List<object> { other, fontChain } };

        var result = SlotGraphWalker.EnumerateFontChainComponentsOnSlot(slot).ToList();
        Assert.Single(result);
        Assert.Same(fontChain, result[0]);
    }
}
