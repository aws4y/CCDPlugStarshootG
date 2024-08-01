//------------------------------------------------------------------------
//
// Module:    CCDStarshootG
// Filename:  CCDStarshootG.cpp
// Purpose:   Create a camera interface to the Orion StarshootG MaxIm DL/CCD
//            plug-in camera interface.  Implements CCDStarshootG class.
// Version:   1.0
//
// Copyright (c) 2022
// Written by Ron W. Smith
//
//------------------------------------------------------------------------

/////////////////////////////////////////////////////////////////
// USE OF THIS SOFTWARE
//
// This source code is licensed for use in developing a plug-in DLL for controlling 
// CCD cameras under MaxIm CCD only.  This plug-in source code file shall not be 
// distributed in whole or in part without the express written permission of 
// Diffraction Limited,  Executable DLL files built using this source code 
// may be freely distributed.  

#include <stddef.h>
#include <stdlib.h>
#include <conio.h>
#include <stdio.h>
#include <math.h>
#include <float.h>
#include <time.h>
#include <string.h>
#include <process.h>
#include "CCDStarshootG.h"
#include "starshootg.h"
#include "resource.h"

typedef struct Settings {
	int GC;
	int Speed;
	int LowNoise;
	int Skip;
	int BlackLevel;
	int DFC;
	int Heat;
};



HStarshootg			m_hcam;
 int nWidth=0;									// Physical nWidth of CCD, not including overscan
 int nHeight=0;									// nHeight of array
//
float pixelXSize = 0.0;							// Pixel physical dimensions
float pixelYSize = 0.0;

unsigned char* byBuff;

int GetGain();
Settings GetSetting();
//////////////////////////////////////////////////////////////////////
// Non-class DLL entry points
//////////////////////////////////////////////////////////////////////

DLLCALL CMaxImCCDPlugIn* NewPlugIn()
{
	// Create a new CMaxImCCDPlugIn-derived object, and return pointer
	return new CCDStarshootG();
}

DLLCALL void DeletePlugIn(CMaxImCCDPlugIn* PlugIn)
{
	// Delete the previously-created object
	CCDStarshootG* Plug = (CCDStarshootG*)PlugIn;
	delete Plug;
}

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CCDStarshootG::CCDStarshootG()
{
	// Initialization that only needs to happen once
	Buffer = NULL;
	BinningX = 1;
	BinningY = 1;
	StopTime = 0;
	SubframeXStart = 0;
	SubframeYStart = 0;
	SubframeXSize = 0;
	SubframeYSize = 0;
	Temperature = 20.0;
	TSetPoint = 20.0;
	Shutter = false;
	Exposing = false;
	Reading = false;
	LightFrame = false;
	NoiseOn = false;
	FilterDelayOn = false;
	OffX = 0.;
	OffY = 0.;
	BlackLevel = 0;
	Pixel = 0;
}

CCDStarshootG::~CCDStarshootG()
{
	// Make sure the camera is shut down
	CloseCamera();
}


//////////////////////////////////////////////////////////////////////
// Member functions
//////////////////////////////////////////////////////////////////////
void CCDStarshootG::MakeStar(unsigned short* Buffer, double CenterX, double CenterY, double Size)
{
	// Create an artificial star for testing guider performance (not needed for a real camera!)
	const float Sigma = 3.0f;
	const int Span = unsigned short(10 * Sigma);

	// Convert sigma to 1/sigma squared
	float w = 1.0f / (2.0f * Sigma * Sigma);

	CenterX -= SubframeXStart;
	CenterY -= SubframeYStart;

	// Loop over image area
	int CY = int(CenterY);
	int CX = int(CenterX);
	for (int IY = CY - Span; IY < CY + Span; IY++)
	{
		if (IY < 0 || IY > SubframeYSize) continue;
		double y = IY - CenterY;
		for (int IX = CX - Span; IX < CX + Span; IX++)
		{
			if (IX < 0 || IX > SubframeXSize) continue;
			double x = IX - CenterX;
			double r = w * ((x * x) + (y * y));
			Buffer[IX + IY * SubframeXSize] += unsigned short(Size * exp(-r));
		}
	}
}

