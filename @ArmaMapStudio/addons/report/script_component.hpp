#define COMPONENT report
#include "\z\ams\addons\main\script_mod.hpp"

 #define DEBUG_MODE_FULL
 #define DISABLE_COMPILE_CACHE

#ifdef DEBUG_ENABLED_REPORT
    #define DEBUG_MODE_FULL
#endif
#ifdef DEBUG_SETTINGS_REPORT
    #define DEBUG_SETTINGS DEBUG_SETTINGS_REPORT
#endif

#include "\z\ams\addons\main\script_macros.hpp"
