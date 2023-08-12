#include "script_component.hpp"

(uiNamespace getVariable 'BIS_fnc_3DENEntityMenu_data') params ["", "_module"];

if ( _module isKindOf "ModuleHideTerrainObjects_F" ) then {
	{
		[_x] call FUNC(transformObject);
	} foreach (_module getVariable ["#objects",[]]);
} else {
	if ( _module isKindOf "ModuleEditTerrainObject_F" ) then {
		private _value = _module getVariable ["#state", 0];
		if ( _value == 5 ) then {
			[_module getVariable ["#building", objNull]] call FUNC(transformObject);
		};
	};
};