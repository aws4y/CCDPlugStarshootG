#pragma once

#include "starshootg.h"

extern HStarshootg g_hCam;
extern StarshootgDeviceV2 g_cameras[STARSHOOTG_MAX];
extern int g_cameraCnt;
extern int g_snapCount;
extern int g_ROITestCount;
extern volatile bool g_bSnapFinished;
extern bool g_bSnapTesting;
extern volatile bool g_bResChangedFinished;
extern volatile bool g_bImageShoot;
extern volatile bool g_bROITesting;
extern volatile bool g_bROITest_SnapFinished;
extern volatile bool g_bOpenCloseFinished;
extern bool g_bROITest_StartSnap;
extern bool g_bTriggerTesting;
extern bool g_bEnableCheckBlack;
extern bool g_bBlack;
extern CString g_snapDir;

CString GetAppTimeDir(const CString& header);