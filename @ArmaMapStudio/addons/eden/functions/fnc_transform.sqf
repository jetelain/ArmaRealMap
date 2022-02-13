#include "script_component.hpp"

INFO_1("-> '%1'", (uiNamespace getVariable 'BIS_fnc_3DENEntityMenu_data'));

(uiNamespace getVariable 'BIS_fnc_3DENEntityMenu_data') params ["", "_module"];

INFO_1("-> '%1'", _module);

if ( _module isKindOf "ModuleHideTerrainObjects_F" ) then {
	
	{
		private _mapObj = _x;
		private _dir = vectorDir _mapObj;
		private _up = vectorUp _mapObj;
		_mapObj setVectorDirAndUp [[0,1,0],[0,0,1]];
		private _pos 	= getPos  _mapObj;
		_mapObj setVectorDirAndUp [_dir,_up];

    	(getModelInfo _mapObj) params ["", "_p3d"]; 
		private _classes =
			(format ["getText (_x >> 'model') == '\%1' && getNumber (_x >> 'scope') >= 1", _p3d] configClasses (configfile >> "CfgVehicles")) + 
			(format ["getText (_x >> 'model') == '%1' && getNumber (_x >> 'scope') >= 1", _p3d] configClasses (configfile >> "CfgVehicles"));

		if ( (count _classes) > 0 ) then {
			private _entity = create3DENEntity ["Object", configName (_classes select 0), _pos, false];
    		(_mapObj call BIS_fnc_getPitchBank) params ["_pitch", "_bank"];
			_entity set3DENAttribute ["rotation", [_pitch, _bank, getDir _mapObj]];
		} else {
			WARNING_1("No class for '%1'", _p3d);
		};
	} foreach (_module getVariable ["#objects",[]]);

};
