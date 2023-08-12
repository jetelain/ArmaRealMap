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
					picture=QPATHTOF(data\icon.paa);
				};
			};
		};
	};
	
	class ContextMenu : ctrlMenu
    {
        class Items
        {
            items[]+={"GRMA3_CreateHiddenObjects", "GRMA3_EditObject"};
            class GRMA3_CreateHiddenObjects
            {
                action=QUOTE([] spawn FUNC(recreateHidden););
                text=CSTRING(CreateHiddenObjects);
				picture=QPATHTOF(data\icon.paa);
                conditionShow="selectedLogic";
                value=0;
                opensNewWindow=0;
            };
			class GRMA3_EditObject
			{
                action=QUOTE([] spawn FUNC(editObject););
                text=CSTRING(EditObject);
				picture=QPATHTOF(data\icon.paa);
				conditionShow="1";
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


