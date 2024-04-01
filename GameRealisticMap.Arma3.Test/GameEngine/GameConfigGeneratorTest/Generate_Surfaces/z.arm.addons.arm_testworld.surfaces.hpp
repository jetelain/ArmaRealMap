class CfgSurfaces {
class Default;

	class Gdt1 : Default
	{
        ACE_canDig=0;
		files="color1";
		character="Gdt1Clutter";
		soundEnviron="env1";
		soundHit="hit1";
		rough=1;
		maxSpeedCoef=2;
		dust=3;
		lucidity=4;
		grassCover=5;
		impact="impact1";
		surfaceFriction=6;
        maxClutterColoringCoef=7;
	};

	class Gdt2 : Default
	{
        ACE_canDig=1;
		files="color2";
		character="Gdt2Clutter";
		soundEnviron="env2";
		soundHit="hit2";
		rough=8;
		maxSpeedCoef=9;
		dust=10;
		lucidity=11;
		grassCover=12;
		impact="impact2";
		surfaceFriction=13;
        maxClutterColoringCoef=14;
	};
};
class CfgSurfaceCharacters {

    class Gdt1Clutter
	{
		probability[]={0.5,0.5};
		names[]={"C1","C2"};
	};

    class Gdt2Clutter
	{
		probability[]={0.25,0.75};
		names[]={"C2","C3"};
	};
};