//////////////////////////////////////////////////////////////////////
// GetParameters tells MaxIm CCD what information to ask the user for
void CCDStarshootG::GetParameters(
	DialogContents& Contents,				// Controls the setup dialog box
	IMPBOOL& HasCamera,						// If true, MaxIm CCD will add this to camera and guider lists
	IMPBOOL& HasFilterWheel,					// If true, MaxIm CCD will add camera's internal filter wheel to list
	IMPBOOL& HasGuiderRelays					// If true; MaxIm CCD will use camera's autoguider relays
	//IMPBOOL& HasGainControl
)
{
	// Enable filter wheel and guider relays display
	HasCamera = true;
	HasFilterWheel = false;
	HasGuiderRelays = false;
	//HasGainControl = true;
	// Set up title, options
	strcpy_s(Contents.CameraName, "StarShootG");
	//strcpy(Contents.FilterName, "None");
	strcpy_s(Contents.Copyright, "Starshoot G Driver\nCopyright (c) 2022 Ron Smith\nSupport: spinlock663@gmail.com");
	strcpy_s(Contents.Parameters[0].ParameterName, "Gain Dialog");
	strcpy_s(Contents.Parameters[1].ParameterName, "Settings Dialog");
	//	ASSERT ( strlen ( Contents.FilterName ) < 15 );
	//	ASSERT ( strlen ( Contents.CameraName ) < 15 );
	//	ASSERT ( strlen ( Contents.Copyright ) < 255 );

		// Set up dialog box initialization
	Contents.UseFilePath = false;
	Contents.NumParameters = 2;
	Contents.Parameters[0].NumOptions = 2;
	Contents.Parameters[1].NumOptions = 2;
	strcpy_s(Contents.Parameters[0].Option[0].Display, "On");
	strcpy_s(Contents.Parameters[0].Option[1].Display, "Off");;
	Contents.Parameters[0].Option[0].Value = true;
	Contents.Parameters[0].Option[1].Value = false;
	strcpy_s(Contents.Parameters[1].Option[0].Display, "On");
	strcpy_s(Contents.Parameters[1].Option[1].Display, "Off");;
	Contents.Parameters[1].Option[0].Value = true;
	Contents.Parameters[1].Option[1].Value = false;

	
}


//////////////////////////////////////////////////////////////////////
// OpenCamera initializes the link to the camera and allocates buffer memory
int CCDStarshootG::OpenCamera(
	const char* FilePath,					// File path provided by user (if UseFilePath above is true)
	int Param[NUM_PARAM]					// 0-based index for each user parameter input
)
{
	int			nBitDepth;
	int			nSpeed;
	int         nSkip;
	int			nBlackLevel;
	int			nHeatmax;
	Settings setting;

	// If we are re-initializing, make sure old storage deleted
	if (m_hcam != NULL)
	{
		CloseCamera();
	}
	// Include any initialization that needs to happen each time the camera is started
	// from MaxIm CCD's Setup tab
	m_hcam = Starshootg_Open(NULL);
	if (m_hcam == NULL)
	{
		return RS_LinkFail;
	}
	HRESULT hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_TRIGGER, 1);
	hr = Starshootg_put_AutoExpoEnable(m_hcam, 0);
	hr = Starshootg_put_VignetEnable(m_hcam, 0);
	hr = Starshootg_put_Negative(m_hcam, 0);
    hr = Starshootg_get_Size(m_hcam, &nWidth, &nHeight);
	if (FAILED(hr))
		return RS_DriverError;

	hr = Starshootg_get_PixelSize(m_hcam, 0, &pixelXSize, &pixelYSize);
	hr = Starshootg_get_Option(m_hcam, STARSHOOTG_OPTION_BLACKLEVEL, &nBlackLevel);
	if (hr != 0x80004001)
	{
		BlackLevel = nBlackLevel ;
	}
	else
	{
		BlackLevel = 0;
	}
	hr = Starshootg_get_Option(m_hcam, STARSHOOTG_OPTION_HEAT_MAX, &nHeatmax);


	hr = Starshootg_put_eSize(m_hcam, 0);
	hr = Starshootg_put_HZ(m_hcam, 2);
	
	// Allocate image buffer used during download
	// 
	// Important:  if this function fails, you must deallocate this memory (and any other allocated variables)

	Buffer = new unsigned short [nWidth * nHeight];
	if (Buffer == NULL) return RS_OutOfMemory;

	byBuff = new unsigned char[2 * nWidth * nHeight];
	if (byBuff == NULL)
	{
		return RS_OutOfMemory;
	}
	if (Param[1])
	{
		char fileName[100];
		char* args = new char[1];
		strcpy_s(fileName, getenv("USERPROFILE"));
		strcat_s(fileName, "\\bin\\StarshootG\\SettingsSSG.exe");
		_spawnl(P_WAIT, fileName, args, NULL);
	}
	
	
	if (Param[0])
	{	
		char fileName[100];
		char* args = new char[1];
		strcpy_s(fileName, getenv("USERPROFILE"));
		strcat_s(fileName, "\\bin\\StarshootG\\GainControlSSG.exe");
		_spawnl(P_NOWAIT, fileName,args,NULL);
	}
	setting=GetSetting();
	hr = Starshootg_put_Speed(m_hcam, setting.Speed);
	hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_RAW, 1);
	hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_BITDEPTH, 1);
	hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_LOW_NOISE, setting.LowNoise);
	hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_CG, setting.GC);
	hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_DFC, setting.DFC);
	if (setting.Heat < nHeatmax)
	{
		hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_HEAT, setting.Heat);
	}
	else
	{
		hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_HEAT, nHeatmax);
	}

	if (BlackLevel != setting.BlackLevel)
	{
		BlackLevel = setting.BlackLevel;
	}

	hr = Starshootg_put_Mode(m_hcam, setting.Skip);
	hr = Starshootg_put_LevelRange(m_hcam, new unsigned short[4], new unsigned short[4] {255, 255, 255, 255});
	hr = Starshootg_StartPullModeWithCallback(m_hcam, NULL, NULL);
	BinningX = 1;
	BinningY = 1;
	StopTime = 0;
	SubframeXStart = 0;
	SubframeYStart = 0;
	SubframeXSize = 0;
	SubframeYSize = 0;
	Temperature = 20.0;
	TSetPoint = 20.0;
	Shutter = false;
	Exposing = false;
	Reading = false;
	

	return 0;
}


