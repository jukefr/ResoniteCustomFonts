using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;

#if DEBUG && RML_HOTRELOAD
using ResoniteHotReloadLib;
#endif

namespace CustomFonts;

/// <summary>Resonite mod: inspector UI uses <see cref="FrooxEngine.FontChain"/> on your user avatar — primary tag for normal text, optional second tag for bolder (GetBolderFont) via UIBuilder + stack.</summary>
public partial class CustomFonts : ResoniteMod
{
    /// <summary>Default <see cref="FontSlotTag"/> when config is empty.</summary>
    public const string DefaultFontSlotTag = "Kayt.CustomFonts";

    public override string Name => "CustomFonts";
    public override string Author => "Kayt";
    public override string Version => typeof(CustomFonts).Assembly.GetName().Version?.ToString() ?? "0.0.0";
    public override string Link => "https://example.com/CustomFonts/";

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> Active = new(
        "active",
        "Master switch — off disables the whole mod.",
        () => true);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> FontLogging = new(
        "fontLogging",
        "Verbose [CustomFonts] lines in the client log.",
        () => false);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<string> FontSlotTag = new(
        "fontSlotTag",
        "Slot.Tag under your avatar for normal UI FontChain.",
        () => DefaultFontSlotTag);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<string> BoldFontSlotTag = new(
        "boldFontSlotTag",
        "Slot.Tag under your avatar for bolder GetBolderFont chain.",
        () => DefaultFontSlotTag);

    private static readonly Harmony harmony = new Harmony("org.Kayt.CustomFonts");

    private static ModConfiguration? _config;

    /// <summary>Hot reload keeps a reference to the mod instance.</summary>
    internal static CustomFonts? Instance { get; private set; }

    public override void OnEngineInit()
    {
        Instance = this;
        _config = GetConfiguration() ?? throw new InvalidOperationException("CustomFonts: GetConfiguration() returned null.");
        _config.Save(true);
#if DEBUG && RML_HOTRELOAD
		HotReloader.RegisterForHotReload(this);
#endif
        UniLog.Log("[CustomFonts] applying Harmony patches…", false);
        harmony.PatchAll();
        // CherryPick: second Patch pass if the first PatchAll skipped the transpiler (e.g. name/signature drift); idempotent when already patched.
        CustomFontsPatches.RetryCherryPickStaticCtorTranspilerPatch(harmony);
        Msg("CustomFonts: targeted inspector creation patches + SetupEditorStyle/EnumMemberEditor style — see PATCH_TARGETS.txt.");
        // Always visible in the same stream as other engine lines (second arg = no stack trace).
        UniLog.Log("[CustomFonts] patches applied. Turn on Mod Settings → CustomFonts → Logging enabled for trace lines.", false);
    }

    internal static bool ActiveEnabled() =>
        Instance != null && _config != null && _config.GetValue(Active);

    internal static bool FontLoggingEnabled() =>
        Instance != null && _config != null && _config.GetValue(FontLogging);

    /// <summary>Exact tag to find the avatar slot that carries <see cref="FrooxEngine.FontChain"/> (whitespace trimmed; empty falls back to <see cref="DefaultFontSlotTag"/>).</summary>
    internal static string FontSourceSlotTag()
    {
        if (Instance == null || _config == null)
            return DefaultFontSlotTag;
        var v = _config.GetValue(FontSlotTag);
        return string.IsNullOrWhiteSpace(v) ? DefaultFontSlotTag : v.Trim();
    }

    /// <summary>
    /// Tag for the **bolder** FontChain under <see cref="FrooxEngine.User.Root"/>.<c>Slot</c>. Whitespace-only uses <see cref="FontSourceSlotTag"/> (track primary after the user clears this field).
    /// </summary>
    internal static string BoldFontSourceSlotTag()
    {
        if (Instance == null || _config == null)
            return FontSourceSlotTag();
        var v = _config.GetValue(BoldFontSlotTag);
        return string.IsNullOrWhiteSpace(v) ? FontSourceSlotTag() : v.Trim();
    }

    /// <summary>Trace lines when <see cref="FontLogging"/> is on; uses the same UniLog path as the rest of the client log.</summary>
    internal static void FontLog(string line)
    {
        if (!FontLoggingEnabled())
            return;
        var text = "[CustomFonts] " + line;
        UniLog.Log(text, false);
        Msg(text);
    }

#if DEBUG && RML_HOTRELOAD
	static void BeforeHotReload()
	{
		harmony.UnpatchAll(harmony.Id);
	}

	static void OnHotReload(ResoniteMod modInstance)
	{
		Instance = modInstance as CustomFonts;
		_config = Instance?.GetConfiguration();
		harmony.PatchAll();
		CustomFontsPatches.RetryCherryPickStaticCtorTranspilerPatch(harmony);
	}
#endif
}
