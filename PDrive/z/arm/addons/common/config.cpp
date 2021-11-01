class CfgPatches
{
	class arm_common
	{
		units[] = {};
		weapons[] = {};
		requiredVersion = 1;
		requiredAddons[] = {"A3_Plants_F", "A3_Map_Stratis"};
	};
};

#include "CfgSurfaces.hpp"

class CfgEditorSubcategories
{
    class arm_tree_faction
    {
		displayName="Arbres [ARM]";
    };
	class arm_huts
    {
        displayName="Huttes [ARM]";
    };
};

class CfgWorlds
{
	class Stratis;
	
	class arm_world: Stratis
	{
		class DefaultClutter;
		class clutter 
		{
			#include "data\gdt\Clutter.hpp"
		};
	};
	
	class arm_world_sahel : arm_world
	{
		
	};
};

class CfgVehicles
{
    class NonStrategic;
    class arm_bush_base : NonStrategic
    {
        scope=0;
        destrType="DestructTree";
        editorCategory="EdCat_Environment";
        editorSubcategory="arm_tree_faction";
    };

	class Land_arm_acacia_01 : arm_bush_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\trees\sahel\acacia_1.p3d";
        displayName="Acacia #1";
	};
	
	class arm_hut_base : NonStrategic
    {
        scope=0;
        destrType="destructbuilding";
        editorCategory="EdCat_Environment";
        editorSubcategory="arm_huts";
    };

	class Land_arm_hut01 : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut01.p3d";
        displayName="Hutte #1 (5.5m x 2m)";
	};
	class Land_arm_hut01A : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut01A.p3d";
        displayName="Hutte #1 (5.5m x 2m) - Variation A";
	};
	class Land_arm_hut01B : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut01B.p3d";
        displayName="Hutte #1 (5.5m x 2m) - Variation B";
	};
	class Land_arm_hut01C : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut01C.p3d";
        displayName="Hutte #1 (5.5m x 2m) - Variation C";
	};
	
	
	
	class Land_arm_hut02 : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut02.p3d";
        displayName="Hutte #2 (5.5m x 2m)";
	};
	class Land_arm_hut02A : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut02A.p3d";
        displayName="Hutte #2 (5.5m x 2m) - Variation A";
	};
	class Land_arm_hut02B : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut02B.p3d";
        displayName="Hutte #2 (5.5m x 2m) - Variation B";
	};
	class Land_arm_hut02C : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut02C.p3d";
        displayName="Hutte #2 (5.5m x 2m) - Variation C";
	};
	
	
	class Land_arm_hut03 : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut03.p3d";
        displayName="Hutte #3 (5.5m x 2m)";
	};
	class Land_arm_hut03A : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut03A.p3d";
        displayName="Hutte #3 (5.5m x 2m) - Variation A";
	};
	class Land_arm_hut03B : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut03B.p3d";
        displayName="Hutte #3 (5.5m x 2m) - Variation B";
	};
	class Land_arm_hut03C : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut03C.p3d";
        displayName="Hutte #3 (5.5m x 2m) - Variation C";
	};
	
	class Land_arm_hut04 : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut04.p3d";
        displayName="Hutte #4 (5.5m x 2m)";
	};
	class Land_arm_hut04A : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut04A.p3d";
        displayName="Hutte #4 (5.5m x 2m) - Variation A";
	};
	class Land_arm_hut04B : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut04B.p3d";
        displayName="Hutte #4 (5.5m x 2m) - Variation B";
	};
	class Land_arm_hut04C : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut04C.p3d";
        displayName="Hutte #4 (5.5m x 2m) - Variation C";
	};
	
	class Land_arm_hut05 : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut05.p3d";
        displayName="Hutte #5 (5.5m x 2m)";
	};
	class Land_arm_hut05A : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut05A.p3d";
        displayName="Hutte #5 (5.5m x 2m) - Variation A";
	};
	class Land_arm_hut05B : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut05B.p3d";
        displayName="Hutte #5 (5.5m x 2m) - Variation B";
	};
	class Land_arm_hut05C : arm_hut_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\houses\hut05C.p3d";
        displayName="Hutte #5 (5.5m x 2m) - Variation C";
	};
};