//////////////////////////////////////////////////////////////////////
// CloseCamera shuts down the link to the camera and deallocates buffer memory
void CCDStarshootG::CloseCamera()
{
	// Shut down link to camera, if any
	if (m_hcam != NULL)
	{
		Starshootg_Close(m_hcam);
		m_hcam = NULL;
	}
	// Delete image buffer and reset pointer to null 

	
	delete[] Buffer;
	Buffer = NULL;

	// Delete any other allocated variables 
	// IMPORTANT -- you must set all pointers to NULL when deleted
}


//////////////////////////////////////////////////////////////////////
// GetArraySize returns the number of pixels in X and Y for the CCD array (unbinned)
void CCDStarshootG::GetArraySize(
	int& XSize,								// Number of array columns (X)
	int& YSize								// Number of array rows (Y)
)
{
	XSize = nWidth;
	YSize = nHeight;
}


//////////////////////////////////////////////////////////////////////
// GetImageSize returns the number of pixels in each dimension for the current image, 
// plus offset value for subframe; includes binning
void CCDStarshootG::GetImageSize(
	int& XStart,							// Column offset for subframe (X) (after binning)
	int& YStart,							// Row offset for subframe (Y) (after binning)
	int& XSize,								// Number of image columns (X)
	int& YSize								// Number of image rows (Y)
)
{
	XStart = SubframeXStart;
	YStart = SubframeYStart;
	XSize = SubframeXSize;
	YSize = SubframeYSize;
}


//////////////////////////////////////////////////////////////////////
// GetPixelAspect returns the size of the pixels in microns (micrometers)
IMPBOOL CCDStarshootG::GetPixelAspect(
	double& PixX,							// Pixel physical X dimension in microns
	double& PixY, 							// Pixel physical Y dimension in microns
	IMPBOOL IncludeBinning						// If TRUE, multiply above by binning factors
)
{
	PixX = pixelXSize;
	PixY = pixelYSize;
	if (IncludeBinning)
	{
		PixX *= BinningX;
		PixY *= BinningY;
	}
	return true;
}

//////////////////////////////////////////////////////////////////////
// GetImageBuffer returns a pointer to the image buffer allocated by OpenCamera and filled by TransferImage
unsigned short* CCDStarshootG::GetImageBuffer()
{
	for (int i = 0; i < nWidth * nHeight; i++ )
	{
		Buffer[i] = byBuff[2 * i+1] << 8 | byBuff[2 * i];
	}
	return Buffer;
}

