# CustomFonts

Change the font used in Resonite's inspector panels and developer UI to whatever you want.

Instead of the default Radiant UI font, you can use **any FontChain component** on your avatar. Set up one slot for regular text and another for bold text — the mod handles the rest.

## Quick start

1. **Install** — drop `CustomFonts.dll` into `Resonite/rml_mods/`
2. **In-game** — open Mod Settings → CustomFonts and turn it on (it's on by default)
3. **On your avatar** — create a slot under your user root with `Slot.Tag` set to `Kayt.CustomFonts`, put a `FontChain` on it, and wire your fonts
4. **Done** — open any inspector, it should now use your font

## Avatar setup (step by step)

1. In your world, open your avatar's hierarchy and find `World.LocalUser.Root.Slot` (it's the top of your avatar)
2. Create a new slot somewhere under it (doesn't matter where)
3. Set that slot's **Tag** to `Kayt.CustomFonts` (or whatever you set in config)
4. Add a **FontChain** component to that slot
5. Wire your fonts into the FontChain like you normally would

That's it. Open any inspector panel — it should now use your font.

### Optional: separate bold font

If you want a different font for bold text (headers, labels, etc.):

1. Create a **second slot** (same as above)
2. Set its **Tag** to something different, like `Kayt.CustomFontsBold`
3. Add a FontChain with your bold font
4. In Mod Settings, set `boldFontSlotTag` to `Kayt.CustomFontsBold`

If you don't set a separate bold tag, the mod uses the same font for both regular and bold text.

## Mod Settings

Open ResoniteModLoader's Mod Settings (usually in your main menu) and find CustomFonts.

### General

| Setting | What it does |
|---------|-------------|
| **active** | Master on/off switch. Turn off to disable the whole mod without removing it. |
| **fontLogging** | Prints extra info to your log file. Only turn this on if you're troubleshooting. |
| **fontSlotTag** | The `Slot.Tag` the mod looks for on your avatar to find your regular FontChain. Default: `Kayt.CustomFonts` |
| **boldFontSlotTag** | The `Slot.Tag` for your bold FontChain. Leave blank to use the same font for both. |

### What gets custom fonts

Each of these toggles controls whether a specific part of the UI gets your font. They're all ON by default. Turn something off if it causes issues with another mod.

| Toggle | Controls font in... |
|--------|-------------------|
| **Inspector main window** | The big inspector panel (scene hierarchy, slot list) |
| **Worker inspector** | Component panels, detached inspector windows |
| **Users list** | The floating user list |
| **Field editors** | Number fields, text fields, reference pickers, lists, etc. |
| **Developer tools** | Export dialog, attach component, wizards, ProtoFlux node visuals, etc. |
| **Global font pipeline** | The main styling system and bold font handling |
| **CherryPick fix** | Fixes a crash in the CherryPick mod's Attach Component window |

Most people can leave everything on. The per-area toggles exist in case another mod conflicts with a specific area.

## How it works (short version)

When Resonite builds an inspector panel, it uses Radiant UI fonts. This mod intercepts that process and swaps in your FontChain instead. It walks your avatar hierarchy looking for a slot with the right tag, finds the FontChain on it, and applies it to the inspector.

The mod only affects your own inspector — other people's UIs are untouched.

## Requirements

- Resonite (any recent version)
- [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) 5.x

## Installing

Download `CustomFonts.dll` from [Releases](https://github.com/jukefr/ResoniteCustomFonts/releases) and put it in `Resonite/rml_mods/`. That's it.

## Limitations

**TexturePackingWizard** and **TextureUnpackingWizard** don't get custom bold fonts — their UI is built asynchronously and the mod can't wrap it. The regular font still applies through the global styling system.

## License

GNU GPL-3.0 — see [LICENSE](LICENSE) for details.
