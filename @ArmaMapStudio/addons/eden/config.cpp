#include "script_component.hpp"

class CfgPatches {
    class ADDON {
        name = QUOTE(COMPONENT);
        units[] = {};
        weapons[] = {};
        requiredVersion = REQUIRED_VERSION;
        requiredAddons[] = {"ams_main", "A3_3DEN"};
        author = "AUTHOR";
        VERSION_CONFIG;
    };
};

class ctrlMenuStrip;
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
					items[] += {"AMS_Export"};
				};
				class AMS_Export
				{
					text = "Export";
					action = QUOTE([] spawn FUNC(export););
				};
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
					class AMS_Exclude
					{
						displayName="Exclude from Arma Map Studio Export";
						property="AMS_Exclude";
						control="Checkbox";
						expression="";
						defaultValue="false";
					};
				};
			};
		};
	};
};

#include "CfgEventHandlers.hpp"


