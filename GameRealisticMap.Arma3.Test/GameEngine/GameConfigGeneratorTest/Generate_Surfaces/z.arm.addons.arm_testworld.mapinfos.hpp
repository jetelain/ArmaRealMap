
latitude=-47.70452771; // positive is south
longitude=6.8535141; // positive is east 

mapArea[] = {
    47.68560000, 6.8270000, //Bottom Left => SW
	47.72344952, 6.8800475 //Top Right => NE
}; 
mapSize=4096;
mapZone=32;

centerPosition[]={2048,2048};

class OutsideTerrain
{
    colorOutside[] = {0.227451,0.27451,0.384314,1};
	enableTerrainSynth = 0;
	satellite = "z\arm\addons\arm_testworld\data\satout_ca.paa";
    class Layers
    {
		class Layer0
        {
			nopx    = "defNormal";
			texture = "defColor"; 
		};
    };
};

class Grid {
    offsetX = 0;
    offsetY = 4096;
    class Zoom1 {
        zoomMax = 0.3750;
        format = "XY";
        formatX = "000";
        formatY = "000";
        stepX = 100;
        stepY = -100;
    };
    class Zoom2 {
        zoomMax = 3.7500;
        format = "XY";
        formatX = "00";
        formatY = "00";
        stepX = 1000;
        stepY = -1000;
    };
    class Zoom3 {
        zoomMax = 1e+030;
        format = "XY";
        formatX = "0";
        formatY = "0";
        stepX = 10000;
        stepY = -10000;
    };
};
