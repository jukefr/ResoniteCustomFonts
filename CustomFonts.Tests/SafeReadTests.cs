using System.Reflection;
using CustomInspectorFonts;
using Xunit;

namespace CustomInspectorFonts.Tests;

public class SafeReadTests
{
    [Fact]
    public void SafeRead_ReturnsValue_WhenReaderSucceeds()
    {
        Func<object?> reader = () => 42;
        var result = ReflectionHelpers.SafeRead(reader);
        Assert.Equal(42, result);
    }

    [Fact]
    public void SafeRead_ReturnsNull_WhenReaderThrows()
    {
        Func<object?> throwingReader = () => throw new InvalidOperationException("expected");
        var result = ReflectionHelpers.SafeRead(throwingReader);
        Assert.Null(result);
    }

    [Fact]
    public void SafeRead_ReturnsNull_WhenReaderReturnsNull()
    {
        Func<object?> nullReader = () => null;
        var result = ReflectionHelpers.SafeRead(nullReader);
        Assert.Null(result);
    }

    [Fact]
    public void SafeRead_DisposesDisposableResults()
    {
        var disposed = false;
        Func<object?> reader = () => new DisposableAction(() => disposed = true);
        var result = ReflectionHelpers.SafeRead(reader);
        Assert.NotNull(result);
    }

    private class DisposableAction : IDisposable
    {
        private readonly Action _action;
        public DisposableAction(Action action) => _action = action;
        public void Dispose() => _action();
    }
}
