#include "script_component.hpp"
params ["_pos"];
private _ctrl = objNull;
if (is3DEN) then {
	_ctrl = findDisplay 313 ctrlCreate ["RscStructuredText", -1]; 
	_ctrl ctrlSetPosition [0,0,1,1]; 
	_ctrl ctrlCommit 0; 
	_ctrl ctrlAddEventHandler ["ButtonClick", { params ["_ctrl"]; ctrlDelete _ctrl; }];
}
else {
	_ctrl = findDisplay 46 createDisplay "RscDisplayEmpty" ctrlCreate ["RscStructuredText", -1]; 
	_ctrl ctrlSetPosition [0,0,1,1]; 
	_ctrl ctrlCommit 0; 
	_ctrl ctrlAddEventHandler ["ButtonClick", { params ["_ctrl"]; (ctrlParent _ctrl) closeDisplay 1; }];
};
private _pattern = "<t align='center' size='2'>Cliquer sur</t><br /><a align='center' colorLink='#ff0000' size='3' href='https://arm.pmad.net/Maps/GameIssue?name=%1&amp;x=%2&amp;y=%3'>Signaler le problème</a><br /><br /><t align='center'>Puis décrire le problème sur le site.</t>";
_ctrl ctrlSetStructuredText parseText format [_pattern, worldName, _pos select 0, _pos select 1];