// Stub for Elements.Core.dll — only types actually used by CustomFonts at compile time.

namespace Elements.Core;

/// <summary>Resonite's logging utility. The real class has many more members — we only stub what CustomFonts calls.</summary>
public static class UniLog
{
    public static void Log(string text, bool stackTrace) { }
}
