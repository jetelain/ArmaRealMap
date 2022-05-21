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


{
	if ( _x isKindOf "ModuleHideTerrainObjects_F" ) then {
		_data pushBack [".hideArea", getPosWorld _x, (_x get3DENAttribute "Size3") select 0, (_x get3DENAttribute "isRectangle") select 0, (_x get3DENAttribute "#filter") select 0];
		_x set3DENLayer _toRemoveLayer;
		private _hidden = _x getVariable ["#objects",[]];
		{
			_data pushBack [".hide", [_x] call FUNC(getPosWrp), (getModelInfo _x) select 1];
		} foreach _hidden;
	};
	
	if ( _x isKindOf "ModuleEditTerrainObject_F" ) then {
		private _value = _x getVariable ["#state", 0];
		if ( _value == 5 ) then {
			private _building = _x getVariable ["#building", objNull];
			if ( !isNull _building ) then {
				_data pushBack [".hideObj",  [_building] call FUNC(getPosWrp), (getModelInfo _building) select 1];
			};
			_x set3DENLayer _toRemoveLayer;
		};
	};
	
	_progress = _progress + 1;
	progressLoadingScreen  (_progress / _progressTotal);
	
} foreach _systems;

{
	if (_x get3DENAttribute "AMS_Exclude" select 0) then {
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
					private _ams_canexport = getnumber (_classConfig >> "AmsEden" >> "canexport");
					private _model = gettext (_classConfig >> "model");
					private _realModel = (getModelInfo _x) select 1;
					if ( (count _textures == 0 || _simpleEden == 1 || _ams_canexport == 1) && (_model != "\A3\Weapons_f\dummyweapon.p3d") && (_ams_canexport != -1)) then {
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
				private _pos = getPosASL _x;
				private _dir = vectorDir _x;
				private _up = vectorUp _x;
				private _wpos = getPosWorld _x; 
				
				// DirAndUp has effect on getPosASL, we have to "correct" getPosWorld to take into account this
				_x setVectorDirAndUp [[0,1,0],[0,0,1]];
				private _pos2 = getPosASL _x;
				_x setVectorDirAndUp [_dir,_up];
				private _zfix = (_pos2 select 2) - (_pos select 2);
				_wpos set [2, (_wpos select 2) - _zfix];

				_data pushBack [_class, _pos, _wpos, _up, _dir, surfaceNormal _pos];
				
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