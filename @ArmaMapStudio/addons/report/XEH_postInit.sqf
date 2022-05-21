#include "script_component.hpp"

["ARM", "arm_report", "Report an issue", {
	[getPos player] call FUNC(report);
	true
}, {false}, [0x35, [false, true, false]], true] call CBA_fnc_addKeybind;
