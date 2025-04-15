#include "script_component.hpp"

params ['_mapObj'];

(getModelInfo _mapObj) params ["", "_p3d", "", "_placingPoint"]; 

if ( _p3d != "" ) then {
	private _scale = getObjectScale _mapObj;
	private _wpos = [_mapObj] call FUNC(getPosWrp);
	private _pos = ASLToAGL getPosASL _mapObj;
	private _realPos = [_wpos#0,_wpos#1,_pos#2];
	private _hasScale = abs (_scale - 1) > 0.001;
	private _classes =
		(format ["getText (_x >> 'model') == '\%1' && getNumber (_x >> 'scope') >= 1", _p3d] configClasses (configfile >> "CfgVehicles")) + 
		(format ["getText (_x >> 'model') == '%1' && getNumber (_x >> 'scope') >= 1", _p3d] configClasses (configfile >> "CfgVehicles"));

	if ( _hasScale ) then {
		private _zOffset = _placingPoint#2;
		_realPos set [2, (_realPos select 2) + _zOffset - (_scale * _zOffset)];
	};

	if ( (count _classes) > 0 ) then {
		private _entity = create3DENEntity ["Object", configName (_classes select 0), _realPos, false];
		(_mapObj call BIS_fnc_getPitchBank) params ["_pitch", "_bank"];
		_entity set3DENAttribute ["Rotation", [_pitch, _bank, getDir _mapObj]];
		_entity set3DENAttribute ["Position", _realPos];
		if ( _hasScale ) then {
			_entity set3DENAttribute ["ENH_objectScaling", _scale];
			_entity setObjectScale _scale;
		};
	} else {
		WARNING_1("No class for '%1'", _p3d);
	};
};