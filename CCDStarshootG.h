//------------------------------------------------------------------------
//
// Class:     CCDStarshootG
// Filename:  STARSHOOTG.h
// Purpose:   Class Definition for the StarshootG CCD Camera from orion for Maxim DL 
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

#if !defined(AFX_STARSHOOTG_H__F7EE87EB_1A2D_472F_A21F_F4A0A3BD9CD0__INCLUDED_)
#define AFX_STARSHOOTG_H__F7EE87EB_1A2D_472F_A21F_F4A0A3BD9CD0__INCLUDED_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

// Defined for dummy camera
//enum GuiderErrorMode {
//	GE_OFF,
//	GE_RANDOM,
//	GE_SINUSOIDAL,
//	GE_BOTH,
//};

#include "MaxImCCDPlugIn.h"


class CCDStarshootG : public CMaxImCCDPlugIn
{
protected:
	int BinningX;					// Binning factor
	int BinningY;
	clock_t StopTime;				// CPU clock time for simulated end of exposure
	unsigned short* Buffer;			// Buffer for readout
	int SubframeXSize;				// Subframe dimensions
	int SubframeYSize;
	int SubframeXStart;
	int SubframeYStart;
	IMPBOOL Shutter;					// Shutter open if true
	double Temperature;				// Temperature of CCD chip
	double TSetPoint;				// Setpoint 
	IMPBOOL Exposing;					// Currently exposing if true
	IMPBOOL Reading;					// Currently reading if true
	int Pixel;						// Pixel counter for simulating readout
	IMPBOOL LightFrame;				// Indicates if a light frame (for simulated data)
	IMPBOOL NoiseOn;					// Simulates image noise floor
	//GuiderErrorMode GuiderErrors;	// Simulates guider errors
	IMPBOOL FilterDelayOn;				// Simulates delay rotating filter wheel
	double OffX;					// Used for guider error simulation
	double OffY;					// Used for guider error simulation
	int BlackLevel;                 //camera exposure gain

public:
	CCDStarshootG();
	virtual ~CCDStarshootG();

	// GetParameters tells MaxIm CCD what information to ask the user for
	void GetParameters(
		DialogContents& Contents,				// Controls the setup dialog box
		IMPBOOL& HasCamera,						// If true, MaxIm CCD will add this to camera and guider lists
		IMPBOOL& HasFilterWheel,					// If true, MaxIm CCD will add camera's internal filter wheel to list
		IMPBOOL& HasGuiderRelays					// If true; MaxIm CCD will use camera's autoguider relays
		//IMPBOOL& HasGainControl
	);

	// OpenCamera initializes the link to the camera and allocates buffer memory
	int OpenCamera(
		const char* FilePath,					// File path provided by user (if UseFilePath above is true)
		int Param[NUM_PARAM]					// 0-based index for each user parameter input
	);

	// CloseCamera shuts down the link to the camera and deallocates buffer memory
	void CloseCamera();

	// GetArraySize returns the number of pixels in X and Y for the CCD array (unbinned)
	void GetArraySize(
		int& XSize,								// Number of array columns (X)
		int& YSize								// Number of array rows (Y)
	);

	// GetImageSize returns the number of pixels in each dimension for the current image, 
	// plus offset value for subframe; includes binning
	void GetImageSize(
		int& XStart,							// Column offset for subframe (X) (after binning)
		int& YStart,							// Row offset for subframe (Y) (after binning)
		int& XSize,								// Number of image columns (X)
		int& YSize								// Number of image rows (Y)
	);

	// GetPixelAspect returns the size of the pixels in microns (micrometers)
	IMPBOOL GetPixelAspect(
		double& PixX,							// Pixel physical X dimension in microns
		double& PixY, 							// Pixel physical Y dimension in microns
		IMPBOOL IncludeBinning						// If TRUE, multiply above by binning factors
	);

	// GetImageBuffer returns a pointer to the image buffer allocated by OpenCamera and filled by TransferImage
	unsigned short* GetImageBuffer();

