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
			_data pushBack [".hide", getPosWorld _x, (getModelInfo _x) select 1];
		} foreach _hidden;
	};
	
	if ( _x isKindOf "ModuleEditTerrainObject_F" ) then {
		private _value = _x getVariable ["#state", 0];
		if ( _value == 5 ) then {
			private _building = _x getVariable ["#building", objNull];
			if ( !isNull _building ) then {
				_data pushBack [".hideObj", getPosWorld _building, (getModelInfo _building) select 1];
			};
			_x set3DENLayer _toRemoveLayer;
		};
	};
	
	_progress = _progress + 1;
	progressLoadingScreen  (_progress / _progressTotal);
	
} foreach _systems;

private _whiteList = ["TargetBootcampHuman_F"];
private _willFollowTerrain = ["Land_New_WiredFence_5m_F", "Land_New_WiredFence_10m_F"];

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
					private _model = gettext (_classConfig >> "model");
					private _keepHorizontal = getNumber (_classConfig >> "keepHorizontalPlacement");
					private _realModel = (getModelInfo _x) select 1;
					
					if ( _class in _willFollowTerrain ) then {
						_keepHorizontal = 0; // config is sometime wrong about this, FIXME: where does this information should come from ?
					};
					
					if ( (count _textures == 0 || _simpleEden == 1 || _class in _whiteList) && (_model != "\A3\Weapons_f\dummyweapon.p3d") ) then {
						_classData = [true, _keepHorizontal];
						_data pushBack [".class", _class, _model, boundingBoxReal _x, boundingCenter _x, _realModel];
					} else {
						_classData = [false, 0];
						WARNING_5("%1 cannot be exported: textures=%2, model=%3, simpleEden=%4, realModel=%5", _class, _textures, _model, _simpleEden, _realModel);
					};
				} else {
					_classData = [false];
					WARNING_1("%1 not found", _class);
				};
				_classes set [_class,_classData];
			};
			if ( _classData select 0 ) then {
				private _pos = getPosASL _x;
				if ( _classData select 1 == 0 ) then { // keepHorizontalPlacement is false, will need extra processing
					_data pushBack [_class, _pos, getPosWorld _x, vectorUp _x, vectorDir _x, surfaceNormal _pos];
				} else {
					_data pushBack [_class, _pos, getPosWorld _x, vectorUp _x, vectorDir _x];
				};
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

systemChat "Copied to clipboard";