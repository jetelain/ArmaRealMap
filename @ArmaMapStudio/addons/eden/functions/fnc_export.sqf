#include "script_component.hpp"

startloadingscreen ["Exporting objects"];
private _data = [];
private _all = all3DENEntities;
private _objects = _all select 0;
private _systems = _all select 3;
private _cfgVehicles = configfile >> "cfgvehicles";
private _classes = createHashMap;
private _toRemoveLayer = -1 add3DENLayer "Map integrable (to remove)";

private _progress = 0;
private _progressTotal = count _objects + count _systems;

_data pushBack [".map", worldName, worldSize, getNumber (configFile >> "CfgWorlds" >> worldName >> "grma3_revision")];

{
	if ( _x isKindOf "ModuleHideTerrainObjects_F" ) then {
		_x set3DENLayer _toRemoveLayer;
		private _hidden = _x getVariable ["#objects",[]];
		{
			_data pushBack [".hide", (getModelInfo _x) select 1, getPosWorld _x, [_x] call FUNC(getPosWrp), vectorUp _x, vectorDir _x, surfaceNormal (getPosASL _x), getObjectScale _x, getObjectID _x];
		} foreach _hidden;
	};
	
	if ( _x isKindOf "ModuleEditTerrainObject_F" ) then {
		private _value = _x getVariable ["#state", 0];
		if ( _value == 5 ) then {
			private _building = _x getVariable ["#building", objNull];
			if ( !isNull _building ) then {
				_data pushBack [".hide", (getModelInfo _building) select 1, getPosWorld _building, [_building] call FUNC(getPosWrp), vectorUp _building, vectorDir _building, surfaceNormal (getPosASL _building), getObjectScale _building, getObjectID _building];
			};
			_x set3DENLayer _toRemoveLayer;
		};
	};
	
	_progress = _progress + 1;
	progressLoadingScreen  (_progress / _progressTotal);
	
} foreach _systems;

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
				_data pushBack [".add", _class, getPosWorld _x, [_x] call FUNC(getPosWrp), vectorUp _x, vectorDir _x, surfaceNormal (getPosASL _x), getObjectScale _x];
				_x set3DENLayer _toRemoveLayer;
			};
		};
	};
	
	_progress = _progress + 1;
	progressLoadingScreen  (_progress / _progressTotal);
	
} foreach _objects;

endloadingscreen;
INFO_1("%1 items", count _data);

toFixed 20; 

private _result = _data joinString endl;

toFixed -1; 

copyToClipboard _result;

systemChat "Done. Copied to clipboard";