	// StartExposures starts a CCD exposure
	int StartExposure(
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
	);

	// AbortExposure aborts the current imaging operation and returns the camera to idle
	// (n.b. If camera hardware needs to be reset, GetCameraState should return CS_FLUSHING until cleared)
	void AbortExposure();

	// TransferImage downloads the image data from the camera
	int TransferImage(
		IMPBOOL& TransferDone,						// May return periodically to update GUI, true when done
		int& PercentDone						// If returns periodically during download, percentage complete
	);

	// SetTemperature controls the CCD cooler
	IMPBOOL SetTemperature(
		IMPBOOL ControlOn,							// If true, controller is turned on, if FALSE, turned off
		IMPBOOL GoToAmbient,						// GoToAmbient should start a gradual warm-up to ambient
		double Temp								// Desired setpoint when ControlOn = true and GoToAmbient = FALSE
	);

	int GetTemperature(
		CoolerState& State,						// Current temperature controller status
		double& Temp,							// Temperature of CCD chip (FLT_MAX if not available)
		double& TempAmbient,					// Ambient temperature if available (FLT_MAX if not)
		unsigned short& Power					// Power level in percent (0-100), return > 100 if not available
	);

	// GetCameraStats returns various information about the camera
	void GetCameraStats(
		int& XSize,							// Number of rows in physical CCD array (no binning)
		int& YSize, 							// Number of columns in physical CCD array (no binning)
		int& MaxBinFactorX,					// Maximum binning factor allowed on X axis
		int& MaxBinFactorY, 					// Maximum binning factor allowed on Y axis
		IMPBOOL& DifferentAxes, 					// Return true if X and Y binning factors can be different
		IMPBOOL& HasShutter, 						// Return true if camera has a shutter
		IMPBOOL& HasCoolerControl, 				// Return true if camera has a controllable cooler setpoint
		IMPBOOL& PowerOfTwoBinning, 				// Return true if user can select 1/2/4/8/16 (max) binning only
		IMPBOOL& HasGuiderRelays					// Return true if camera has built-in autoguider output relays
	);

	// GetCameraState returns the current camera status
	void GetCameraState(
		CameraState& State,						// Camera state; lower enum values are higher priority
		IMPBOOL& ShutterOpen						// Return true if shutter is currently open
	);

	// ActivateRelay turns on an autoguider relay output; only one axis is actuated at a time
	int ActivateRelay(
		int X,									// Activate X axis for T = abs ( X/100 ) seconds, direction based on sign
		int Y									// Activate Y axis for T = abs ( X/100 ) seconds, direction based on sign
	);

	// IsRelayDone returns true if the autoguider relay is inactive, FALSE if active
	IMPBOOL IsRelayDone();

	// OpenFilterWheel opens a filter wheel attached to the camera
	int OpenFilterWheel(
		int Param[NUM_FPARAM]					// Parameters specified in 
	);

	// CloseFilterWheel disconnects the filter wheel attached to the camera
	void CloseFilterWheel();

	// SetFilterWheel sets the filter wheel position 
	int SetFilterWheel(
		int Position							// 0-based index of filter wheel position
	);

	// IsFilterWheelMoving returns true if the filter wheel is moving, FALSE if stationary
	// Note:  Some cameras only move their filters when a StartExposure command is issued; in that case,
	// the GetCameraState command must return CS_FILTERWHEELMOVING until the wheel has stopped
	IMPBOOL IsFilterWheelMoving();


	// Additional members not forming part of interface

	// Simulate a star image
	void MakeStar(unsigned short* Buffer, double CenterX, double CenterY, double Size);
#ifdef _WIN64
	void GetCameraStats(CameraInfo& camInfo);
#endif
};

#endif // !defined(AFX_STARSHOOTG_H__F7EE87EB_1A2D_472F_A21F_F4A0A3BD9CD0__INCLUDED_)

