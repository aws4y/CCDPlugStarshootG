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
#include "CCDStarshootG.h"
#include "starshootg.h"

HStarshootg			m_hcam;
 int nWidth=0;									// Physical nWidth of CCD, not including overscan
 int nHeight=0;									// nHeight of array
//
float pixelXSize = 0.0;							// Pixel physical dimensions
float pixelYSize = 0.0;


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
	GuiderErrors = GE_OFF;
	FilterDelayOn = false;
	OffX = 0.;
	OffY = 0.;
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
)
{
	// Enable filter wheel and guider relays display
	HasCamera = true;
	HasFilterWheel = false;
	HasGuiderRelays = false;

	// Set up title, options
	strcpy(Contents.CameraName, "StarShootG");
	strcpy(Contents.FilterName, "None");
	strcpy(Contents.Copyright, "Starshoot G Driver\nCopyright (c) 2022 Ron Smith\nSupport: spinlock663@gmail.com");
	//	ASSERT ( strlen ( Contents.FilterName ) < 15 );
	//	ASSERT ( strlen ( Contents.CameraName ) < 15 );
	//	ASSERT ( strlen ( Contents.Copyright ) < 255 );

		// Set up dialog box initialization
	Contents.UseFilePath = false;
	Contents.NumParameters = 2;

	// Noise simulation control
	strcpy(Contents.Parameters[0].ParameterName, "Noise");
	Contents.Parameters[0].NumOptions = 2;

	strcpy(Contents.Parameters[0].Option[0].Display, "Off");
	Contents.Parameters[0].Option[0].Value = false;

	strcpy(Contents.Parameters[0].Option[1].Display, "Simulate");
	Contents.Parameters[0].Option[1].Value = false;

	// Guider simulation control
	strcpy(Contents.Parameters[1].ParameterName, "Guide Errors");
	Contents.Parameters[1].NumOptions = 4;

	strcpy(Contents.Parameters[1].Option[0].Display, "Off");
	Contents.Parameters[1].Option[0].Value = GE_OFF;

	strcpy(Contents.Parameters[1].Option[1].Display, "Sinusoidal");
	Contents.Parameters[1].Option[1].Value = GE_SINUSOIDAL;

	strcpy(Contents.Parameters[1].Option[2].Display, "Random");
	Contents.Parameters[1].Option[2].Value = GE_RANDOM;

	strcpy(Contents.Parameters[1].Option[3].Display, "Both");
	Contents.Parameters[1].Option[3].Value = GE_BOTH;

	// Set up filter wheel
	Contents.NumFilters = 0;
	Contents.NumFilterParameters = 0;

	// Filter rotation delay simulation control
	strcpy(Contents.FilterParameters[0].ParameterName, "Delay");
	Contents.FilterParameters[0].NumOptions = 2;

	Contents.FilterParameters[0].NumOptions = 2;
	strcpy(Contents.FilterParameters[0].Option[0].Display, "On");
	Contents.FilterParameters[0].Option[0].Value = 1;
	strcpy(Contents.FilterParameters[0].Option[1].Display, "Off");
	Contents.FilterParameters[0].Option[1].Value = 0;
}


//////////////////////////////////////////////////////////////////////
// OpenCamera initializes the link to the camera and allocates buffer memory
int CCDStarshootG::OpenCamera(
	const char* FilePath,					// File path provided by user (if UseFilePath above is true)
	int Param[NUM_PARAM]					// 0-based index for each user parameter input
)
{
	// If we are re-initializing, make sure old storage deleted
	CloseCamera();

	// Include any initialization that needs to happen each time the camera is started
	// from MaxIm CCD's Setup tab
	m_hcam = Starshootg_Open(NULL);
	if (m_hcam == NULL)
	{
		return RS_LinkFail;
	}

	HRESULT hr = Starshootg_get_Size(m_hcam, &nWidth, &nHeight);
	if (FAILED(hr))
		return RS_DriverError;

	hr = Starshootg_get_PixelSize(m_hcam, 0, &pixelXSize, &pixelYSize);

	// Allocate image buffer used during download
	// Important:  if this function fails, you must deallocate this memory (and any other allocated variables)

	Buffer = new unsigned short [nWidth * nHeight];
	if (Buffer == NULL) return RS_OutOfMemory;

	// Check whether we want to add noise to the image
	NoiseOn = Param[0];
	GuiderErrors = GuiderErrorMode(Param[1]);

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
	Starshootg_Close(m_hcam);

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

	StopTime = clock() + Exposure * CLOCKS_PER_SEC / 100;

	return 0;
}

