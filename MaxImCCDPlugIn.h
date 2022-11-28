//------------------------------------------------------------------------
//
// Class:     CMaxImCCDPlugIn
// Filename:  MaxImCCDPlugIn.h
// Purpose:   Class definition for MaxImCCD camera plug in
//            plus DLL entry points
// Version:   1.1
//
// Copyright (c) 1998-2003  Diffraction Limited
// Written by Douglas B. George
//
// 2000/02/04 Added COOL_INCREASING and COOL_DECREASING display options
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


/////////////////////////////////////////////////////////////////
// TECHNICAL NOTES
//
// This file is designed for Visual C++ 5.0 or higher; modifications may be 
// required for other compilers. MaxIm CCD will try to link to the DLL on 
// startup if its name is CCDPlug*.dll (where * is any valid file name string)
// and the dll has the correct two entry points. 
// 
// This file defines an abstract class.  All functions must be overridden, 
// even if they are not used.  The two entry points are not part of the class; 
// however they use C++ calling conventions, including name mangling.  
// 
// The DLL is responsible for allocating the image buffer and all other 
// variables when OpenCamera is called, and deallocating all variables when 
// CloseCamera is called.  Failure to properly allocate/deallocate memory may 
// cause memory corruption and crash the application.
// 
// Source code for a sample "DummyCCD" DLL is included.  This DLL will be 
// detected by MaxIm CCD and will imitate a camera with filter wheel.  DummyCCD 
// is an excellent starting point for your driver; just modify the code to add 
// your camera interface.  Knowledge of C++ programming is therefore not required 
// to implement a driver.  
//
// If you are using this DLL to implement a stand-alone filter wheel interface,
// set HasCamera to FALSE. All of the class function must be present, but the 
// unused ones can be empty.
// 
// Support for the DLL is the responsibility of the author.  We strongly recommend 
// that you include a version number and support information in your copyright notice.  
// Three lines of text are provided for that information. 


// Ensure header file is not included more than once
#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#define IMPBOOL unsigned short

// Return status parameters

typedef enum ReturnStatus
{
								// Note: camera driver may return positive numbers as OEM error codes
	RS_OK = 0,					// Everything OK!
	RS_GenericError = -1,		// Any error not represented below
	RS_OutOfMemory = -2,		// malloc or new failed
	RS_IllegalInput = -3,		// Illegal input parameter detected
	RS_LinkFail = -4,			// Could not link or link dropped
	RS_FilterNeedsCamera = -5,	// Filter wheel could not connect because camera must be initialized first
	RS_DriverError = -6,		// Internal driver error
	RS_PortNotAvailable = -7,	// Port conflict
	RS_GuiderNeedsCamera = -8,	// Dual chip autoguider camera requires main camera connected first
	RS_GuiderConflict = -9,		// Dual chip autoguider fighting with main camera shutter
	RS_FilterCFGMissing = -10,	// Cannot find configuration file for filter wheel
};

typedef enum CameraState
{							// Highest priority at top of list
	CCD_ERROR,				// Camera is not available
	CCD_FILTERWHEELMOVING,	// Waiting for filter wheel to finish moving
	CCD_FLUSHING,			// Flushing CCD chip or camera otherwise busy
	CCD_WAITTRIGGER,		// Waiting for an external trigger event
	CCD_DOWNLOADING,		// Downloading the image from camera hardware
	CCD_READING,			// Reading the CCD chip into camera hardware
	CCD_EXPOSING,			// Exposing dark or light frame
	CCD_IDLE,				// Camera idle
};

typedef enum CoolerState
{
	COOL_OFF,				// Cooler is off
	COOL_ON,				// Cooler is on
	COOL_ATAMBIENT,			// Cooler is on and regulating at ambient temperature (optional)
	COOL_GOTOAMBIENT,		// Cooler is on and ramping to ambient
	COOL_NOCONTROL,			// Cooler cannot be controlled on this camera; open loop
	COOL_INITIALIZING,		// Cooler control is initializing (optional -- displays "Please Wait")
	COOL_INCREASING,		// Cooler temperature is going up    NEW 2000/02/04
	COOL_DECREASING,		// Cooler temperature is going down  NEW 2000/02/04
	COOL_BROWNOUT,			// Cooler brownout condition  NEW 2001/09/06
};

typedef enum IniFileUsage
{
    INI_None = 0,           // No INI file needed, don't display selection controls
    INI_Path = 1,           // INI file needed, show path & browse button
    INI_IP = 2              // IP address needed, show edit control only
};

// Interface constants

const int NUM_PARAM = 5;	// Camera parameter array runs from 0 to 4
const int NUM_FPARAM = 3;	// Filter wheel parameter array runs from 0 to 2
const int NUM_OPTION = 16;	// Maximum 16 setting options per parameter
const int MAX_DISPLAY = 16;	// Maximum 15 characters in parameter strings, plus NULL char
const int MAX_PTITLE = 21;	// Maximum 20 characters in parameter titles, plus NULL char
const int MAX_NSTR = 15;	// Maximum 14 characters in camera model name, plus NULL char
const int MAX_FILTERS = 16;	// Maximum 12 filter positions // Updated 2001/09/16
const int MAX_COPYRIGHT = 255; // Copyright notice string

