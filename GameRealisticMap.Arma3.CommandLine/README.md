# GameRealisticMap.Arma3.CommandLine — Arma 3 CLI

Command-line interface for driving the Arma 3 generation pipeline without the Studio GUI.

---

## Verbs

| Verb | Class | Description |
|------|-------|-------------|
| `genmod` | `GenerateModOptions` | Full generation: OSM download → builder pipeline → WRP compilation → PBO packaging |
| `genwrp` | `GenerateWrpOptions` | Generate the `.wrp` terrain file only (no PBO packaging) |
| `gentb` | `GenerateTerrainBuilderOptions` | Export TerrainBuilder-compatible CSV files for all object layers |
| `layer` | `GenerateObjectLayerOptions` | Export a single named object layer as a TerrainBuilder CSV |

---

## Common Options

All verbs share `MapOptionsBase` which configures:
- Path to the `Arma3MapConfig` JSON file
- Path to the region asset config JSON file
- Source cache directories (SRTM tiles, Sentinel-2 tiles)
- P:\ project drive path

---

## Usage Examples

```shell
# Full mod generation
grma3 genmod --config mymap.json --assets europe.json

# WRP only (skip PBO)
grma3 genwrp --config mymap.json --assets europe.json

# TerrainBuilder CSVs for all layers
grma3 gentb --config mymap.json --assets europe.json --target ./tb_output

# Single layer export
grma3 layer --config mymap.json --assets europe.json --layer ForestGenerator --target ./tb_output
```

---

## See Also

- [`GameRealisticMap.Arma3/README.md`](../GameRealisticMap.Arma3/README.md) — library used by this CLI
- [`GameRealisticMap.Studio/README.md`](../GameRealisticMap.Studio/README.md) — GUI alternative
