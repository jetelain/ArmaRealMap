# Arma Real Map
Tools to generate realistic Arma3 terrain from OpenStreeMap data in an highly automated process.


## TODO List

  - [x] Generate satellite image from IGN BD Ortho (50cm/pixel -> transformed to 1m/pixel)
  - [ ] Generate elevation grid from SRTM data
    - [x] Raw eleveation grid
    - [ ] Adjust using rivers and lakes data
    - [ ] Adjust using roads data
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
    - [ ] Forest
	  - [x] Experimental using Terrain Processor
	  - [ ] Builtin placing
    - [x] Isolated trees
    - [ ] Tree rows
    - [ ] Rocks
    - [ ] Adjust using rivers and lakes data
    - [x] Adjust using roads data
    - [ ] Adjust using parkings data
    - [ ] Adjust using buildings data
  - [ ] Place roads from OpenStreeMap data
    - [x] Roads
    - [ ] Bridges
    - [ ] Sidewalk
    - [ ] Road lights
  - [ ] Place infrastrure elements  from OpenStreeMap data
    - [ ] Railroads
    - [ ] Powerlines
    - [ ] Radio towers

## Data sources

  - IGN : https://geoservices.ign.fr/documentation/diffusion/telechargement-donnees-libres.html
  - NASA SRTM : https://www2.jpl.nasa.gov/srtm/, needs account on https://urs.earthdata.nasa.gov/
  - OpenStreeMap, Get data in "osm.pbf" format, using 
    - https://protomaps.com/extracts
    - https://download.bbbike.org/osm/
    - See https://wiki.openstreetmap.org/wiki/Planet.osm for other tools

## Requirements

  - RAM : 16 GB minimal, 32 GB recommanded
  - HDD : SSD recommanded, 64 GB free disk
  
# Belfort Map

It's used to test the tool chain