//////////////////////////////////////////////////////////////////////
// StartExposures starts a CCD exposure
int CCDStarshootG::StartExposure(
	unsigned int Exposure,					// Exposure duration in 1/100ths of a second
	int XStart,								// X Offset to start of subframe 
	int YStart, 							// Y Offset to start of subframe 
	int NumX, 								// Number of columns for subframe
	int NumY, 								// Number of rows for subframe
	int BinX, 								// X binning factor (all above numbers are in binned pixel units)
	int BinY, 								// Y binning factor
	IMPBOOL Light, 							// If true open shutter for exposure, if false close shutter
	IMPBOOL FastReadout, 						// If true we are in focus mode (optionally use faster readout mode)
	IMPBOOL HoldShutterOpen 					// If true we are in guider mode (optionally leave shutter open for readout)
)
{
	if ((XStart + NumX) * BinX > nWidth) return RS_IllegalInput;
	if ((YStart + NumY) * BinY > nHeight) return RS_IllegalInput;

	Shutter = Light;
	LightFrame = Light;
	SubframeXStart = XStart;
	SubframeYStart = YStart;
	SubframeXSize = NumX;
	SubframeYSize = NumY;
	Exposing = true;
	Reading = false;
	Pixel = 0;

	HRESULT hr= Starshootg_put_ExpoTime(m_hcam,Exposure*10);
	hr= Starshootg_put_ExpoAGain(m_hcam,GetGain());
	
	hr = Starshootg_Trigger(m_hcam, 1);
	
	
	
	
	StopTime = clock() + Exposure * CLOCKS_PER_SEC / 100;

	return 0;
}

//////////////////////////////////////////////////////////////////////
// AbortExposure aborts the current imaging operation and returns the camera to idle
// (n.b. If camera hardware needs to be reset, GetCameraState should return CCD_FLUSHING until cleared)
void CCDStarshootG::AbortExposure()
{
	StopTime = 0;
	HRESULT hr = Starshootg_Trigger(m_hcam, 0);
	hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_FLUSH, 3);
}

//////////////////////////////////////////////////////////////////////
// TransferImage downloads the image data from the camera
int CCDStarshootG::TransferImage(
	IMPBOOL& TransferDone,						// May return periodically to update GUI, true when done
	int& PercentDone						// If returns periodically during download, percentage complete
)
{
	unsigned int pWidth;
	unsigned int pHeight;
	


	// Note: recommend exiting periodically, not on every line, to avoid slowing down the readout
	// Returning with percentage is recommended, but optional (some cameras don't like random delays during readout)
	TransferDone = false;

	PercentDone = 100;
	TransferDone = true;
	return 0;
}

//////////////////////////////////////////////////////////////////////
// SetTemperature controls the CCD cooler
IMPBOOL CCDStarshootG::SetTemperature(
	IMPBOOL ControlOn,							// If true, controller is turned on, if false, turned off
	IMPBOOL GoToAmbient,						// GoToAmbient should start a gradual warm-up to ambient
	double Temp								// Desired setpoint when ControlOn = true and GoToAmbient = false
)
{
	HRESULT hr;
	short ssgTemp; 
	int ssgFan;
	if (!ControlOn)
	{
		TSetPoint = 20.0;
		hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_TEC, 0);
		if (FAILED(hr))
		{
			return false;
		}
		hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_FAN, 0);
		if (FAILED(hr))
		{
			return false;
		}
		hr=Starshootg_get_Temperature(m_hcam, &ssgTemp);
		if (FAILED(hr))
		{
			return false;
		}
		Temperature = (double)ssgTemp / 10.0;
	}
	else if (GoToAmbient)
	{
		hr= Starshootg_put_Temperature(m_hcam, 0);
		if (FAILED(hr))
		{
			return false;
		}
		hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_FAN, 0);
		if (FAILED(hr))
		{
			return false;
		}

	}
	else
	{
		
		TSetPoint = Temp;
		hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_TEC, 1);
		if (FAILED(hr))
		{
			return false;
		}
		hr = Starshootg_get_Option(m_hcam, STARSHOOTG_OPTION_FAN, &ssgFan);
		if (FAILED(hr))
		{
			return false;
		}
		if (ssgFan == 0)
		{
			hr = Starshootg_put_Option(m_hcam, STARSHOOTG_OPTION_FAN, INT_MAX);
			if (FAILED(hr))
			{
				return false;
			}
		}
		if (FAILED(hr))
		{
			return false;
		}
		ssgTemp = (short)(TSetPoint * 10.0);
		hr = Starshootg_put_Temperature(m_hcam, ssgTemp);
		if (FAILED(hr))
		{
			return false;
		}
		hr = Starshootg_get_Temperature(m_hcam, &ssgTemp);
		if (FAILED(hr))
		{
			return false;
		}
		Temperature = (double)ssgTemp / 10.0;
		
	
		
	}
	
	return true;
}