// Interface sturctures

typedef struct {
	char Display[ MAX_DISPLAY ];		// String to appear for this option
	int Value;							// Value to pass to camera driver when this option selected.
} OptionVals;

typedef struct {
	char ParameterName[ MAX_PTITLE ];		// Name of parameter to display
	int NumOptions;							// Number of options for this parameter (max 16)
	OptionVals Option[ NUM_OPTION ];		// List of parameter options (to appear in combo box)
} ParamVals;

typedef struct {
	char CameraName[ MAX_NSTR ];			// Name of CCD camera model
	char FilterName[ MAX_NSTR ];			// Name of filter wheel model
	char Copyright[ MAX_COPYRIGHT ];		// Copyright notice, version number, support info (can include linefeeds)
	IMPBOOL UseFilePath;						// 0 - show nothing, 1 - ask user for ".ini" file path, 2 - ask user for IP address
    int NumParameters;						// Number of parameters to display in setup dialog box max 6)
	ParamVals Parameters[ NUM_PARAM ];		// Array of camera parameter titles and values
	int NumFilters;							// Number of available filter slots (Maximum 12)
	int NumFilterParameters;				// Number of parameters to display in filter wheel setup dialog
	ParamVals FilterParameters[ NUM_FPARAM ]; // Array of filter wheel parameter titles and values
} DialogContents;

// If GetCameraStatus parameter XSize is zero, then this structure is passed back in YSize
typedef struct {
	int Revision;						// Revision level of this structure (zero at this time)
	int XSize;							// Number of rows in physical CCD array (no binning)
	int YSize; 							// Number of columns in physical CCD array (no binning)
	int MaxBinFactorX;					// Maximum binning factor allowed on X axis
	int MaxBinFactorY; 					// Maximum binning factor allowed on Y axis
	IMPBOOL DifferentAxes; 				// Return TRUE if X and Y binning factors can be different
	IMPBOOL HasShutter; 					// Return TRUE if camera has a shutter
	IMPBOOL HasCoolerControl; 			// Return TRUE if camera has a controllable cooler setpoint
	IMPBOOL PowerOfTwoBinning; 			// Return TRUE if user can select 1/2/4/8/16 (max) binning only
	IMPBOOL HasGuiderRelays;				// Return TRUE if camera has built-in autoguider output relays
	IMPBOOL HighResMode;					// Return TRUE if driver can do 0.001 second exposures
	IMPBOOL DualChip;						// Return TRUE if this is a dual chip self-guiding camera
	IMPBOOL LoadTwoCopies;				// Return TRUE if we need two copies of the plug-in to appear
	int SkipWhenScanning;				// Can be used to set a skip border around edge of image for use when scanning for brightest pixel
} CameraInfo;

// Class definition

class CMaxImCCDPlugIn
{
public:
	// GetParameters tells MaxIm CCD what information to ask the user for
	virtual void GetParameters ( 
		DialogContents &Contents,				// Controls the setup dialog box
		IMPBOOL &HasCamera,						// If true, MaxIm CCD will add this to camera and guider lists
		IMPBOOL &HasFilterWheel,					// If true, MaxIm CCD will add camera's internal filter wheel to list
		IMPBOOL &HasGuiderRelays					// If true; MaxIm CCD will use camera's autoguider relays
		) = 0;

	// OpenCamera initializes the link to the camera and allocates buffer memory
	virtual int OpenCamera(
		const char * FilePath,					// File path provided by user (if UseFilePath above is TRUE)
		int Param[ NUM_PARAM ]					// 0-based index for each user parameter input
		) = 0;

	// CloseCamera shuts down the link to the camera and deallocates buffer memory
	virtual void CloseCamera() = 0;

	// GetArraySize returns the number of pixels in X and Y for the CCD array (unbinned)
	virtual void GetArraySize ( 
		int &XSize,								// Number of array columns (X)
		int &YSize								// Number of array rows (Y)
		) = 0;

	// GetImageSize returns the number of pixels in each dimension for the current image, 
	// plus offset value for subframe; includes binning
	virtual void GetImageSize ( 
		int &XStart,							// Column offset for subframe (X) (after binning)
		int &YStart,							// Row offset for subframe (Y) (after binning)
		int &XSize,								// Number of image columns (X)
		int &YSize								// Number of image rows (Y)
		) = 0;

	// GetPixelAspect returns the size of the pixels in microns (micrometers)
	virtual IMPBOOL GetPixelAspect ( 
		double &PixX,							// Pixel physical X dimension in microns
		double &PixY, 							// Pixel physical Y dimension in microns
		IMPBOOL IncludeBinning						// If TRUE, multiply above by binning factors
		) = 0;

