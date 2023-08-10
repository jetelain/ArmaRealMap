#include "script_component.hpp"

INFO_1("-> '%1'", (uiNamespace getVariable 'BIS_fnc_3DENEntityMenu_data'));

(uiNamespace getVariable 'BIS_fnc_3DENEntityMenu_data') params ["", "_module"];

INFO_1("-> '%1'", _module);

if ( _module isKindOf "ModuleHideTerrainObjects_F" ) then {
	
	{
		private _mapObj = _x;
		(getModelInfo _mapObj) params ["", "_p3d"]; 

		if ( _p3d != "" ) then {

			private _wpos = [_mapObj] call FUNC(getPosWrp);
			private _pos = ASLToAGL getPosASL _mapObj;
			private _realPos = [_wpos#0,_wpos#1,_pos#2];

			private _classes =
				(format ["getText (_x >> 'model') == '\%1' && getNumber (_x >> 'scope') >= 1", _p3d] configClasses (configfile >> "CfgVehicles")) + 
				(format ["getText (_x >> 'model') == '%1' && getNumber (_x >> 'scope') >= 1", _p3d] configClasses (configfile >> "CfgVehicles"));

			if ( (count _classes) > 0 ) then {
				private _entity = create3DENEntity ["Object", configName (_classes select 0), _realPos, false];
				(_mapObj call BIS_fnc_getPitchBank) params ["_pitch", "_bank"];
				_entity set3DENAttribute ["Rotation", [_pitch, _bank, getDir _mapObj]];
				_entity set3DENAttribute ["Position", _realPos];
			} else {
				WARNING_1("No class for '%1'", _p3d);
			};
		};
	} foreach (_module getVariable ["#objects",[]]);

};
