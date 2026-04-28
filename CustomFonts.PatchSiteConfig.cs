using ResoniteModLoader;

namespace CustomFonts;

/// <summary>Per-area toggles in Mod Settings (only used when the mod is enabled).</summary>
public partial class CustomFonts
{
    /// <summary>When <see cref="Active"/> is true, whether this area's styling runs (can be toggled while playing; transpiler options need a reload).</summary>
    internal static bool PatchSiteEnabled(ModConfigurationKey<bool> siteKey) =>
        ActiveEnabled() && Instance != null && _config != null && _config.GetValue(siteKey);

    // --- Inspector: main window ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleSceneInspectorOnAttach = new(
        "styleSceneInspectorOnAttach",
        "Inspector: first time you open it",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleSceneInspectorOnChanges = new(
        "styleSceneInspectorOnChanges",
        "Inspector: refresh after edits",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleSlotInspectorOnChanges = new(
        "styleSlotInspectorOnChanges",
        "Slot tree in inspector",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleInspectorPanelSetup = new(
        "styleInspectorPanelSetup",
        "Inspector: outer shell/tabs",
        () => true);

    // --- Worker inspector ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleWorkerInspectorCreate = new(
        "styleWorkerInspectorCreate",
        "Component panel: opening",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleWorkerInspectorBuildUIForComponent = new(
        "styleWorkerInspectorBuildUIForComponent",
        "Component panel: body",
        () => true);

    // --- Users list ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleUserInspectorOnAttach = new(
        "styleUserInspectorOnAttach",
        "Users list: opening",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleUserInspectorItemRebuildUser = new(
        "styleUserInspectorItemRebuildUser",
        "Users list: rows",
        () => true);

    // --- Field editors ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleSyncMemberEditorBuilder = new(
        "styleSyncMemberEditorBuilder",
        "Most typed fields",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleFieldEditorSetup = new(
        "styleFieldEditorSetup",
        "Generic/boxed fields",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleRefEditorSetup = new(
        "styleRefEditorSetup",
        "Reference fields",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleListEditorBuildListItem = new(
        "styleListEditorBuildListItem",
        "List/array rows",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleTextureRefEditorSetup = new(
        "styleTextureRefEditorSetup",
        "Texture reference fields",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleDelegateEditorSetup = new(
        "styleDelegateEditorSetup",
        "Delegate/event fields",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleBagEditorBuildBagItem = new(
        "styleBagEditorBuildBagItem",
        "Dictionary/bag rows",
        () => true);

    // --- Developer tools and dialogs ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleExportDialogSetup = new(
        "styleExportDialogSetup",
        "Export dialog",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleComponentSelectorSetupUi = new(
        "styleComponentSelectorSetupUi",
        "Attach Component: setup",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleComponentSelectorBuildUi = new(
        "styleComponentSelectorBuildUi",
        "Attach Component: list",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleInspectorHelperSetupProxyVisual = new(
        "styleInspectorHelperSetupProxyVisual",
        "Proxy visuals in inspector",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleDevCreateNewFormOpenCategory = new(
        "styleDevCreateNewFormOpenCategory",
        "Create New menu",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleWizardFormOnAttach = new(
        "styleWizardFormOnAttach",
        "Wizard dialogs",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleFolderImportDialogOnAttach = new(
        "styleFolderImportDialogOnAttach",
        "Folder import dialog",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleRecordEditFormOpenDialogWindow = new(
        "styleRecordEditFormOpenDialogWindow",
        "Record edit dialog",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleHostAccessDialogOnAttach = new(
        "styleHostAccessDialogOnAttach",
        "Host access dialog",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleHyperlinkOpenDialogOnAttach = new(
        "styleHyperlinkOpenDialogOnAttach",
        "Hyperlink dialog",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleBrowserCreateDirectoryDialogOnAttach = new(
        "styleBrowserCreateDirectoryDialogOnAttach",
        "Create directory dialog",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleNewWorldDialogOpenDialogWindow = new(
        "styleNewWorldDialogOpenDialogWindow",
        "New world dialog",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleProtoFluxNodeVisualGenerateVisual = new(
        "styleProtoFluxNodeVisualGenerateVisual",
        "ProtoFlux node visuals",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleAssetOptimizationWizardOnAttach = new(
        "styleAssetOptimizationWizardOnAttach",
        "Asset optimization wizard",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleAvatarCreatorOnAttach = new(
        "styleAvatarCreatorOnAttach",
        "Avatar creator wizard",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleCubemapCreatorOnAttach = new(
        "styleCubemapCreatorOnAttach",
        "Cubemap creator wizard",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleReflectionProbeWizardOnAttach = new(
        "styleReflectionProbeWizardOnAttach",
        "Reflection probe wizard",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleVhacdDialogOnAttach = new(
        "styleVhacdDialogOnAttach",
        "V-HACD dialog",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleWorldLightSourcesWizardOnAttach = new(
        "styleWorldLightSourcesWizardOnAttach",
        "Light sources wizard",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleWorldTextRendererWizardOnAttach = new(
        "styleWorldTextRendererWizardOnAttach",
        "Text renderer wizard",
        () => true);

    // --- Global font pipeline ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleRadiantUiSetupEditorStyle = new(
        "styleRadiantUiSetupEditorStyle",
        "Global: main UI styling pass",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleTextRenderHelperGetBolderFont = new(
        "styleTextRenderHelperGetBolderFont",
        "Global: bold font swapping",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleEnumMemberEditorBuildUI = new(
        "styleEnumMemberEditorBuildUI",
        "Global: enum dropdowns",
        () => true);

    // --- Other mods ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleCherryPickCherryPickerCctorFix = new(
        "styleCherryPickCherryPickerCctorFix",
        "CherryPick Attach Component fix",
        () => true);
}
