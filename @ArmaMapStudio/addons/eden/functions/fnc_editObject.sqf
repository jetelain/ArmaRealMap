#include "script_component.hpp"

(uinamespace getvariable ["bis_fnc_3DENEntityMenu_data",[]]) params ["_pos"];
_pos set [2,0];

private _module = create3DENEntity ["Logic", "ModuleEditTerrainObject_F", _pos, false];
_module set3DENAttribute ["#position", _pos];
_module set3DENAttribute ["#state", 5];
_module set3DENAttribute ["#filter", 15];

private _building = ["getBuilding",[_module]] call bis_fnc_moduleEditTerrainObject;

[_building] call FUNC(transformObject);