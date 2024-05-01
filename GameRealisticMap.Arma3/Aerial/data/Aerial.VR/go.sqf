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

	setAperture _aperture;
	"grma3aerial" callExtension "TakeClear";
	sleep 0.5;

	private _obj = createSimpleObject [_model, [ 300, 300, _zPos + 5.05 ], true];
	setAperture _aperture;
	sleep 2;

	private _wa = screenToWorld [safeZoneX, safeZoneY];
	private _wb = screenToWorld [safeZoneX+safeZoneW, safeZoneY+safeZoneH];

	private _payload = [
		_wa # 0 - 300, _wa # 1 - 300, 
		_wb # 0 - 300, _wb # 1 - 300, 
		_model];

	setAperture _aperture;
	"grma3aerial" callExtension ["TakeImage", _payload];
	sleep 0.5;
	diag_log text format ["GRM::ONE %1", _payload];

	deleteVehicle _obj;
	
} foreach _data;

sleep 1;
endMission "END1";