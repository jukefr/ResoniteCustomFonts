using CustomFonts;
using Xunit;

namespace CustomFonts.Tests;

public class FontResolverTests
{
    // ---- Helper types that mirror FrooxEngine's reflection-accessible shape ----

    private class FakeSlot
    {
        public string Tag { get; set; } = "";
        public object? World { get; set; }
        public object? Parent { get; set; }
        public int ChildrenCount { get; set; }

        // Simulates Slot's int indexer for child access
        public object? this[int i]
        {
            get
            {
                var children = Children;
                return i >= 0 && i < children.Count ? children[i] : null;
            }
        }

        public List<FakeSlot> Children { get; set; } = new();
        public List<object?> _components { get; set; } = new();
    }

    private class FakeWorld
    {
        public object? LocalUser { get; set; }
        public object? LocalUserSpace { get; set; }
    }

    private class FakeUser
    {
        public object? Root { get; set; }
    }

    private class FakeRoot
    {
        public object? Slot { get; set; }
    }

    private class FakeComponent
    {
        public object? Slot { get; set; }
        public object? World { get; set; }
        public object? LocalUser { get; set; }
    }

    private class FakeInspectorPanel
    {
#pragma warning disable CS0649
        public object? _hierarchyContentRoot;
        public object? _componentsContentRoot;
#pragma warning restore CS0649
    }

    // ---- Null guards ----

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

    // ---- Reflection-based logic tests ----

    [Fact]
    public void ResolveLocalUserAvatarRootSlot_FollowsObjectChain()
    {
        var avatarSlot = new object();
        var root = new FakeRoot { Slot = avatarSlot };
        var user = new FakeUser { Root = root };
        var world = new FakeWorld { LocalUser = user };

        var result = FontResolver.ResolveLocalUserAvatarRootSlot(world);

        Assert.Same(avatarSlot, result);
    }

    [Fact]
    public void ResolveLocalUserAvatarRootSlot_ReturnsNull_WhenLocalUserIsNull()
    {
        var world = new FakeWorld { LocalUser = null };
        var result = FontResolver.ResolveLocalUserAvatarRootSlot(world);
        Assert.Null(result);
    }

    [Fact]
    public void ResolveLocalUserAvatarRootSlot_ReturnsNull_WhenRootIsNull()
    {
        var user = new FakeUser { Root = null };
        var world = new FakeWorld { LocalUser = user };
        var result = FontResolver.ResolveLocalUserAvatarRootSlot(world);
        Assert.Null(result);
    }

    [Fact]
    public void ResolveLocalUserAvatarRootSlot_ReturnsNull_WhenSlotIsNull()
    {
        var root = new FakeRoot { Slot = null };
        var user = new FakeUser { Root = root };
        var world = new FakeWorld { LocalUser = user };
        var result = FontResolver.ResolveLocalUserAvatarRootSlot(world);
        Assert.Null(result);
    }

    [Fact]
    public void SlotIsUnderWorldLocalUserSpace_ReturnsTrue_WhenSlotIsLocalUserSpace()
    {
        var world = new FakeWorld { LocalUserSpace = new FakeSlot() };
        var lus = world.LocalUserSpace;
        // Make the LocalUserSpace aware of its world
        ((FakeSlot)lus!).World = world;

        var result = FontResolver.SlotIsUnderWorldLocalUserSpace(lus);
        Assert.True(result);
    }

    [Fact]
    public void SlotIsUnderWorldLocalUserSpace_ReturnsTrue_WhenSlotIsChildOfLocalUserSpace()
    {
        var lus = new object();
        var world = new FakeWorld { LocalUserSpace = lus };
        var slot = new FakeSlot { Parent = lus, World = world };

        var result = FontResolver.SlotIsUnderWorldLocalUserSpace(slot);
        Assert.True(result);
    }

    [Fact]
    public void SlotIsUnderWorldLocalUserSpace_ReturnsFalse_WhenNoLocalUserSpace()
    {
        var world = new FakeWorld { LocalUserSpace = null };
        var slot = new FakeSlot { World = world };

        var result = FontResolver.SlotIsUnderWorldLocalUserSpace(slot);
        Assert.False(result);
    }

    [Fact]
    public void SlotOrAncestorsUnderLocalUser_SearchesParentChain()
    {
        // FakeSlot has no IsUnderLocalUser method, so it should return false
        var slot = new FakeSlot { Parent = new FakeSlot() };
        var result = FontResolver.SlotOrAncestorsUnderLocalUser(slot);
        Assert.False(result);
    }

