#include "script_component.hpp"

class CfgPatches {
    class ADDON {
        name = QUOTE(COMPONENT);
        units[] = {};
        weapons[] = {};
        requiredVersion = REQUIRED_VERSION;
        requiredAddons[] = {"grma3_main", "A3_3DEN"};
        author = "GrueArbre";
        VERSION_CONFIG;
    };
};

class ctrlMenuStrip;
class ctrlMenu;
class display3DEN
{
	class Controls
	{
		class MenuStrip: ctrlMenuStrip
		{
			class Items
			{
				class Tools
				{
					items[] += {"GRMA3_Export"};
				};
				class GRMA3_Export
				{
					text = CSTRING(ExportToGrmStudio);
					action = QUOTE([] spawn FUNC(export););
				};
			};
		};
	};
	
	class ContextMenu : ctrlMenu
    {
        class Items
        {
            items[]+={"GRMA3_Transform"};
            class GRMA3_Transform
            {
                action=QUOTE([] spawn FUNC(transform););
                text=CSTRING(CreateHiddenObjects);
                conditionShow="selectedLogic";
                value=0;
                opensNewWindow=0;
            };
        };
    };
};


class Cfg3DEN
{
	class Object
	{
		class AttributeCategories
		{
			class StateSpecial
			{
				class Attributes
				{
					class GRMA3_Exclude
					{
						displayName=CSTRING(ExcludeFromExport);
						property="GRMA3_Exclude";
						control="Checkbox";
						expression="";
						defaultValue="false";
					};
				};
			};
		};
	};
};


class CfgVehicles {
	
	class Wall_F;
	class Land_New_WiredFence_5m_F: Wall_F 
	{
		grma3_canexport = 1;
    };
    class Land_New_WiredFence_10m_F: Wall_F 
	{
		grma3_canexport = 1;
    };
	
	class TargetBootcampHumanSimple_F;
	class TargetBootcampHuman_F: TargetBootcampHumanSimple_F 
	{
		grma3_canexport = 1;
	};
	
	class All;
	class AllVehicles: All 
	{
		grma3_canexport = -1;
    };

	class Shelter_base_F;
	class CamoNet_BLUFOR_F: Shelter_base_F 
	{
		grma3_canexport = 1;
    };
};

#include "CfgEventHandlers.hpp"


