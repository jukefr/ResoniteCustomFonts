using ResoniteModLoader;

namespace CustomFonts;

/// <summary>Per-area toggles in Mod Settings (only used when the mod is enabled).</summary>
public partial class CustomFonts
{
    /// <summary>When <see cref="Active"/> is true, whether this area’s styling runs (can be toggled while playing; transpiler options need a reload).</summary>
    internal static bool PatchSiteEnabled(ModConfigurationKey<bool> siteKey) =>
        ActiveEnabled() && Instance != null && _config != null && _config.GetValue(siteKey);

    // --- Inspector: main window (scene / hierarchy / shell) ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleSceneInspectorOnAttach = new(
        "styleSceneInspectorOnAttach",
        "Inspector main: first open.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleSceneInspectorOnChanges = new(
        "styleSceneInspectorOnChanges",
        "Inspector main: refresh / undo / hierarchy edits.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleSlotInspectorOnChanges = new(
        "styleSlotInspectorOnChanges",
        "Inspector main: left slot tree list.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleInspectorPanelSetup = new(
        "styleInspectorPanelSetup",
        "Inspector main: outer shell / tabs layout.",
        () => true);

    // --- Inspector: worker (detached / worker list) ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleWorkerInspectorCreate = new(
        "styleWorkerInspectorCreate",
        "Worker inspector: open panel.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleWorkerInspectorBuildUIForComponent = new(
        "styleWorkerInspectorBuildUIForComponent",
        "Worker inspector: single-component body.",
        () => true);

    // --- Inspector: users list ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleUserInspectorOnAttach = new(
        "styleUserInspectorOnAttach",
        "Users list: open floating browser.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleUserInspectorItemRebuildUser = new(
        "styleUserInspectorItemRebuildUser",
        "Users list: row rebuild.",
        () => true);

    // --- Inspector: field editors ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleSyncMemberEditorBuilder = new(
        "styleSyncMemberEditorBuilder",
        "Fields: SyncMemberEditorBuilder.Build (most fields).",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleFieldEditorSetup = new(
        "styleFieldEditorSetup",
        "Fields: FieldEditor.Setup.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleRefEditorSetup = new(
        "styleRefEditorSetup",
        "Fields: RefEditor.Setup.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleListEditorBuildListItem = new(
        "styleListEditorBuildListItem",
        "Fields: ListEditor row items.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleTextureRefEditorSetup = new(
        "styleTextureRefEditorSetup",
        "Fields: TextureRefEditor.Setup.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleDelegateEditorSetup = new(
        "styleDelegateEditorSetup",
        "Fields: DelegateEditor.Setup.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleBagEditorBuildBagItem = new(
        "styleBagEditorBuildBagItem",
        "Fields: BagEditor dictionary rows.",
        () => true);

    // --- Developer tools, dialogs, wizards (SetupEditorStyle outer scope) ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleExportDialogSetup = new(
        "styleExportDialogSetup",
        "Dev UI: Export dialog Setup.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleComponentSelectorSetupUi = new(
        "styleComponentSelectorSetupUi",
        "Dev UI: Attach Component SetupUI.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleComponentSelectorBuildUi = new(
        "styleComponentSelectorBuildUi",
        "Dev UI: Attach Component BuildUI.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleInspectorHelperSetupProxyVisual = new(
        "styleInspectorHelperSetupProxyVisual",
        "Dev UI: InspectorHelper.SetupProxyVisual.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleDevCreateNewFormOpenCategory = new(
        "styleDevCreateNewFormOpenCategory",
        "Dev UI: Create New OpenCategory.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleWizardFormOnAttach = new(
        "styleWizardFormOnAttach",
        "Dev UI: WizardForm OnAttach.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleFolderImportDialogOnAttach = new(
        "styleFolderImportDialogOnAttach",
        "Dev UI: Folder import dialog.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleRecordEditFormOpenDialogWindow = new(
        "styleRecordEditFormOpenDialogWindow",
        "Dev UI: Record edit OpenDialogWindow.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleHostAccessDialogOnAttach = new(
        "styleHostAccessDialogOnAttach",
        "Dev UI: Host access dialog.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleHyperlinkOpenDialogOnAttach = new(
        "styleHyperlinkOpenDialogOnAttach",
        "Dev UI: Hyperlink open dialog.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleBrowserCreateDirectoryDialogOnAttach = new(
        "styleBrowserCreateDirectoryDialogOnAttach",
        "Dev UI: Create directory dialog.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleNewWorldDialogOpenDialogWindow = new(
        "styleNewWorldDialogOpenDialogWindow",
        "Dev UI: New world OpenDialogWindow.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleProtoFluxNodeVisualGenerateVisual = new(
        "styleProtoFluxNodeVisualGenerateVisual",
        "Dev UI: ProtoFlux node GenerateVisual.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleAssetOptimizationWizardOnAttach = new(
        "styleAssetOptimizationWizardOnAttach",
        "Dev UI: Asset optimization wizard.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleAvatarCreatorOnAttach = new(
        "styleAvatarCreatorOnAttach",
        "Dev UI: Avatar creator wizard.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleCubemapCreatorOnAttach = new(
        "styleCubemapCreatorOnAttach",
        "Dev UI: Cubemap creator wizard.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleReflectionProbeWizardOnAttach = new(
        "styleReflectionProbeWizardOnAttach",
        "Dev UI: Reflection probe wizard.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleVhacdDialogOnAttach = new(
        "styleVhacdDialogOnAttach",
        "Dev UI: V-HACD dialog.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleWorldLightSourcesWizardOnAttach = new(
        "styleWorldLightSourcesWizardOnAttach",
        "Dev UI: World light sources wizard.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleWorldTextRendererWizardOnAttach = new(
        "styleWorldTextRendererWizardOnAttach",
        "Dev UI: World text renderer wizard.",
        () => true);

    // --- Global font pipeline ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleRadiantUiSetupEditorStyle = new(
        "styleRadiantUiSetupEditorStyle",
        "Global: RadiantUI SetupEditorStyle postfix.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleTextRenderHelperGetBolderFont = new(
        "styleTextRenderHelperGetBolderFont",
        "Global: TextRenderHelper.GetBolderFont postfix.",
        () => true);

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleEnumMemberEditorBuildUI = new(
        "styleEnumMemberEditorBuildUI",
        "Global: EnumMemberEditor.BuildUI postfix.",
        () => true);

    // --- Other mods ---

    [AutoRegisterConfigKey]
    internal static readonly ModConfigurationKey<bool> StyleCherryPickCherryPickerCctorFix = new(
        "styleCherryPickCherryPickerCctorFix",
        "CherryPick: Attach Component static-ctor transpiler.",
        () => true);
}
