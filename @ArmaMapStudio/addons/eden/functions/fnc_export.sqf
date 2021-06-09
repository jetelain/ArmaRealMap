#include "script_component.hpp"

startloadingscreen ["Exporting objects"];
private _data = [];
private _all = all3DENEntities;
private _objects = _all select 0;
private _systems = _all select 3;
private _cfgVehicles = configfile >> "cfgvehicles";
private _classes = createHashMap;

{
	if ( _x isKindOf "ModuleHideTerrainObjects_F" ) then {
		_data pushBack [".hide", getPosWorld _x, (_x get3DENAttribute "Size3") select 0, (_x get3DENAttribute "isRectangle") select 0, (_x get3DENAttribute "#filter") select 0];
	};
} foreach _systems;

private _toRemoveLayer = -1 add3DENLayer "Map integrable (to remove)";
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