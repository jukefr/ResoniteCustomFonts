using System.Reflection;
using CustomFonts;
using Xunit;

namespace CustomFonts.Tests;

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
}
