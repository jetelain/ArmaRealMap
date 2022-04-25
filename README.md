# Arma Map Studio (AMS)

## Workflow with Terrain Builder

Traditional terrain making with Arma

![Workflow](./docs/workflow-tb.svg)

## Workflow without Terrain Builder

Importing data in Terrain Builder can be really long, the new "PreCooked" workflow directly generates layers and WRP files.

![Workflow](./docs/workflow-precooked.svg)

Files for terrain builder are still generated and can be used a fallback.

## Eden Map Editor (@ArmaMapStudio + AmsUtil)

Edit your generated map in Arma 3 Eden Editor, like a regular mission : 
1. Add objects, hide objects to remove
2. Export with @ArmaMapStudio addon
3. Launch AmsUtil on the wrp file
4. Pack/Binarize again
5. It's done !

You can also edit map generated with other tools with the same way.

## Realistic Map Generator (ArmaRealMap)
Tools to generate realistic Arma3 terrain from OpenStreeMap data in an highly automated process.

### TODO List

  - [x] Generate satellite image from Sentinel-2 cloudless
  - [ ] Generate elevation grid from SRTM data
    - [x] Raw eleveation grid
    - [x] Adjust using rivers and lakes data
    - [x] Adjust using roads data
    - [ ] Adjust using buildings data
  - [ ] Generate ground texture details from OpenStreeMap data
    - [x] Baseline from terrain usage
    - [x] Adjust using roads data
    - [ ] Adjust using parkings data
    - [x] Adjust using buildings data
  - [ ] Place best-fit buildings from OpenStreeMap data and objects libraries
    - [x] Residential buildings
    - [ ] Commercial buildings
    - [ ] Industrial buildings
    - [ ] Military buildings
    - [ ] Historic buildings
    - [ ] Sports buildings
  - [ ] Place natural elements from OpenStreeMap data
    - [x] Forest
    - [x] Isolated trees
    - [x] Tree rows
    - [x] Rocks
    - [x] Adjust using rivers and lakes data
    - [x] Adjust using roads data
    - [ ] Adjust using parkings data
    - [ ] Adjust using buildings data
  - [ ] Place roads from OpenStreeMap data
    - [x] Roads
    - [x] Bridges
    - [ ] Sidewalk
    - [ ] Road lights
  - [ ] Place infrastrure elements  from OpenStreeMap data
    - [ ] Railroads
    - [ ] Powerlines
    - [ ] Radio towers

### Data sources

  - NASA SRTM : https://www2.jpl.nasa.gov/srtm/, needs account on https://urs.earthdata.nasa.gov/
  - OpenStreeMap (automatic)
  - Sentinel-2 cloudless - https://s2maps.eu (automatic)
  
Generated maps MUST includes following credits :
  - Elevation Model : NASA - Shuttle Radar Topography Mission (SRTM)
  - Cartography : © <a href="https://www.openstreetmap.org/copyright">OpenStreetMap Contributors</a> - released under <a href="https://opendatacommons.org/licenses/odbl/">Open Data Commons Open Database License (ODbL)</a> 
  - Satellite image : <a xmlns:dct="http://purl.org/dc/terms/" href="https://s2maps.eu" property="dct:title">Sentinel-2 cloudless - https://s2maps.eu</a> by <a xmlns:cc="http://creativecommons.org/ns#" href="https://eox.at" property="cc:attributionName" rel="cc:attributionURL">EOX IT Services GmbH</a> (Contains modified Copernicus Sentinel data 2016 &amp; 2017) released under <a rel="license" href="https://creativecommons.org/licenses/by/4.0/">Creative Commons Attribution 4.0 International License</a>

### Requirements

  - RAM : 16 GB minimal, 32 GB recommanded
  - HDD : SSD recommanded, 64 GB free disk
  
## Maps

It's used to test the tool chain

### Belfort, France

20x20 Km. Complex area with a lot of objects.

Specific tasks : 
  - [ ] Find appropriate buildings assets

### Gossi, Mali

82x82 Km. Simple but very large area.

Specific tasks :
  - [x] Improve OpenStreetMap data : https://www.openstreetmap.org/#map=11/15.5926/-0.9304
    1. Create an account
    2. Add data based on satellite image for :
      - "Forest" / vegetation areas ;
      - Wet / water areas ;
      - Roads / trails ;
      - Buildings.
  - [x] Find appropriate buildings assets
  - [x] Find appropriate ground textures
  - [x] Find appropriate vegetation assets
    - [x] Trees
    - [x] Edges
    - [x] Clutter
  