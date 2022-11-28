#ifndef __starshootg_labview_h__
#define __starshootg_labview_h__

#include "extcode.h"

#ifdef STARSHOOTG_LABVIEW_EXPORTS
#define STARSHOOTG_LABVIEW_API(x) __declspec(dllexport)    x   __cdecl
#else
#define STARSHOOTG_LABVIEW_API(x) __declspec(dllimport)    x   __cdecl
#include "starshootg.h"
#endif

#ifdef __cplusplus
extern "C" {
#endif

STARSHOOTG_LABVIEW_API(HRESULT) Start(HStarshootg h, LVUserEventRef *rwer);

#ifdef __cplusplus
}
#endif

#endif