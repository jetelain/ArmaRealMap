# Game Realistic Map

![](./GameRealisticMap.Studio/Resources/Icons/grms128.png)

Game Realistic Map is a toolchain to generate realistic game maps from OpenStreetMap data.

It's designed to work in an highly automated process, and to be usable by most people.

The "Game Realistic Map Studio" aims to guide you in the design steps and generation process.

Current version supports only Arma 3 as generation target.

## Documentation

[Documentation is available in the Wiki](https://github.com/jetelain/ArmaRealMap/wiki)

## Legacy Arma Realistic Map

Looking for Arma Realistic Map / Arma Map Studio ? See [legacy documentation](./docs/legacy.md).
Tools have been kept in repository, but might be broken. See `legacy` branch for last known valid version.

## Data sources

  - NASA SRTM : https://www2.jpl.nasa.gov/srtm/ (automatic)
  - OpenStreetMap (automatic)
  - Sentinel-2 cloudless - https://s2maps.eu (automatic)
  
Generated maps MUST includes following credits :
  - Elevation Model : NASA - Shuttle Radar Topography Mission (SRTM)
  - Cartography : © <a href="https://www.openstreetmap.org/copyright">OpenStreetMap Contributors</a> - released under <a href="https://opendatacommons.org/licenses/odbl/">Open Data Commons Open Database License (ODbL)</a> 
  - Satellite image : <a xmlns:dct="http://purl.org/dc/terms/" href="https://s2maps.eu" property="dct:title">Sentinel-2 cloudless - https://s2maps.eu</a> by <a xmlns:cc="http://creativecommons.org/ns#" href="https://eox.at" property="cc:attributionName" rel="cc:attributionURL">EOX IT Services GmbH</a> (Contains modified Copernicus Sentinel data 2016 &amp; 2017) released under <a rel="license" href="https://creativecommons.org/licenses/by/4.0/">Creative Commons Attribution 4.0 International License</a>
