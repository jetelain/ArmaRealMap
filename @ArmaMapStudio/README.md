# @ArmaMapStudio — Arma 3 Mod

**@ArmaMapStudio** is the in-game companion mod for Game Realistic Map Studio. It integrates with Arma 3's Eden editor to provide two editing workflows:

1. **Export to GRM Studio** — export objects placed in Eden back to Studio for integration into the terrain
2. **In-game hidden-object editing** — manage hidden terrain objects directly from within Arma 3

---

## Addons

| Addon | Description |
|-------|-------------|
| `grma3_main` | Core mod infrastructure (macros, versioning) |
| `grma3_eden` | Eden editor integration: export function, hidden-object tools, context menu items |
| `grma3_report` | In-game progress/status reporting UI |

---

## Eden Editor Integration (`grma3_eden`)

### Export to GRM Studio

A **"Export to GRM Studio"** entry is added to the Eden editor's *Tools* menu. Running the export:

1. Iterates all 3DEN entities in the current scene
2. Collects placed objects with their model path, WRP-space position, orientation (vectorUp/vectorDir), scale, and object ID
3. Serialises hidden-terrain-object modules (`ModuleHideTerrainObjects_F`, `ModuleEditTerrainObject_F`)
4. Writes the data to a format that GRM Studio can import to update the terrain's WRP

The output includes a prelude with the world name, world size, and GRM revision number so Studio can validate compatibility.

### Hidden Object Editing

Two context menu items are added to 3DEN objects:

| Item | Function | Description |
|------|----------|-------------|
| *Create Hidden Objects* | `fnc_recreateHidden` | Re-creates hidden terrain objects as visible Eden entities for editing |
| *Edit Object* | `fnc_editObject` | Opens an edit dialog for a selected terrain object |

`fnc_transformObject` handles position/orientation recalculation when objects are moved within Eden.

---

## Typical Workflow

```
[GRM Studio] Generate terrain (WRP/PBO)
      ↓  load mod in Arma 3
[Arma 3 Eden] Place / adjust objects
      ↓  Tools → Export to GRM Studio
[GRM Studio] Import Eden export → apply as WRP patch
      ↓  repack PBO
[Arma 3] Updated terrain
```

---

## Build

The mod is built with [HEMTT](https://hemtt.dev/). Run `hemtt.exe build` in the `@ArmaMapStudio/` directory, or use `build-debug.ps1` for a development build.

---

## See Also

- [`GameRealisticMap.Studio/README.md`](../GameRealisticMap.Studio/README.md) — the Studio application that consumes the export
- [`GameRealisticMap.Arma3/README.md`](../GameRealisticMap.Arma3/README.md) — WRP edit tools used to apply the import
