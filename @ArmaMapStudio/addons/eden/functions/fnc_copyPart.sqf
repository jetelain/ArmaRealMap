#include "script_component.hpp"		

copyToClipboard (GVAR(results) # GVAR(current));
GVAR(current) = GVAR(current) + 1;
systemChat (format ["Part %1 on %2 copied", GVAR(current), count GVAR(results)]);

if ( GVAR(current) < count GVAR(results)) then {
	[
		format [LLSTRING(NextPartMessage),GVAR(current), count GVAR(results)],
		LLSTRING(ExportToGrmStudio),
		[LLSTRING(NextPartButton),{ 0 spawn { sleep 0.1; call FUNC(copyPart); }; }]
	] call BIS_fnc_3DENShowMessage;
} else {
	[LLSTRING(ExportDone), 0, 5] call BIS_fnc_3DENNotification;
}