//////////////////////////////////////////////////////////////////////
// GetTemperature returns information about the cooler status
int CCDStarshootG::GetTemperature(
	CoolerState& State,						// Current temperature controller status
	double& Temp,							// Temperature of CCD chip (FLT_MAX if not available)
	double& TempAmbient,					// Ambient temperature if available (FLT_MAX if not)
	unsigned short& Power					// Power level in percent (0-100), return > 100 if not available
)
{   
	HRESULT hr;
	short ssgTemp; 
	int bTec;
	int ssgCVoltage;
	int ssgMaxVoltage;
	hr = Starshootg_get_Temperature(m_hcam, &ssgTemp);
	if (FAILED(hr))
	{
		return false;
	}
	Temp = (double)ssgTemp / 10.0;
	TempAmbient = 0.0f;
	State = COOL_ON;						// You must set State = COOL_NOCONTROL the camera has no cooler control
	hr = Starshootg_get_Option(m_hcam, STARSHOOTG_OPTION_TEC_VOLTAGE, &ssgCVoltage);
	if (FAILED(hr))
	{
		return false;
	}
	hr = Starshootg_get_Option(m_hcam, STARSHOOTG_OPTION_TEC_VOLTAGE_MAX, &ssgMaxVoltage);
	if (FAILED(hr))
	{
		return false;
	}
	Power = (short)(((double)ssgCVoltage/(double)ssgMaxVoltage)*100.0);
	hr = Starshootg_get_Option(m_hcam, STARSHOOTG_OPTION_TEC, &bTec);
	if (!bTec)
	{
		State = COOL_OFF;
	}
	return 0;
}

//////////////////////////////////////////////////////////////////////
// GetCameraStats returns various information about the camera
void CCDStarshootG::GetCameraStats(
	int& XSize,							// Number of rows in physical CCD array (no binning)
	int& YSize, 							// Number of columns in physical CCD array (no binning)
	int& MaxBinFactorX,					// Maximum binning factor allowed on X axis
	int& MaxBinFactorY, 					// Maximum binning factor allowed on Y axis
	IMPBOOL& DifferentAxes, 					// Return true if X and Y binning factors can be different
	IMPBOOL& HasShutter, 						// Return true if camera has a shutter
	IMPBOOL& HasCoolerControl, 				// Return true if camera has a controllable cooler setpoint
	IMPBOOL& PowerOfTwoBinning, 				// Return true if user can select 1/2/4/8/16 (max) binning only
	IMPBOOL& HasGuiderRelays					// Return true if camera has built-in autoguider output relays
)
{
	XSize = nWidth;
	YSize = nHeight;
	MaxBinFactorX = 1;
	MaxBinFactorY = 1;
	DifferentAxes = false;
	HasShutter = false;
	HasCoolerControl = true;
	PowerOfTwoBinning = false;
	HasGuiderRelays = false;
}

//////////////////////////////////////////////////////////////////////
// GetCameraState returns the current camera status
void CCDStarshootG::GetCameraState(
	CameraState& State,						// Camera state; lower enum values are higher priority
	IMPBOOL& ShutterOpen						// Return true if shutter is currently open
)
{
	ShutterOpen = Shutter;
	if (Exposing)
	{
		State = CCD_EXPOSING;
		//HRESULT hr = Starshootg_Trigger(m_hcam, 1);
	
		
		if(clock()>StopTime)
		{
			Shutter = false;
			ShutterOpen = Shutter;
			Exposing = false;
			Reading = true;
			State = CCD_READING;
			HRESULT hr=Starshootg_Trigger(m_hcam, 0);
		
		    hr = Starshootg_PullImageV2(m_hcam, byBuff,8, NULL);
			StopTime += CLOCKS_PER_SEC * 2;
			return;
		}
	}
	else if (Reading)
	{
		State = CCD_READING;
		
		if (clock() > StopTime)
		{
			Reading = false;
			State = CCD_IDLE;
			StopTime = 0;
			return;
		}
	}
	else
	{
		State = CCD_IDLE;
	}
}

