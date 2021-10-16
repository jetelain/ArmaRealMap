class CfgPatches
{
	class arm_common
	{
		units[] = {};
		weapons[] = {};
		requiredVersion = 1;
		requiredAddons[] = {"A3_Plants_F"};
	};
};

#include "CfgSurfaces.hpp"


class CfgEditorSubcategories
{
    class arm_tree_faction
    {
        displayName="Arbres ARM";
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

	class Land_arm_acacia_acacia_01 : arm_bush_base
	{
		scope=2;
        scopeCurator=2;
        model="z\arm\addons\common\data\trees\sahel\acacia_1.p3d";
        displayName="Acacia #1";

	};
	
};