#include "script_component.hpp"

params ["_building"];
private _pos = getPosASL _building;
private _dir = vectorDir _building;
private _up = vectorUp _building;
private _wpos = getPosWorld _building; 
private _scale = getObjectScale _building;
private _hasScale = abs (_scale - 1) > 0.001;
_building setVectorDirAndUp [[0,1,0],[0,0,1]];
if ( _hasScale ) then { _building setObjectScale _scale; };
private _pos2 = getPosASL _building;
_building setVectorDirAndUp [_dir,_up];
if ( _hasScale ) then { _building setObjectScale _scale; };
private _zfix = (_pos2 select 2) - (_pos select 2);
_wpos set [2, (_wpos select 2) - _zfix];
_wpos