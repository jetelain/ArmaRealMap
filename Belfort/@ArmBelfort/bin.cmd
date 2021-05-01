P:
"E:\Program Files\Steam\steamapps\common\Arma 3 Tools\Binarize\binarize_x64.exe" ^
	-targetBonesInterval=56 ^
	-maxProcesses=8 ^
	-always ^
	-addon="%~dp0addons" ^
	-exclude="%~dp0addons\belfort\exclude.lst" ^
	-textures="%~dp0textures" ^
	-binpath="E:\Program Files\Steam\steamapps\common\Arma 3 Tools\Binarize" ^
	"P:\z\arm\addons\belfort" ^
	"%~dp0addons\belfort"
copy /Y %~dp0textures\texHeaders.bin %~dp0addons\belfort\
"E:\Program Files\Steam\steamapps\common\Arma 3 Tools\CfgConvert\CfgConvert.exe" -bin -dst "%~dp0addons\belfort\config.bin" "P:\z\arm\addons\belfort\config.cpp"
pause