	// GetImageBuffer returns a pointer to the image buffer allocated by OpenCamera and filled by TransferImage
	virtual unsigned short * GetImageBuffer() = 0;

	// StartExposures starts a CCD exposure
	virtual int StartExposure ( 
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
		) = 0;

	// AbortExposure aborts the current imaging operation and returns the camera to idle
	// (n.b. If camera hardware needs to be reset, GetCameraState should return CS_FLUSHING until cleared)
	virtual void AbortExposure() = 0;

	// TransferImage downloads the image data from the camera
	virtual int TransferImage ( 
		IMPBOOL &TransferDone,						// May return periodically to update GUI, TRUE when done
		int &PercentDone						// If returns periodically during download, percentage complete
		) = 0;

	// SetTemperature controls the CCD cooler
	virtual IMPBOOL SetTemperature ( 
		IMPBOOL ControlOn,							// If TRUE, controller is turned on, if FALSE, turned off
		IMPBOOL GoToAmbient,						// GoToAmbient should start a gradual warm-up to ambient
		double Temp								// Desired setpoint when ControlOn = TRUE and GoToAmbient = FALSE
		) = 0;

	// GetTemperature is called periodically to get the cooler status and temperature
	virtual int GetTemperature (
		CoolerState &State,						// Current temperature controller status
		double &Temp,							// Temperature of CCD chip (FLT_MAX if not available)
		double &TempAmbient,					// Ambient temperature if available (FLT_MAX if not)
		unsigned short &Power					// Cooler power level in percent (0-100), use > 100 if not available
		) = 0;

	// GetCameraStats is called on initialization to get information about the camera's capabilities
	// NOTE:  If XSize is zero, then YSize is a pointer to an ExtendedStatus structure
	virtual void GetCameraStats(
		int &XSize,								// Number of rows in physical CCD array (no binning)
		int &YSize, 							// Number of columns in physical CCD array (no binning)
		int &MaxBinFactorX,						// Maximum binning factor allowed on X axis
		int &MaxBinFactorY, 					// Maximum binning factor allowed on Y axis
		IMPBOOL &DifferentAxes, 					// Return TRUE if X and Y binning factors can be different
		IMPBOOL &HasShutter, 						// Return TRUE if camera has a shutter
		IMPBOOL &HasCoolerControl, 				// Return TRUE if camera has a controllable cooler setpoint
		IMPBOOL &PowerOfTwoBinning, 				// Return TRUE if user can select 1/2/4/8/16 (max) binning only
		IMPBOOL &HasGuiderRelays					// Return TRUE if camera has built-in autoguider output relays
		) = 0;

	// GetCameraState is called periodically to get the current camera status
	virtual void GetCameraState ( 
		CameraState &State,						// Camera state; lower enum values are higher priority
		IMPBOOL &ShutterOpen						// Return TRUE if shutter is currently open
		) = 0;

	// ActivateRelay turns on an autoguider relay output; only one axis is actuated at a time
	virtual int ActivateRelay ( 
		int X,									// Activate X axis for T = abs ( X/100 ) seconds, direction based on sign
		int Y									// Activate Y axis for T = abs ( X/100 ) seconds, direction based on sign
		) = 0;

	// IsRelayDone returns TRUE if the autoguider relay is inactive, FALSE if active
	virtual IMPBOOL IsRelayDone() = 0;

	// OpenFilterWheel opens a filter wheel attached to the camera
	virtual int OpenFilterWheel(
		int Param[ NUM_FPARAM ]					// Parameters specified in 
		) = 0;

	// CloseFilterWheel disconnects the filter wheel attached to the camera
	virtual void CloseFilterWheel() = 0;

	// SetFilterWheel sets the filter wheel position 
	virtual int SetFilterWheel ( 
		int Position							// 0-based index of filter wheel position
		) = 0;

	// IsFilterWheelMoving returns TRUE if the filter wheel is moving, FALSE if stationary
	// Note:  Some cameras only move their filters when a StartExposure command is issued; in that case,
	// the GetCameraState command must return CS_FILTERWHEELMOVING until the wheel has stopped
	virtual IMPBOOL IsFilterWheelMoving() = 0;
};

/////////////////////////////////////////////////////////////////
// DLL entry points -- Called by MaxIm CCD

#ifdef CCDInternal
#define DLLCALL __declspec (dllimport) 
#else
#define DLLCALL __declspec (dllexport) 
#endif

// Create a new plug-in CCD camera object; called when MaxIm CCD window is initialized
DLLCALL CMaxImCCDPlugIn * NewPlugIn();

// Delete the plug-in CCD camera object; called when MaxIm DL application is shut down
// ALL allocated variables MUST be deleted when this is called
DLLCALL void DeletePlugIn ( CMaxImCCDPlugIn * PlugIn );
