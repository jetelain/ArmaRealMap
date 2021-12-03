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

#include "CfgEventHandlers.hpp"

class ctrlMenu;

class Display3DEN
{
    class ContextMenu : ctrlMenu
    {
        class Items
        {
            items[]+={"Separator", "AMS_Report"};
            class AMS_Report
            {
                action=QUOTE([screenToWorld getMousePosition] call FUNC(report););
                text="Signaler un problème";
                conditionShow="1";
                value=0;
                opensNewWindow=1;
            };
        };
    };
	
};
