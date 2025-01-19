#include "script_component.hpp"

startloadingscreen ["Exporting objects"];
private _data = [];
private _all = all3DENEntities;
private _objects = _all select 0;
private _systems = _all select 3;
private _cfgVehicles = configfile >> "cfgvehicles";
private _classes = createHashMap;

private _layerName = LLSTRING(ExportLayer);
private _existingLayers = (_all #6) select { ((_x get3DENAttribute "name")#0) == _layerName };
private _toRemoveLayer =  if ( count _existingLayers > 0 ) then { _existingLayers # 0 } else { -1 add3DENLayer LLSTRING(ExportLayer) };

private _progress = 0;
private _progressTotal = count _objects + count _systems;

private _prelude = (str [".map", worldName, worldSize, getNumber (configFile >> "CfgWorlds" >> worldName >> "grma3_revision")]);

{
	if ( _x isKindOf "ModuleHideTerrainObjects_F" ) then {
		_x set3DENLayer _toRemoveLayer;
		private _hidden = _x getVariable ["#objects",[]];
		{
			_data pushBack [".hide", (getModelInfo _x) select 1, [], [_x] call FUNC(getPosWrp), vectorUp _x, vectorDir _x, surfaceNormal (getPosASL _x), getObjectScale _x, getObjectID _x];
		} foreach _hidden;
	};
	
	if ( _x isKindOf "ModuleEditTerrainObject_F" ) then {
		private _value = _x getVariable ["#state", 0];
		if ( _value == 5 ) then {
			private _building = _x getVariable ["#building", objNull];
			if ( !isNull _building ) then {
				_data pushBack [".hide", (getModelInfo _building) select 1, [], [_building] call FUNC(getPosWrp), vectorUp _building, vectorDir _building, surfaceNormal (getPosASL _building), getObjectScale _building, getObjectID _building];
			};
			_x set3DENLayer _toRemoveLayer;
		};
	};
	
	_progress = _progress + 1;
	progressLoadingScreen  (_progress / _progressTotal);
	
} foreach _systems;

private _deleteCount = count _data;

{
	if (_x get3DENAttribute "GRMA3_Exclude" select 0) then {
		// Ignored
	} else {
		if (!((_x get3denattribute "init" select 0) isequalto "")) then {
			// Ignored
		} else {
			private _class = typeof _x;
			private _classData = _classes getOrDefault [_class, []];
			if ( count _classData == 0 ) then {
				private _classConfig = _cfgVehicles >> _class;
				
				if ( isClass _classConfig ) then {
					private _textures = getarray (_classConfig >> "hiddenselectionstextures");
					private _simpleEden = getnumber (_classConfig >> "SimpleObject" >> "eden");
					private _canexport = getnumber (_classConfig >> "grma3_canexport");
					private _model = gettext (_classConfig >> "model");
					private _realModel = (getModelInfo _x) select 1;
					if ( ((_class regexMatch "Land_(.*)/i") || count _textures == 0 || _simpleEden == 1 || _canexport == 1) && (_model != "\A3\Weapons_f\dummyweapon.p3d") && (_model != "") && (_canexport != -1)) then {
						_classData = [true, 1];
						_data pushBack [".class", _class, _model, boundingBoxReal _x, boundingCenter _x, _realModel, 1];
					} else {
						_classData = [false, 0];
						WARNING_5("%1 cannot be exported: textures=%2, model=%3, simpleEden=%4, realModel=%5", _class, _textures, _model, _simpleEden, _realModel);
					};
				} else {
					_classData = [false, 0];
					WARNING_1("%1 not found", _class);
				};
				_classes set [_class,_classData];
			};
			if ( _classData select 0 ) then {
				_data pushBack [".add", _class, [], [_x] call FUNC(getPosWrp), vectorUp _x, vectorDir _x, surfaceNormal (getPosASL _x), getObjectScale _x];
				_x set3DENLayer _toRemoveLayer;
			};
		};
	};
	
	_progress = _progress + 1;
	progressLoadingScreen  (_progress / _progressTotal);
	
} foreach _objects;


private _addCount = (count _data) - _deleteCount - (count _classes);
private _elevationCount = 0;

// Deformer related data
if ( !isNil "GF_gridMap") then {
	toFixed 3; 
	private _hmap = GF_gridMap apply { [_x#0, _x#1, _y#0] };
	_elevationCount = count _hmap;
	for "_i" from 0 to (count _hmap) step 25 do { 
		_data pushBack ((str [".dhmap", _hmap select [_i, 25] ]) regexReplace ["\.000/gio", ""]);
	};
};

endloadingscreen;

INFO_1("%1 items", count _data);

private _results = [];

toFixed 20; 

// 10 000 000 max bytes
// 550 bytes max  per line
// => 18 000 lines per page
for "_i" from 0 to (count _data) step 18000 do { 
	_results pushBack (([_prelude] + (_data select [_i, 18000])) joinString endl);
};

toFixed -1; 

systemChat (format ["Total: %1 deletes, %2 adds, %3 elevation changes", _deleteCount, _addCount, _elevationCount]);

if ( count _results <= 1 ) then {
	copyToClipboard (_results # 0);
	[LLSTRING(ExportDone), 0, 5] call BIS_fnc_3DENNotification;
}
else {
	GVAR(results) = _results;
	GVAR(current) = 0;
	call FUNC(copyPart);
};