//////////////////////////////////////////////////////////////////////
// AbortExposure aborts the current imaging operation and returns the camera to idle
// (n.b. If camera hardware needs to be reset, GetCameraState should return CCD_FLUSHING until cleared)
void CCDStarshootG::AbortExposure()
{
	StopTime = 0;
}

//////////////////////////////////////////////////////////////////////
// TransferImage downloads the image data from the camera
int CCDStarshootG::TransferImage(
	IMPBOOL& TransferDone,						// May return periodically to update GUI, true when done
	int& PercentDone						// If returns periodically during download, percentage complete
)
{
	int Size = SubframeXSize * SubframeYSize;

	// Simulate readout progress
	// Write some dummy data into the array (consumes time and simulates date)
	for (int I = 0; I < Size / 20; I++)
	{
		if (Pixel >= Size) break;
		if (NoiseOn)
		{
			Buffer[Pixel++] = rand() >> 10;
		}
		else
		{
			Buffer[Pixel++] = 0;
		}
	}

	// Note: recommend exiting periodically, not on every line, to avoid slowing down the readout
	// Returning with percentage is recommended, but optional (some cameras don't like random delays during readout)
	TransferDone = false;
	PercentDone = Pixel * 100 / Size;
	if (PercentDone <= 95) return 0;

	// Make sure last bit of array filled
	while (Pixel < Size)
	{
		if (NoiseOn)
		{
			Buffer[Pixel++] = rand() >> 10;
		}
		else
		{
			Buffer[Pixel++] = 0;
		}
	}

	if (LightFrame)
	{
		// Simulate a sinusoidal guider error of +/-three pixels with a time constant of four minutes 
		// (typical worm cycle) plus small random variations of < 1/4 pixel
		time_t Tic = clock() % (CLOCKS_PER_SEC * 240);  // Modulo 240 seconds
		double T = double(Tic) / CLOCKS_PER_SEC;
		double Delta = 3. * sin(2 * 3.141592654f * T / 240.);
		double AbsOff = 0.;

		double RandX = double(rand() - RAND_MAX / 4) / RAND_MAX;
		double RandY = double(rand() - RAND_MAX / 4) / RAND_MAX;

		switch (GuiderErrors)
		{
		case GE_OFF:
			break;
		case GE_RANDOM:
			OffX += RandX;
			OffY += RandY;
			break;
		case GE_SINUSOIDAL:
			AbsOff = Delta;
			break;
		case GE_BOTH:
			OffX += RandX;
			OffY += RandY;
			AbsOff = Delta;
			break;
		}

		// Make artificial stars
		MakeStar(Buffer, 256. + OffX + AbsOff, 128. + OffY, 32.f);
		MakeStar(Buffer, 512. + OffX + AbsOff, 128. + OffY, 64.f);
		MakeStar(Buffer, 384. + OffX + AbsOff, 256. + OffY, 512.f);
		MakeStar(Buffer, 256. + OffX + AbsOff, 384. + OffY, 1024.f);
		MakeStar(Buffer, 512. + OffX + AbsOff, 384. + OffY, 2048.f);
	}

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
	if (!ControlOn)
	{
		TSetPoint = 20.0;
		Temperature = 20.0;
	}
	else if (GoToAmbient)
	{
		TSetPoint = 20.0;
	}
	else
	{
		TSetPoint = Temp;
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
	Temp = 0.0f;
	TempAmbient = 0.0f;
	State = COOL_ON;						// You must set State = COOL_NOCONTROL the camera has no cooler control
	Power = 101;
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

		if (clock() > StopTime)
		{
			Shutter = false;
			ShutterOpen = Shutter;
			Exposing = false;
			Reading = true;
			State = CCD_READING;
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