    [Fact]
    public void WorkerBelongsToThisClient_ReturnsTrue_WhenLocalUserMatches()
    {
        var localUser = new object();
        var world = new FakeWorld { LocalUser = localUser };
        var worker = new FakeComponent
        {
            World = world,
            LocalUser = localUser,
            Slot = new object()
        };

        var result = FontResolver.WorkerBelongsToThisClient(worker);
        Assert.True(result);
    }

    [Fact]
    public void WorkerBelongsToThisClient_ReturnsFalse_WhenLocalUserDiffers()
    {
        var world = new FakeWorld { LocalUser = new object() };
        var worker = new FakeComponent
        {
            World = world,
            LocalUser = new object(), // different user
            Slot = new object()
        };

        var result = FontResolver.WorkerBelongsToThisClient(worker);
        Assert.False(result);
    }

    [Fact]
    public void WorkerBelongsToThisClient_ReturnsFalse_WhenWorldHasNoLocalUser()
    {
        var worker = new FakeComponent
        {
            World = new FakeWorld { LocalUser = null },
            LocalUser = new object(),
            Slot = new object()
        };
        var result = FontResolver.WorkerBelongsToThisClient(worker);
        Assert.False(result);
    }

    [Fact]
    public void FindInspectorPanelFromSlot_FindsPanelInParent()
    {
        var panel = new FakeInspectorPanel
        {
            _hierarchyContentRoot = new object(),
            _componentsContentRoot = new object()
        };
        var parent = new FakeSlot();
        parent._components.Add(panel);
        var slot = new FakeSlot { Parent = parent };

        var result = FontResolver.FindInspectorPanelFromSlot(slot);
        Assert.Same(panel, result);
    }

    [Fact]
    public void FindInspectorPanelFromSlot_ReturnsNull_WhenNoPanelFound()
    {
        var slot = new FakeSlot { Parent = new FakeSlot() };
        var result = FontResolver.FindInspectorPanelFromSlot(slot);
        Assert.Null(result);
    }

    [Fact]
    public void FindInspectorPanelFromSlot_ReturnsNull_WhenComponentMissingField()
    {
        var comp = new object(); // no _hierarchyContentRoot/_componentsContentRoot fields
        var parent = new FakeSlot();
        parent._components.Add(comp);
        var slot = new FakeSlot { Parent = parent };

        var result = FontResolver.FindInspectorPanelFromSlot(slot);
        Assert.Null(result);
    }

    [Fact]
    public void ShouldApplyCustomFontsForUiSlot_ReturnsFalse_ForSimpleSlot()
    {
        var slot = new FakeSlot();
        var result = FontResolver.ShouldApplyCustomFontsForUiSlot(slot);
        Assert.False(result);
    }

    [Fact]
    public void ShouldApplyCustomFontsForUiSlot_ReturnsTrue_WhenSlotUnderLocalUserSpace()
    {
        var lus = new object();
        var world = new FakeWorld { LocalUserSpace = lus };
        var slot = new FakeSlot { Parent = lus, World = world };

        var result = FontResolver.ShouldApplyCustomFontsForUiSlot(slot);
        Assert.True(result);
    }

    [Fact]
    public void SlotOrAncestorsUnderLocalUser_ReturnsTrue_WhenSlotHasIsUnderLocalUserProperty()
    {
        // Slot.IsUnderLocalUser is a property (not a method) — verify our
        // TrySlotIsUnderLocalUser reads it via ReadMemberValue correctly
        var slot = new SlotWithIsUnderLocalUser { IsUnderLocalUser = true };
        var result = FontResolver.SlotOrAncestorsUnderLocalUser(slot);
        Assert.True(result);
    }

    [Fact]
    public void SlotOrAncestorsUnderLocalUser_ReturnsFalse_WhenIsUnderLocalUserIsFalse()
    {
        var slot = new SlotWithIsUnderLocalUser { IsUnderLocalUser = false };
        var result = FontResolver.SlotOrAncestorsUnderLocalUser(slot);
        Assert.False(result);
    }

    [Fact]
    public void SlotOrAncestorsUnderLocalUser_ChecksParentChain()
    {
        var parent = new SlotWithIsUnderLocalUser { IsUnderLocalUser = true };
        var child = new SlotWithIsUnderLocalUser { IsUnderLocalUser = false };
        // Link via Parent property (reflection-read)
        typeof(SlotWithIsUnderLocalUser)
            .GetProperty("Parent")!
            .SetValue(child, parent);

        var result = FontResolver.SlotOrAncestorsUnderLocalUser(child);
        Assert.True(result);
    }

    /// <summary>Fake type whose IsUnderLocalUser is a property (like the real FrooxEngine.Slot).</summary>
    private class SlotWithIsUnderLocalUser
    {
        public bool IsUnderLocalUser { get; set; }
        public object? Parent { get; set; }
        public object? World { get; set; }
    }
}
