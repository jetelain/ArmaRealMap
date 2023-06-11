[Arma Realistic Map Guide](guide.md)

# Create Map Configuration

## Coordinates and technical parameters

You can use https://arm.pmad.net/Maps/SelectLocation to easily compute map parameters

You have to determine :
- Coordinates of origin point / South West coordinates of map
- Arma Grid Size : size of elevation grid, it can be 2048, 4096 or 8192
- Arma Cell Size : resolution in meters of elevation, 10 meters is the maximum recommanded size
- Resolution of imagery : number of meters per pixels of the map for satellite view and ground textures. 
  - 1 m/px is the best, 
  - 2 m/px is better for very large maps (>40Km) to limit memory requirements for both generation and game engine.

## Name and region

Then you have to choose:
- A technical name of the map : alphanumeric string with underscore.
- Region for assets : Sahel, CentralEurope, Tropical, Mediterranean, or NearEast.

Please note: Only "Sahel" region is almost complete. Other regions are still in progress.

## Configuration file

Once you have computed and choosed all parameters, create a JSON file with parameters :

```json
{
  "GridSize": 2048 or 4096 or 8192,
  "CellSize": from 2.5 to 10,
  "Resolution": 1 or 2 (intermediate values may works),
  "BottomLeft": "MGRS coordinates of South West Point",
  "Terrain": "Region for assets",
  "WorldName": "Technical Name of Map",
  "PboPrefix": "z\\arm\\addons\\Technical Name of Map"
}
```