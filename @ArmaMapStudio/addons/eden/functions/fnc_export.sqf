#include "script_component.hpp"

startloadingscreen ["Exporting objects"];
private _data = [];
private _all = all3DENEntities;
private _objects = _all select 0;
private _systems = _all select 3;
private _cfgVehicles = configfile >> "cfgvehicles";
private _classes = createHashMap;
private _toRemoveLayer = -1 add3DENLayer "Map integrable (to remove)";

{
	if ( _x isKindOf "ModuleHideTerrainObjects_F" ) then {
		_data pushBack [".hideArea", getPosWorld _x, (_x get3DENAttribute "Size3") select 0, (_x get3DENAttribute "isRectangle") select 0, (_x get3DENAttribute "#filter") select 0];
		_x set3DENLayer _toRemoveLayer;
		private _hidden = _x getVariable ["#objects",[]];
		{
			_data pushBack [".hide", getPosWorld _x, getModelInfo _x];
		} foreach _hidden;
	};
	
	if ( _x isKindOf "ModuleEditTerrainObject_F" ) then {
		private _value = _x getVariable ["#state", 0];
		if ( _value == 5 ) then {
			private _building = _x getVariable ["#building", objNull];
			if ( !isNull _building ) then {
				_data pushBack [".hideObj", getPosWorld _building, getModelInfo _building];
			};
			_x set3DENLayer _toRemoveLayer;
		};
	};
	
} foreach _systems;


private _progress = 0;
private _progressTotal = count _objects;
private _whiteList = ["TargetBootcampHuman_F"];

{
	if (_x get3DENAttribute "AMS_Exclude" select 0) then {
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

				if ( (count _textures == 0 || _simpleEden == 1 || _class in _whiteList) && (_model != "\A3\Weapons_f\dummyweapon.p3d") ) then {
					_classData = [true];
					_data pushBack [".class", _class, _model, boundingBoxReal _x, boundingCenter _x];
				} else {
					_classData = [false];
					WARNING_2("%1 has textures, %2", _class, _textures);
				};
			} else {
				_classData = [false];
				WARNING_1("%1 not found", _class);
			};
			_classes set [_class,_classData];
		};
		if ( _classData select 0 ) then {
			_data pushBack [_class, getPosASL _x, getPosWorld _x, vectorUp _x, vectorDir _x];
			_x set3DENLayer _toRemoveLayer;
		};
    };
	
	_progress = _progress + 1;
	progressLoadingScreen  (_progress / _progressTotal);
	
} foreach _objects;

endloadingscreen;

uinamespace setvariable ["Display3DENCopy_data",["Arma Map Studio Data",_data joinString endl]];
(finddisplay 313) createdisplay "Display3DENCopy";