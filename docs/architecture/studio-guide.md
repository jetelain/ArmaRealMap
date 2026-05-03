# Game Realistic Map Studio

## Overview

Game Realistic Map Studio is a WPF desktop application built on the [Gemini](https://github.com/tgjones/gemini) IDE framework. It provides a guided workflow for designing, configuring, and generating Arma 3 maps without requiring command-line usage.

The application uses a module-based architecture — each major concern is a separate Gemini module registered at startup in `StudioAppBootstrapper`.

---

## Modules

### Main (`Modules/Main/`)
Central document management, application shell, menu/toolbar integration. Manages the open map project and coordinates between other modules.

### MapConfigEditor (`Modules/MapConfigEditor/`)
Visual editor for `Arma3MapConfigJson` / `GenericMapConfigJson`:
- Map center or south-west corner (with interactive map picker)
- Grid cell size and grid size (terrain resolution and total area)
- World name, PBO prefix, target mod directory
- Satellite imagery options (resolution, fake-sat blend)
- Asset configuration file selection

### GenericMapConfigEditor (`Modules/GenericMapConfigEditor/`)
Equivalent config editor for generic (non-Arma3) map generation profiles.

### Arma3Data (`Modules/Arma3Data/`)
Loads and caches Arma 3 game data from a configured game installation:
- Scans P:\ drive for available P3D models
- Loads material/texture library
- Makes data available to AssetBrowser and BuildingGenerator

### AssetBrowser (`Modules/AssetBrowser/`)
Browses the loaded Arma 3 P3D model library. Allows searching by path and previewing model metadata.

### AssetConfigEditor (`Modules/AssetConfigEditor/`)
Edits region asset configuration files (`IArma3RegionAssets` JSON):
- Building definitions (model assignments per category/size)
- Forest/scrub cluster collections
- Material library (terrain surface definitions)
- Fence, row, railway definitions

### CompositionTool (`Modules/CompositionTool/`)
Builds `Composition` objects — multi-component object assemblies composed of multiple P3D models with relative position/rotation offsets. Used for complex buildings and structures.

### DensityConfigEditor (`Modules/DensityConfigEditor/`)
Adjusts object placement density for forest/scrub/etc. generators. Controls how many trees/shrubs are placed per square metre and their random distribution parameters.

### ConditionTool (`Modules/ConditionTool/`)
Visual editor for tag-filter conditions. Conditions control whether an OSM feature is included or excluded, using a DSL defined in `TagFilterLanguage`. Used for fine-grained control over which OSM features are rendered and how.

### Arma3WorldEditor (`Modules/Arma3WorldEditor/`)
Live 3D-like preview of the terrain being generated. Shows object placement, elevation, and material coverage. Allows interactive editing of the generated WRP — moving/replacing objects, adjusting materials — without a full regeneration.

### Explorer (`Modules/Explorer/`)
File system browser panel for navigating workspace files (configs, generated outputs, asset libraries).

### Reporting (`Modules/Reporting/`)
Progress and error reporting panel. Displays the `IProgressScope` tree as generation runs, with timing information and any errors/warnings.

---

## Application Bootstrap

`StudioAppBootstrapper` (inheriting Gemini's `AppBootstrapper`) registers all modules via MEF (Managed Extensibility Framework). Each module exposes its views, view-models, and commands through MEF exports.

---

## Undo/Redo

`UndoRedo/` contains a generic undo/redo stack used by the world editor and config editors. Operations are represented as `IUndoableAction` pairs.

---

## Shared Utilities

`Shared/` contains common infrastructure shared across modules:
- Base view-model classes
- Converters and behaviours
- Helper services (file I/O, dialogs)

`Toolkit/` contains reusable UI controls specific to the Studio (map picker, polygon editor, density visualiser, etc.).
