# CustomFonts (Resonite)

Use custom FontChain components on your user avatar for the inspector UI instead of the default Radiant UI fonts. Supports separate regular and bold font faces, with per-area config toggles.

**Repository**: [https://github.com/Kayt/ResoniteCustomFonts](https://github.com/Kayt/ResoniteCustomFonts)

## Usage (avatar setup)

1. Under **`World.LocalUser.Root.Slot`** (your user root / avatar hierarchy), create one or two slots whose **`Slot.Tag`** string matches the mod config (see **General** below).
2. On each tagged slot, add a **`FontChain`** (and wire fonts as you normally would in Resonite).
3. **Primary** tag (`fontSlotTag`, default `Kayt.CustomFonts`): used for normal inspector **`UIBuilder.Style.Font`** after `SetupEditorStyle` and similar paths.
4. **Bold** tag (`boldFontSlotTag`, default same as primary): used when the game asks for **`GetBolderFont`** / bolder stack scopes (e.g. the line in `RadiantUI_Constants.SetupEditorStyle` that sets style font from `GetBolderFont`). Use a **second** tag (e.g. `Kayt.CustomFontsBold`) and a second slot + **FontChain** if you want a different face for "bolder" inspector text.
5. If `boldFontSlotTag` is **blank** in config, it follows **`fontSlotTag`** (same chain for both). Whitespace-only is treated as "follow primary".

The mod does **not** pull FontChain from the inspector hierarchy; resolution is **tag + local user avatar** only. Harmony limits **where** fonts apply (inspector panels, worker inspector, component selector, `LocalUserSpace`, etc.); see code `ShouldApplyCustomFontsForUiSlot`.

## Configuration (Mod Settings)

In-game labels are **short** on purpose (Resonite truncates long descriptions). This section is the full reference.

### General

| Key | Meaning |
|-----|--------|
| **active** | Master switch. When off, no patches / font logic run. |
| **fontLogging** | When on, emits extra `[CustomFonts]` lines to the client log (verbose; useful once, then turn off). |
| **fontSlotTag** | Exact **`Slot.Tag`** string to search for under your user root. First matching descendant with a **FontChain** wins for **normal** UI font. Default `Kayt.CustomFonts`. Empty/whitespace in config falls back to that default. |
| **boldFontSlotTag** | Same idea for **bolder** font resolution. Default in code matches primary; set e.g. `Kayt.CustomFontsBold` for a second slot + FontChain. Blank = use whatever `fontSlotTag` resolves to. |

### Inspector — main window

Harmony **bolder stack** entry points for the big inspector window (scene / hierarchy / shell).

| Key | Patched area |
|-----|----------------|
| **styleSceneInspectorOnAttach** | First time you open inspect / scene inspector attach. |
| **styleSceneInspectorOnChanges** | Refreshes from edits, undo, hierarchy changes, etc. |
| **styleSlotInspectorOnChanges** | Left slot tree / hierarchy list. |
| **styleInspectorPanelSetup** | `InspectorPanel.Setup` — outer shell, tabs, shared layout. |

### Worker inspector

| Key | Patched area |
|-----|----------------|
| **styleWorkerInspectorCreate** | Static `WorkerInspector.Create` — opening the worker / component-list panel. |
| **styleWorkerInspectorBuildUIForComponent** | Per-component inspector body. |

### Users list

| Key | Patched area |
|-----|----------------|
| **styleUserInspectorOnAttach** | Session users floating list — open. |
| **styleUserInspectorItemRebuildUser** | Rows when the list rebuilds. |

### Field editors

| Key | Patched area |
|-----|----------------|
| **styleSyncMemberEditorBuilder** | `SyncMemberEditorBuilder.Build` — most typed fields; Attach Component field UI uses this too. |
| **styleFieldEditorSetup** | Generic / boxed field rows. |
| **styleRefEditorSetup** | Reference fields. |
| **styleListEditorBuildListItem** | List rows. |
| **styleTextureRefEditorSetup** | Texture reference fields. |
| **styleDelegateEditorSetup** | Delegate fields. |
| **styleBagEditorBuildBagItem** | Dictionary / bag rows. |

### Developer tools and dialogs

Targeted **Prefix/Postfix** around `SetupEditorStyle` (and related) for exporter, attach component, wizards, security dialogs, ProtoFlux node visuals, etc. Each key gates one patch class in `Patches/Targeted/`. Turn off a line if you need to narrow down a conflict with another mod.

| Key | Patched area (short) |
|-----|------------------------|
| **styleExportDialogSetup** | `ExportDialog.Setup` |
| **styleComponentSelectorSetupUi** | `ComponentSelector.SetupUI` |
| **styleComponentSelectorBuildUi** | `ComponentSelector.BuildUI` |
| **styleInspectorHelperSetupProxyVisual** | `InspectorHelper.SetupProxyVisual` (static) |
| **styleDevCreateNewFormOpenCategory** | `DevCreateNewForm.OpenCategory` |
| **styleWizardFormOnAttach** | `WizardForm.OnAttach` |
| **styleFolderImportDialogOnAttach** | `FolderImportDialog.OnAttach` |
| **styleRecordEditFormOpenDialogWindow** | `RecordEditForm.OpenDialogWindow` (static) |
| **styleHostAccessDialogOnAttach** | `HostAccessDialog.OnAttach` |
| **styleHyperlinkOpenDialogOnAttach** | `HyperlinkOpenDialog.OnAttach` |
| **styleBrowserCreateDirectoryDialogOnAttach** | `BrowserCreateDirectoryDialog.OnAttach` |
| **styleNewWorldDialogOpenDialogWindow** | `NewWorldDialog.OpenDialogWindow` (static) |
| **styleProtoFluxNodeVisualGenerateVisual** | `ProtoFluxNodeVisual.GenerateVisual` |
| **styleAssetOptimizationWizardOnAttach** | `AssetOptimizationWizard.OnAttach` |
| **styleAvatarCreatorOnAttach** | `AvatarCreator.OnAttach` |
| **styleCubemapCreatorOnAttach** | `CubemapCreator.OnAttach` |
| **styleReflectionProbeWizardOnAttach** | `ReflectionProbeWizard.OnAttach` |
| **styleVhacdDialogOnAttach** | `VHACD_Dialog.OnAttach` |
| **styleWorldLightSourcesWizardOnAttach** | `WorldLightSourcesWizard.OnAttach` |
| **styleWorldTextRendererWizardOnAttach** | `WorldTextRendererWizard.OnAttach` |

### Global font pipeline

| Key | Meaning |
|-----|--------|
| **styleRadiantUiSetupEditorStyle** | Postfix on `RadiantUI_Constants.SetupEditorStyle` — applies avatar FontChain to `UIBuilder` after engine styling (and coordinates primary vs bold slot for `Style.Font`). |
| **styleTextRenderHelperGetBolderFont** | Postfix on `TextRenderHelper.GetBolderFont` — substitutes stack font when a bolder scope is active. Should stay on with the bolder stack patches. |
| **styleEnumMemberEditorBuildUI** | Postfix on `EnumMemberEditor.BuildUI` — font pass on enum dropdown UI (no `SetupEditorStyle` inside that method). |

### Other mods

| Key | Meaning |
|-----|--------|
| **styleCherryPickCherryPickerCctorFix** | Transpiler on CherryPick's `CherryPicker` static constructor: snapshot `flatten` with `ToList()` so Attach Component does not throw "collection was modified". Needs **reload** if toggled at patch time. |

## Building

```bash
# Build (auto-builds stubs, auto-fetches NuGet deps)
dotnet build

# Build and copy to Resonite mods folder
dotnet build -t:Copy

# Run tests
dotnet test

# Build for release (embeds deps into single DLL)
dotnet build --configuration Release
```

## Building for development (hot reload)

```bash
dotnet build -p:DefineConstants="DEBUG;RML_HOTRELOAD"
```

This only does something if `ResoniteHotReloadLib.dll` is present at runtime.

## Development

The project is split into a core library (`CustomFonts.Core`) without Resonite or Harmony dependencies, making it easier to test. Tests are in `CustomFonts.Tests` using xUnit + NSubstitute.

### Project layout

```
ResoniteCustomFonts/
├── CustomFonts.Core/              # Core logic, no Resonite dependencies
│   ├── ReflectionHelpers.cs
│   ├── SlotGraphWalker.cs
│   └── FontResolver.cs
├── CustomFonts.Tests/             # xUnit tests
├── CustomFonts/                   # Main mod code
│   ├── CustomFonts.cs
│   ├── CustomFonts.PatchSiteConfig.cs
│   └── Patches/
└── [.csproj / stubs / tools / ...]
```

### Stubs

The project builds its own stub assemblies for `Elements.Core` and `Renderite.Host` so it can compile without a Resonite installation. These are rebuilt automatically before each build.

## Limitations

**TexturePackingWizard** and **TextureUnpackingWizard**: the first `SetupEditorStyle` for those UIs runs inside an **`async`** continuation **after** `OnAttach` returns. A Harmony Prefix/Postfix on `OnAttach` cannot wrap that call, so there is **no** dedicated bolder-stack patch for those two; global `SetupEditorStyle` / `SyncMemberEditorBuilder` behavior still applies where relevant.

## License

GNU GPL-3.0 - see [LICENSE](LICENSE) for details.