//////////////////////////////////////////////////////////////////////
// ActivateRelay turns on an autoguider relay output; only one axis is actuated at a time
int CCDStarshootG::ActivateRelay(
	int X,									// Activate X axis for T = abs ( X/100 ) seconds, direction based on sign
	int Y									// Activate Y axis for T = abs ( X/100 ) seconds, direction based on sign
)
{
	// Simulate relay motion, different in X and Y
	OffX += X * 0.02;
	OffY += Y * 0.01;
	return 0;
}

//////////////////////////////////////////////////////////////////////
// IsRelayDone returns true if the autoguider relay is inactive, false if active
IMPBOOL CCDStarshootG::IsRelayDone()
{
	return true;
}

//////////////////////////////////////////////////////////////////////
// OpenFilterWheel opens a filter wheel attached to the camera
int CCDStarshootG::OpenFilterWheel(
	int Param[NUM_FPARAM]					// Parameters specified in 
)
{
	FilterDelayOn = Param[0];

	// This filter wheel needs to have the camera initialized in order
	// If the filter wheel is independent, don't do this
	if (Buffer == NULL)
	{
		return RS_FilterNeedsCamera;
	}
	else
	{
		return RS_OK;
	}
}

//////////////////////////////////////////////////////////////////////
// CloseFilterWheel disconnects the filter wheel attached to the camera
void CCDStarshootG::CloseFilterWheel()
{

}

//////////////////////////////////////////////////////////////////////
// SetFilterWheel sets the filter wheel position 
int CCDStarshootG::SetFilterWheel(
	int Position							// 0-based index of filter wheel position
)
{
	if (FilterDelayOn)
	{
		// Waste cycles for half a second (ugly...)
		time_t Stop = clock() + CLOCKS_PER_SEC / 2;
		while (clock() < Stop);
	}
	return 0;
}

//////////////////////////////////////////////////////////////////////
// IsFilterWheelMoving returns true if the filter wheel is moving, false if stationary
// Note:  Some cameras only move their filters when a StartExposure command is issued; in that case,
// the GetCameraState command must return CCD_FILTERWHEELMOVING until the wheel has stopped
IMPBOOL CCDStarshootG::IsFilterWheelMoving()
{
	// We'll just return false; MaxIm CCD will very briefly show Moving
	return false;
}
#ifdef _WIN64
void CCDStarshootG::GetCameraStats(CameraInfo& camInfo)
{
	camInfo.DifferentAxes=false;
	camInfo.MaxBinFactorX = 8;
	camInfo.MaxBinFactorY = 8;
	camInfo.DualChip = false;
	camInfo.XSize = nWidth;
	camInfo.YSize = nHeight;
	camInfo.HasCoolerControl = true;
	camInfo.LoadTwoCopies = false;
	camInfo.HasShutter = false;
	camInfo.HasGuiderRelays = false;
	camInfo.HighResMode = true;
	camInfo.PowerOfTwoBinning = false;
	camInfo.Revision = 1;
}
#endif
int GetGain()
{
	FILE* inFile;
	char fileName[100];
	int min,max,gain,flag;
	strcpy(fileName, getenv("USERPROFILE"));
	strcat(fileName, "\\bin\\StarshootG\\Gain.json");
	inFile = fopen(fileName, "r");
	flag=fscanf(inFile, "{\"max_gain\":%i,\"min_gain\":%i,\"gain\":%i}", &max, &min, &gain);
	return gain; 

}

Settings GetSetting()
{
	FILE* inFile;
	char fileName[100];
	int min, max, gain, flag;
	Settings newSettings;
	strcpy(fileName, getenv("USERPROFILE"));
	strcat(fileName, "\\bin\\StarshootG\\Settings.json");
	inFile = fopen(fileName, "r");
	if(inFile != NULL)
		flag = fscanf(inFile, "{ \"gc\":%i, \"speed\" : %i, \"low_noise\" : %i, \"skip\" : %i, \"blacklevel\" : %i, \"dfc\" : %i ,\"heat\":%i}",
		&newSettings.GC, &newSettings.Speed, &newSettings.LowNoise, &newSettings.Skip, &newSettings.BlackLevel, &newSettings.DFC, &newSettings.Heat);
	return newSettings;
}
