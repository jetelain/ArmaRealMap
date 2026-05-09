# GameRealisticMap.Arma3.CommandLine — Arma 3 CLI

Command-line interface for driving the Arma 3 generation pipeline without the Studio GUI.

The command line can work on Linux/on Windows without Arma 3 installed but it will require the `modelinfo.json` file
alongside the map configuration file (`.grma3m`) from a previous run on Windows with Arma 3 and Arma 3 tools installed.

On Windows the file is located in `%LOCALAPPDATA%\GameRealisticMap\Arma3`, and is automatically generated 
by the Studio application.

The `genmod` will only work on Windows with Arma 3 and Arma 3 Tools installed.

The command line was intended to offload generation process to a Linux server, but it can be used on Windows as well for users who prefer CLI over GUI.

The Studio application will eventually be able to call the command line for generation on a remote Linux Server (through SSH).

---

## Verbs

| Verb | Class | Description |
|------|-------|-------------|
| `genmod` | `GenerateModOptions` | Full generation: OSM download → builder pipeline → WRP compilation → PBO packaging. Works only on Windows with Arma 3 and Arma 3 Tools installed |
| `genwrp` | `GenerateWrpOptions` | Generate the `.wrp` terrain file only (no PBO packaging) |
| `gentb` | `GenerateTerrainBuilderOptions` | Export TerrainBuilder-compatible CSV files for all object layers |
| `layer` | `GenerateObjectLayerOptions` | Export a single named object layer as a TerrainBuilder CSV |

---

## Common Options

All verbs share `MapOptionsBase` which configures path to the map configuration file (`.grma3m`).

---

## Usage Examples

```shell
# Full mod generation
grma3 genmod --config mymap.grma3m

# WRP only (skip PBO)
grma3 genwrp --config mymap.grma3m

# TerrainBuilder CSVs for all layers
grma3 gentb --config mymap.grma3m --target ./tb_output

# Single layer export
grma3 layer --config mymap.grma3m --layer ForestGenerator --target ./tb_output
```

---

## See Also

- [`GameRealisticMap.Arma3/README.md`](../GameRealisticMap.Arma3/README.md) — library used by this CLI
- [`GameRealisticMap.Studio/README.md`](../GameRealisticMap.Studio/README.md) — GUI alternative
