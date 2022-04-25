# Target structure



- `~/.config/share/ArmaRealMap` or `%USERPROFILE%\AppData\Local\ArmaRealMap` : Global data
  - `srtm`
	- SRTM files cache
  - `s2c`
    - S2 Cloudless files cache
  - `config.json`         : Global config
  - `libraries.json`      : Terrain objects database (Cache of https://arm.pmad.net/ObjectLibraries/Export)

- Work directory
  - `mapname.json` : Map config
  - `mapname`
    - `cache`    
      - `internal` : Internal data cache, can be safely cleared
        - Misc files to make re-run faster
      - `input`               : Input data cache. Be careful, clearing data here can be very costly
  	    - `area.osm.xml`      : OpenStreeMap data for terrain coordinates
        - `sat-raw.png`       : S2 Cloudless Satellite image for terrain coordinates   (MGRS projected)
  	    - `elevation-raw.bin` : SRTM elevation data for terrain coordinates (MGRS   projected)
    - `output`
      - `terrain`           : Files for TerrainBuilder (if you need additional editing in   that tool)
  	    - `idmap`           : ID Map
  	      - `idmap.png`     : Full image
  		  - `idmap.N_N.png` : Tiles for easier import
  	    - `sat`
  	      - (nothing generated anymore, everything is generated in `layers` in current version)
  	    - `objects`
  	      - `*.abs.txt`     : Objects with absolute elevation
  	      - `*.rel.txt`     : Objects with relative elevation
		- `elevation.asc`   : Elevatio data
      - `precooked`         : Files ready for binarization with PboProject from Mikero
  	    - `data`
  	      - `layers`
  		    - ID Map segments, satellite segments, and materials for map
          - `roads`
  		    - Files for roads
  	    - `mapname.wrp`     : WRP World file
  	    - `mapinfos.hpp`    : Map coordinates and basic infos
  	    - `names.hpp`       : City names
    - `debug`    
      - `*.png` : Debugging of some stages
	  - `*.log` : Log file (one for each launch of generator)