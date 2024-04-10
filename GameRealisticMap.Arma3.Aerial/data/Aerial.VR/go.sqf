private _aperture = 45; // Color calibration of Arma3ColorRender have been done with this Aperture (but 35 seems more accruate)
private _cam = "camera" camcreate [ 300, 300, 300 ];
showcinemaborder false;
sleep 0.1;
_cam cameraEffect ["internal","back"];
_cam camSetFocus [-1, -1];
_cam camSetFov 1;
_cam camSetDir [0, 0, -1];
_cam setVectorUp [0, 1, 0];
_cam camCommit 0;
setAperture _aperture;
showcinemaborder false;
sleep 2;

private _data = call (compile preprocessFileLineNumbers "data.sqf");

{
	_x params ['_model', '_altitude', '_halfWidth', '_zPos'];

	private _fov = tan(asin(_halfWidth / sqrt(( _altitude*_altitude )+(_halfWidth*_halfWidth))));

	_cam camSetFov _fov;
	_cam camSetPos [ 300, 300, _altitude ];
	_cam camCommit 0;
	setAperture _aperture;
	
	toFixed 4;
	sleep 0.1;

	private _a = screenToWorld [safeZoneX, safeZoneY];
	private _b = screenToWorld [safeZoneX+safeZoneW, safeZoneY+safeZoneH];

	setAperture _aperture;
	"grma3aerial" callExtension "TakeClear";
	sleep 0.5;
	
	private _obj = createSimpleObject [_model, [ 300, 300, _zPos + 5 ], true];
	setAperture _aperture;
	sleep 2;
	
	setAperture _aperture;
	"grma3aerial" callExtension ["TakeImage",[_a # 0 - 300, _a # 1 - 300, _b # 0 - 300, _b # 1 - 300, _model]];
	sleep 0.5;
	diag_log text format ["GRM::ONE"];

	deleteVehicle _obj;
	
} foreach _data;

sleep 1;
endMission "END1";