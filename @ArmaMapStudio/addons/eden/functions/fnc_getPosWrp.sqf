#include "script_component.hpp"

params ["_building"];
private _pos = getPosASL _building;
private _dir = vectorDir _building;
private _up = vectorUp _building;
private _wpos = getPosWorld _building; 
_building setVectorDirAndUp [[0,1,0],[0,0,1]];
private _pos2 = getPosASL _building;
_building setVectorDirAndUp [_dir,_up];
private _zfix = (_pos2 select 2) - (_pos select 2);
_wpos set [2, (_wpos select 2) - _zfix];
_wpos