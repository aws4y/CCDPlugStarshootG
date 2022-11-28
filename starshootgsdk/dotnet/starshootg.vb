Imports System.Runtime.InteropServices
Imports Microsoft.Win32.SafeHandles
#If Not (NETFX_CORE OrElse WINDOWS_UWP) Then
Imports System.Security.Permissions
Imports System.Runtime.ConstrainedExecution
#End If
Imports System.Collections.Generic
Imports System.Threading

'    Versin: 50.19915.20211127
'
'    For Microsoft dotNET Framework & dotNet Core
'
'    We use P/Invoke to call into the starshootg.dll API, the VB.net class Starshootg is a thin wrapper class to the native api of starshootg.dll.
'    So the manual en.html and hans.html are also applicable for programming with starshootg.vb.
'    See it in the 'doc' directory:
'       (1) en.html, English
'       (2) hans.html, Simplified Chinese
'
Public Class Starshootg
    Implements IDisposable

#If Not (NETFX_CORE OrElse WINDOWS_UWP) Then
    Public Class SafeCamHandle
        Inherits SafeHandleZeroOrMinusOneIsInvalid
        <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
        Private Shared Sub Starshootg_Close(h As IntPtr)
        End Sub
        
        Public Sub New()
            MyBase.New(True)
        End Sub
        
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)>
        Protected Overrides Function ReleaseHandle() As Boolean
            ' Here, we must obey all rules for constrained execution regions.
            Starshootg_Close(handle)
            Return True
        End Function
    End Class
#Else
    Public Class SafeCamHandle
        Inherits SafeHandle
        <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
        Private Shared Sub Starshootg_Close(h As IntPtr)
        End Sub
        
        Public Sub New()
            MyBase.New(IntPtr.Zero, True)
        End Sub
        
        Protected Overrides Function ReleaseHandle() As Boolean
            Starshootg_Close(handle)
            Return True
        End Function
        
        Public Overrides ReadOnly Property IsInvalid() As Boolean
            Get
                Return MyBase.handle = IntPtr.Zero
            End Get
        End Property
    End Class
#End If
    
    <Flags>
    Public Enum eFLAG As Long
        FLAG_CMOS = &H1                           ' cmos sensor
        FLAG_CCD_PROGRESSIVE = &H2                ' progressive ccd sensor
        FLAG_CCD_INTERLACED = &H4                 ' interlaced ccd sensor
        FLAG_ROI_HARDWARE = &H8                   ' support hardware ROI
        FLAG_MONO = &H10                          ' monochromatic
        FLAG_BINSKIP_SUPPORTED = &H20             ' support bin/skip mode
        FLAG_USB30 = &H40                         ' usb3.0
        FLAG_TEC = &H80                           ' Thermoelectric Cooler
        FLAG_USB30_OVER_USB20 = &H100             ' usb3.0 camera connected to usb2.0 port
        FLAG_ST4 = &H200                          ' ST4
        FLAG_GETTEMPERATURE = &H400               ' support to get the temperature of the sensor
        FLAG_RAW10 = &H1000                       ' pixel format, RAW 10bits
        FLAG_RAW12 = &H2000                       ' pixel format, RAW 12bits
        FLAG_RAW14 = &H4000                       ' pixel format, RAW 14bits
        FLAG_RAW16 = &H8000                       ' pixel format, RAW 16bits
        FLAG_FAN = &H10000                        ' cooling fan
        FLAG_TEC_ONOFF = &H20000                  ' Thermoelectric Cooler can be turn on or off, support to set the target temperature of TEC
        FLAG_ISP = &H40000                        ' ISP (Image Signal Processing) chip
        FLAG_TRIGGER_SOFTWARE = &H80000           ' support software trigger
        FLAG_TRIGGER_EXTERNAL = &H100000          ' support external trigger
        FLAG_TRIGGER_SINGLE = &H200000            ' only support trigger single: one trigger, one image
        FLAG_BLACKLEVEL = &H400000                ' support set and get the black level
        FLAG_AUTO_FOCUS = &H800000                ' support auto focus
        FLAG_BUFFER = &H1000000                   ' frame buffer
        FLAG_DDR = &H2000000                      ' use very large capacity DDR (Double Data Rate SDRAM) for frame buffer. The capacity is not less than one full frame
        FLAG_CG = &H4000000                       ' Conversion Gain: HCG, LCG
        FLAG_YUV411 = &H8000000                   ' pixel format, yuv411
        FLAG_VUYY = &H10000000                    ' pixel format, yuv422, VUYY
        FLAG_YUV444 = &H20000000                  ' pixel format, yuv444
        FLAG_RGB888 = &H40000000                  ' pixel format, RGB888
        <Obsolete("Use FLAG_RAW10")>
        FLAG_BITDEPTH10 = FLAG_RAW10              ' obsolete, same as FLAG_RAW10
        <Obsolete("Use FLAG_RAW12")>
        FLAG_BITDEPTH12 = FLAG_RAW12              ' obsolete, same as FLAG_RAW12
        <Obsolete("Use FLAG_RAW14")>
        FLAG_BITDEPTH14 = FLAG_RAW14              ' obsolete, same as FLAG_RAW14
        <Obsolete("Use FLAG_RAW16")>
        FLAG_BITDEPTH16 = FLAG_RAW16              ' obsolete, same as FLAG_RAW16
        FLAG_RAW8 = &H80000000                    ' pixel format, RAW8
        FLAG_GMCY8 = &H100000000                  ' pixel format, GMCY8
        FLAG_GMCY12 = &H200000000                 ' pixel format, GMCY12
        FLAG_UYVY = &H400000000                   ' pixel format, yuv422, UYVY
        FLAG_CGHDR = &H800000000                  ' Conversion Gain: HCG, LCG, HDR
        FLAG_GLOBALSHUTTER = &H1000000000         ' global shutter
        FLAG_FOCUSMOTOR = &H2000000000            ' support focus motor
        FLAG_PRECISE_FRAMERATE = &H4000000000     ' support precise framerate & bandwidth, see OPTION_PRECISE_FRAMERATE & OPTION_BANDWIDTH
        FLAG_HEAT = &H8000000000                  ' support heat to prevent fogging up
        FLAG_LOW_NOISE = &H10000000000            ' support low noise mode (Higher signal noise ratio, lower frame rate)
        FLAG_LEVELRANGE_HARDWARE = &H20000000000  ' hardware level range, put(get)_LevelRangeV2
        FLAG_EVENT_HARDWARE = &H40000000000       ' hardware event, such as exposure start & stop
        FLAG_LIGHTSOURCE = &H80000000000          ' light source
    End Enum
    
    Public Enum eEVENT As UInteger
        EVENT_EXPOSURE = &H1                      ' exposure time or gain changed
        EVENT_TEMPTINT = &H2                      ' white balance changed, Temp/Tint mode
        EVENT_CHROME = &H3                        ' reversed, do not use it
        EVENT_IMAGE = &H4                         ' live image arrived, use Starshootg_PullImage to get this image
        EVENT_STILLIMAGE = &H5                    ' snap (still) frame arrived, use Starshootg_PullStillImage to get this frame
        EVENT_WBGAIN = &H6                        ' white balance changed, RGB Gain mode
        EVENT_TRIGGERFAIL = &H7                   ' trigger failed
        EVENT_BLACK = &H8                         ' black balance
        EVENT_FFC = &H9                           ' flat field correction status changed
        EVENT_DFC = &Ha                           ' dark field correction status changed
        EVENT_ROI = &Hb                           ' roi changed
        EVENT_LEVELRANGE = &Hc                    ' level range changed
        EVENT_ERROR = &H80                        ' generic error
        EVENT_DISCONNECTED = &H81                 ' camera disconnected
        EVENT_NOFRAMETIMEOUT = &H82               ' no frame timeout error
        EVENT_AFFEEDBACK = &H83                   ' auto focus feedback information
        EVENT_FOCUSPOS = &H84                     ' focus positon
        EVENT_NOPACKETTIMEOUT = &H85              ' no packet timeout
        EVENT_EXPO_START = &H4000                 ' hardware event: exposure start
        EVENT_EXPO_STOP = &H4001                  ' hardware event: exposure stop
        EVENT_TRIGGER_ALLOW = &H4002              ' hardware event: next trigger allow
        EVENT_HEARTBEAT = &H4003                  ' hardware event: heartbeat, can be used to monitor whether the camera is alive
        EVENT_FACTORY = &H8001                    ' restore factory settings
    End Enum
    
    Public Enum eOPTION As UInteger
        OPTION_NOFRAME_TIMEOUT        = &H1        ' no frame timeout: 1 = enable; 0 = disable. default: disable
        OPTION_THREAD_PRIORITY        = &H2        ' set the priority of the internal thread which grab data from the usb device.
                                                   '   Win: iValue: 0 = THREAD_PRIORITY_NORMAL; 1 = THREAD_PRIORITY_ABOVE_NORMAL; 2 = THREAD_PRIORITY_HIGHEST; 3 = THREAD_PRIORITY_TIME_CRITICAL; default: 1; see: https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-setthreadpriority
                                                   '   Linux & macOS: The high 16 bits for the scheduling policy, and the low 16 bits for the priority; see: https://linux.die.net/man/3/pthread_setschedparam
        OPTION_RAW                    = &H4        ' raw data mode, read the sensor "raw" data. This can be set only BEFORE Starshootg_StartXXX(). 0 = rgb, 1 = raw, default value: 0
        OPTION_HISTOGRAM              = &H5        ' 0 = only one, 1 = continue mode
        OPTION_BITDEPTH               = &H6        ' 0 = 8 bits mode, 1 = 16 bits mode
        OPTION_FAN                    = &H7        ' 0 = turn off the cooling fan, [1, max] = fan speed
        OPTION_TEC                    = &H8        ' 0 = turn off the thermoelectric cooler, 1 = turn on the thermoelectric cooler
        OPTION_LINEAR                 = &H9        ' 0 = turn off the builtin linear tone mapping, 1 = turn on the builtin linear tone mapping, default value: 1
        OPTION_CURVE                  = &Ha        ' 0 = turn off the builtin curve tone mapping, 1 = turn on the builtin polynomial curve tone mapping, 2 = logarithmic curve tone mapping, default value: 2
        OPTION_TRIGGER                = &Hb        ' 0 = video mode, 1 = software or simulated trigger mode, 2 = external trigger mode, 3 = external + software trigger, default value = 0
        OPTION_RGB                    = &Hc        ' 0 => RGB24; 1 => enable RGB48 format when bitdepth > 8; 2 => RGB32; 3 => 8 Bits Gray (only for mono camera); 4 => 16 Bits Gray (only for mono camera when bitdepth > 8)
        OPTION_COLORMATIX             = &Hd        ' enable or disable the builtin color matrix, default value: 1
        OPTION_WBGAIN                 = &He        ' enable or disable the builtin white balance gain, default value: 1
        OPTION_TECTARGET              = &Hf        ' get or set the target temperature of the thermoelectric cooler, in 0.1 degree Celsius. For example, 125 means 12.5 degree Celsius, -35 means -3.5 degree Celsius
        OPTION_AUTOEXP_POLICY         = &H10       ' auto exposure policy:
                                                   '       0: Exposure Only
                                                   '       1: Exposure Preferred
                                                   '       2: Gain Only
                                                   '       3: Gain Preferred
                                                   '    default value: 1
                                                   '
        OPTION_FRAMERATE              = &H11       ' limit the frame rate, range=[0, 63], the default value 0 means no limit
        OPTION_DEMOSAIC               = &H12       ' demosaic method for both video and still image: BILINEAR = 0, VNG(Variable Number of Gradients) = 1, PPG(Patterned Pixel Grouping) = 2, AHD(Adaptive Homogeneity Directed) = 3, EA(Edge Aware) = 4, see https://en.wikipedia.org/wiki/Demosaicing, default value: 0
        OPTION_DEMOSAIC_VIDEO         = &H13       ' demosaic method for video
        OPTION_DEMOSAIC_STILL         = &H14       ' demosaic method for still image
        OPTION_BLACKLEVEL             = &H15       ' black level
        OPTION_MULTITHREAD            = &H16       ' multithread image processing
        OPTION_BINNING                = &H17       ' binning, 0x01 (no binning), 0x02 (add, 2*2), 0x03 (add, 3*3), 0x04 (add, 4*4), 0x05 (add, 5*5), 0x06 (add, 6*6), 0x07 (add, 7*7), 0x08 (add, 8*8), 0x82 (average, 2*2), 0x83 (average, 3*3), 0x84 (average, 4*4), 0x85 (average, 5*5), 0x86 (average, 6*6), 0x87 (average, 7*7), 0x88 (average, 8*8). The final image size is rounded down to an even number, such as 640/3 to get 212
        OPTION_ROTATE                 = &H18       ' rotate clockwise: 0, 90, 180, 270
        OPTION_CG                     = &H19       ' Conversion Gain: 0 = LCG, 1 = HCG, 2 = HDR
        OPTION_PIXEL_FORMAT           = &H1a       ' pixel format
        OPTION_FFC                    = &H1b       ' flat field correction
                                                   '    set:
                                                   '         0: disable
                                                   '         1: enable
                                                   '        -1: reset
                                                   '        (0xff000000 | n): average number, [1~255]
                                                   '    get:
                                                   '         (val & 0xff): 0 -> disable, 1 -> enable, 2 -> inited
                                                   '         ((val & 0xff00) >> 8): sequence
                                                   '         ((val & 0xff0000) >> 8): average number
        OPTION_DDR_DEPTH              = &H1c       ' the number of the frames that DDR can cache
                                                   '     1: DDR cache only one frame
                                                   '     0: Auto:
                                                   '         ->one for video mode when auto exposure is enabled
                                                   '         ->full capacity for others
                                                   '    -1: DDR can cache frames to full capacity
       OPTION_DFC                     = &H1d       ' dark field correction
                                                   '    set:
                                                   '         0: disable
                                                   '         1: enable
                                                   '        -1: reset
                                                   '        (0xff000000 | n): average number, [1~255]
                                                   '    get:
                                                   '         (val & 0xff): 0 -> disable, 1 -> enable, 2 -> inited
                                                   '         ((val & 0xff00) >> 8): sequence
                                                   '         ((val & 0xff0000) >> 8): average number
        OPTION_SHARPENING             = &H1e       ' Sharpening: (threshold << 24) | (radius << 16) | strength)
                                                   '    strength: [0, 500], default: 0 (disable)
                                                   '    radius: [1, 10]
                                                   '    threshold: [0, 255]
                                                   '
        OPTION_FACTORY                = &H1f       ' restore the factory settings
        OPTION_TEC_VOLTAGE            = &H20       ' get the current TEC voltage in 0.1V, 59 mean 5.9V; readonly
        OPTION_TEC_VOLTAGE_MAX        = &H21       ' get the TEC maximum voltage in 0.1V; readonly
        OPTION_DEVICE_RESET           = &H22       ' reset usb device, simulate a replug
        OPTION_UPSIDE_DOWN            = &H23       ' upsize down:
                                                   '    1: yes
                                                   '    0: no
                                                   '    default: 1 (win), 0 (linux/macos)
                                                   '
        OPTION_FOCUSPOS               = &H24       ' focus positon
        OPTION_AFMODE                 = &H25       ' auto focus mode (0:manul focus; 1:auto focus; 2:once focus; 3:conjugate calibration)
        OPTION_AFZONE                 = &H26       ' auto focus zone
        OPTION_AFFEEDBACK             = &H27       ' auto focus information feedback; 0:unknown; 1:focused; 2:focusing; 3:defocus; 4:up; 5:down
        OPTION_TESTPATTERN            = &H28       ' test pattern:
                                                   '    0: TestPattern Off
                                                   '    3: monochrome diagonal stripes
                                                   '    5: monochrome vertical stripes
                                                   '    7: monochrome horizontal stripes
                                                   '    9: chromatic diagonal stripes
        OPTION_AUTOEXP_THRESHOLD      = &H29       ' threshold of auto exposure, default value: 5, range = [2, 15]
        OPTION_BYTEORDER              = &H2a       ' Byte order, BGR or RGB: 0->RGB, 1->BGR, default value: 1(Win), 0(macOS, Linux, Android)
        OPTION_NOPACKET_TIMEOUT       = &H2b       ' no packet timeout: 0 = disable, positive value = timeout milliseconds. default: disable
        OPTION_MAX_PRECISE_FRAMERATE  = &H2c       ' get the precise frame rate maximum value in 0.1 fps, such as 115 means 11.5 fps. E_NOTIMPL means not supported
        OPTION_PRECISE_FRAMERATE      = &H2d       ' precise frame rate current value in 0.1 fps, range:[1~maximum]
        OPTION_BANDWIDTH              = &H2e       ' bandwidth, [1-100]%
        OPTION_RELOAD                 = &H2f       ' reload the last frame in trigger mode
        OPTION_CALLBACK_THREAD        = &H30       ' dedicated thread for callback
        OPTION_FRONTEND_DEQUE_LENGTH  = &H31       ' frontend frame buffer deque length, range: [2, 1024], default: 4
        OPTION_FRAME_DEQUE_LENGTH     = &H31       ' alias of STARSHOOTG_OPTION_FRONTEND_DEQUE_LENGTH
        OPTION_MIN_PRECISE_FRAMERATE  = &H32       ' get the precise frame rate minimum value in 0.1 fps, such as 15 means 1.5 fps
        OPTION_SEQUENCER_ONOFF        = &H33       ' sequencer trigger: on/off
        OPTION_SEQUENCER_NUMBER       = &H34       ' sequencer trigger: number, range = [1, 255]
        OPTION_SEQUENCER_EXPOTIME     = &H01000000 ' sequencer trigger: exposure time, iOption = STARSHOOTG_OPTION_SEQUENCER_EXPOTIME | index, iValue = exposure time
                                                   '   For example, to set the exposure time of the third group to 50ms, call:
                                                   '     Starshootg_put_Option(STARSHOOTG_OPTION_SEQUENCER_EXPOTIME | 3, 50000)
        OPTION_SEQUENCER_EXPOGAIN     = &H02000000 ' sequencer trigger: exposure gain, iOption = STARSHOOTG_OPTION_SEQUENCER_EXPOGAIN | index, iValue = gain
        OPTION_DENOISE                = &H35       ' denoise, strength range: [0, 100], 0 means disable
        OPTION_HEAT_MAX               = &H36       ' get maximum level: heat to prevent fogging up
        OPTION_HEAT                   = &H37       ' heat to prevent fogging up
        OPTION_LOW_NOISE              = &H38       ' low noise mode (Higher signal noise ratio, lower frame rate): 1 => enable
        OPTION_POWER                  = &H39       ' get power consumption, unit: milliwatt
        OPTION_GLOBAL_RESET_MODE      = &H3a       ' global reset mode
        OPTION_OPEN_USB_ERRORCODE     = &H3b       ' get the open usb error code
        OPTION_LINUX_USB_ZEROCOPY     = &H3c       ' global option for linux platform:
                                                   '   enable or disable usb zerocopy (helps to reduce memory copy and improve efficiency. Requires kernel version >= 4.6 and hardware platform support)
                                                   '   if the image is wrong, this indicates that the hardware platform does not support this feature, please disable it when the program starts:
                                                   '      Starshootg_put_Option((this is a global option, the camera handle parameter is not required, use nullptr), STARSHOOTG_OPTION_LINUX_USB_ZEROCOPY, 0)
                                                   '   default value:
                                                   '      disable(0): android or arm
                                                   '      enable(1):  others
        OPTION_FLUSH                  = &H3d       ' 1 = hard flush, discard frames cached by camera DDR (if any)
                                                   ' 2 = soft flush, discard frames cached by starshootg.dll (if any)
                                                   ' 3 = both flush
                                                   ' Starshootg_Flush means 'both flush'
        OPTION_NUMBER_DROP_FRAME      = &H3e       ' get the number of frames that have been grabbed from the USB but dropped by the software
        OPTION_DUMP_CFG               = &H3f       ' dump configuration to ini, json, or EEPROM. when camera is closed, it will dump configuration automatically
        OPTION_DEFECT_PIXEL           = &H40       ' Defect Pixel Correction: 0 => disable, 1 => enable; default: 1
        OPTION_BACKEND_DEQUE_LENGTH   = &H41       ' backend frame buffer deque length (Only available in pull mode), range: [2, 1024], default: 3
        OPTION_LIGHTSOURCE_MAX        = &H42       ' get the light source range, [0 ~ max]
        OPTION_LIGHTSOURCE            = &H43       ' light source
        OPTION_HEARTBEAT              = &H44       ' Heartbeat interval in millisecond, range = [HEARTBEAT_MIN, HEARTBEAT_MAX], 0 = disable, default: disable
        OPTION_FRONTEND_DEQUE_CURRENT = &H45       ' get the current number in frontend deque
        OPTION_BACKEND_DEQUE_CURRENT  = &H46       ' get the current number in backend deque
        OPTION_EVENT_HARDWARE         = &H4000000  ' enable or disable hardware event: iOption = OPTION_EVENT_HARDWARE | (event type), iValue = 1 (enable), 0 (disable); default: disable
        OPTION_PACKET_NUMBER          = &H47       ' get the received packet number
    End Enum

    Public Const TEMP_DEF                 = 6503     ' temp, Default
    Public Const TEMP_MIN                 = 2000     ' temp, minimum
    Public Const TEMP_MAX                 = 15000    ' temp, maximum
    Public Const TINT_DEF                 = 1000     ' tint
    Public Const TINT_MIN                 = 200      ' tint
    Public Const TINT_MAX                 = 2500     ' tint
    Public Const HUE_DEF                  = 0        ' hue
    Public Const HUE_MIN                  = -180     ' hue
    Public Const HUE_MAX                  = 180      ' hue
    Public Const SATURATION_DEF           = 128      ' saturation
    Public Const SATURATION_MIN           = 0        ' saturation
    Public Const SATURATION_MAX           = 255      ' saturation
    Public Const BRIGHTNESS_DEF           = 0        ' brightness
    Public Const BRIGHTNESS_MIN           = -64      ' brightness
    Public Const BRIGHTNESS_MAX           = 64       ' brightness
    Public Const CONTRAST_DEF             = 0        ' contrast
    Public Const CONTRAST_MIN             = -100     ' contrast
    Public Const CONTRAST_MAX             = 100      ' contrast
    Public Const GAMMA_DEF                = 100      ' gamma
    Public Const GAMMA_MIN                = 20       ' gamma
    Public Const GAMMA_MAX                = 180      ' gamma
    Public Const AETARGET_DEF             = 120      ' target Of auto exposure
    Public Const AETARGET_MIN             = 16       ' target Of auto exposure
    Public Const AETARGET_MAX             = 220      ' target Of auto exposure
    Public Const WBGAIN_DEF               = 0        ' white balance gain
    Public Const WBGAIN_MIN               = -127     ' white balance gain
    Public Const WBGAIN_MAX               = 127      ' white balance gain
    Public Const BLACKLEVEL_MIN           = 0        ' minimum black level
    Public Const BLACKLEVEL8_MAX          = 31       ' maximum black level For bit depth = 8
    Public Const BLACKLEVEL10_MAX         = 31 * 4   ' maximum black level For bit depth = 10
    Public Const BLACKLEVEL12_MAX         = 31 * 16  ' maximum black level For bit depth = 12
    Public Const BLACKLEVEL14_MAX         = 31 * 64  ' maximum black level For bit depth = 14
    Public Const BLACKLEVEL16_MAX         = 31 * 256 ' maximum black level For bit depth = 16
    Public Const SHARPENING_STRENGTH_DEF  = 0        ' sharpening strength
    Public Const SHARPENING_STRENGTH_MIN  = 0        ' sharpening strength
    Public Const SHARPENING_STRENGTH_MAX  = 500      ' sharpening strength
    Public Const SHARPENING_RADIUS_DEF    = 2        ' sharpening radius
    Public Const SHARPENING_RADIUS_MIN    = 1        ' sharpening radius
    Public Const SHARPENING_RADIUS_MAX    = 10       ' sharpening radius
    Public Const SHARPENING_THRESHOLD_DEF = 0        ' sharpening threshold
    Public Const SHARPENING_THRESHOLD_MIN = 0        ' sharpening threshold
    Public Const SHARPENING_THRESHOLD_MAX = 255      ' sharpening threshold
    Public Const AUTOEXPO_THRESHOLD_DEF   = 5        ' auto exposure threshold
    Public Const AUTOEXPO_THRESHOLD_MIN   = 2        ' auto exposure threshold
    Public Const AUTOEXPO_THRESHOLD_MAX   = 15       ' auto exposure threshold
    Public Const BANDWIDTH_DEF            = 90       ' bandwidth
    Public Const BANDWIDTH_MIN            = 1        ' bandwidth
    Public Const BANDWIDTH_MAX            = 100      ' bandwidth
    Public Const DENOISE_DEF              = 0        ' denoise
    Public Const DENOISE_MIN              = 0        ' denoise
    Public Const DENOISE_MAX              = 100      ' denoise
    Public Const TEC_TARGET_MIN           = -300     ' TEC target: -30.0 degrees Celsius
    Public Const TEC_TARGET_DEF           = 0        ' 0.0 degrees Celsius
    Public Const TEC_TARGET_MAX           = 300      ' TEC target: 30.0 degrees Celsius
    Public Const HEARTBEAT_MIN            = 100      ' millisecond
    Public Const HEARTBEAT_MAX            = 10000    ' millisecond

    Public Enum ePIXELFORMAT As Integer
        PIXELFORMAT_RAW8             = &H0
        PIXELFORMAT_RAW10            = &H1
        PIXELFORMAT_RAW12            = &H2
        PIXELFORMAT_RAW14            = &H3
        PIXELFORMAT_RAW16            = &H4
        PIXELFORMAT_YUV411           = &H5
        PIXELFORMAT_VUYY             = &H6
        PIXELFORMAT_YUV444           = &H7
        PIXELFORMAT_RGB888           = &H8
        PIXELFORMAT_GMCY8            = &H9
        PIXELFORMAT_GMCY12           = &Ha
        PIXELFORMAT_UYVY             = &Hb
    End Enum
    
    Public Enum eFRAMEINFO_FLAG As Integer
        FRAMEINFO_FLAG_SEQ       = &H1                 ' sequence number
        FRAMEINFO_FLAG_TIMESTAMP = &H2
    End Enum
    
    Public Enum eIoControType As Integer
        IOCONTROLTYPE_GET_SUPPORTEDMODE         = &H1  ' 1->Input, 2->Output, (1 | 2)->support both Input and Output
        IOCONTROLTYPE_GET_GPIODIR               = &H3  ' 0x00->Input, 0x01->Output
        IOCONTROLTYPE_SET_GPIODIR               = &H4
        IOCONTROLTYPE_GET_FORMAT                = &H5  ' 0-> not connected
                                                       ' 1-> Tri-state: Tri-state mode (Not driven)
                                                       ' 2-> TTL: TTL level signals
                                                       ' 3-> LVDS: LVDS level signals
                                                       ' 4-> RS422: RS422 level signals
                                                       ' 5-> Opto-coupled'
        IOCONTROLTYPE_SET_FORMAT                = &H6
        IOCONTROLTYPE_GET_OUTPUTINVERTER        = &H7  ' boolean, only support output signal
        IOCONTROLTYPE_SET_OUTPUTINVERTER        = &H8
        IOCONTROLTYPE_GET_INPUTACTIVATION       = &H9  ' 0x00->Positive, 0x01->Negative
        IOCONTROLTYPE_SET_INPUTACTIVATION       = &Ha
        IOCONTROLTYPE_GET_DEBOUNCERTIME         = &Hb  ' debouncer time in microseconds, [0, 20000]
        IOCONTROLTYPE_SET_DEBOUNCERTIME         = &Hc
        IOCONTROLTYPE_GET_TRIGGERSOURCE         = &Hd  ' 0-> Opto-isolated input
                                                       ' 1-> GPIO0
                                                       ' 2-> GPIO1
                                                       ' 3-> Counter
                                                       ' 4-> PWM
                                                       ' 5-> Software
        IOCONTROLTYPE_SET_TRIGGERSOURCE         = &He
        IOCONTROLTYPE_GET_TRIGGERDELAY          = &Hf  ' Trigger delay time in microseconds, [0, 5000000]
        IOCONTROLTYPE_SET_TRIGGERDELAY          = &H10
        IOCONTROLTYPE_GET_BURSTCOUNTER          = &H11 ' Burst Counter, range: [1 ~ 65535]
        IOCONTROLTYPE_SET_BURSTCOUNTER          = &H12
        IOCONTROLTYPE_GET_COUNTERSOURCE         = &H13 ' 0-> Opto-isolated input, 1-> GPIO0, 2-> GPIO1
        IOCONTROLTYPE_SET_COUNTERSOURCE         = &H14
        IOCONTROLTYPE_GET_COUNTERVALUE          = &H15 ' Counter Value, range: [1 ~ 65535]
        IOCONTROLTYPE_SET_COUNTERVALUE          = &H16
        IOCONTROLTYPE_SET_RESETCOUNTER          = &H18
        IOCONTROLTYPE_GET_PWM_FREQ              = &H19
        IOCONTROLTYPE_SET_PWM_FREQ              = &H1a
        IOCONTROLTYPE_GET_PWM_DUTYRATIO         = &H1b
        IOCONTROLTYPE_SET_PWM_DUTYRATIO         = &H1c
        IOCONTROLTYPE_GET_PWMSOURCE             = &H1d ' 0-> Opto-isolated input, 0x01-> GPIO0, 0x02-> GPIO1
        IOCONTROLTYPE_SET_PWMSOURCE             = &H1e
        IOCONTROLTYPE_GET_OUTPUTMODE            = &H1f ' 0-> Frame Trigger Wait
                                                       ' 1-> Exposure Active
                                                       ' 2-> Strobe
                                                       ' 3-> User output
        IOCONTROLTYPE_SET_OUTPUTMODE            = &H20
        IOCONTROLTYPE_GET_STROBEDELAYMODE       = &H21 ' boolean, 1 -> delay, 0 -> pre-delay; compared to exposure active signal
        IOCONTROLTYPE_SET_STROBEDELAYMODE       = &H22
        IOCONTROLTYPE_GET_STROBEDELAYTIME       = &H23 ' Strobe delay or pre-delay time in microseconds, [0, 5000000]
        IOCONTROLTYPE_SET_STROBEDELAYTIME       = &H24
        IOCONTROLTYPE_GET_STROBEDURATION        = &H25 ' Strobe duration time in microseconds, [0, 5000000]
        IOCONTROLTYPE_SET_STROBEDURATION        = &H26
        IOCONTROLTYPE_GET_USERVALUE             = &H27 ' bit0-> Opto-isolated output
                                                       ' bit1-> GPIO0 output
                                                       ' bit2-> GPIO1 output
        IOCONTROLTYPE_SET_USERVALUE             = &H28
        IOCONTROLTYPE_GET_UART_ENABLE           = &H29 ' enable: 1-> on; 0-> off
        IOCONTROLTYPE_SET_UART_ENABLE           = &H2a
        IOCONTROLTYPE_GET_UART_BAUDRATE         = &H2b ' baud rate: 0-> 9600; 1-> 19200; 2-> 38400; 3-> 57600; 4-> 115200
        IOCONTROLTYPE_SET_UART_BAUDRATE         = &H2c
        IOCONTROLTYPE_GET_UART_LINEMODE         = &H2d ' line mode: 0-> TX(GPIO_0)/RX(GPIO_1); 1-> TX(GPIO_1)/RX(GPIO_0)
        IOCONTROLTYPE_SET_UART_LINEMODE         = &H2e
    End Enum

    ' hardware level range mode
    Public Enum eLevelRange As UShort
        LEVELRANGE_MANUAL = &H0     ' manual
        LEVELRANGE_ONCE = &H1       ' once
        LEVELRANGE_CONTINUE = &H2   ' continue
        LEVELRANGE_ROI = &HFFFF     ' update roi rect only
    End Enum

    Public Structure Resolution
        Public width As UInteger
        Public height As UInteger
    End Structure
    Public Structure ModelV2
        Public name As String           ' model name
        Public flag As Long             ' STARSHOOTG_FLAG_xxx, 64 bits
        Public maxspeed As UInteger     ' number of speed level, same as Starshootg_get_MaxSpeed(), the speed range = [0, maxspeed], closed interval
        Public preview As UInteger      ' number of preview resolution, same as Starshootg_get_ResolutionNumber()
        Public still As UInteger        ' number of still resolution, same as get_StillResolutionNumber()
        Public maxfanspeed As UInteger  ' maximum fan speed
        Public ioctrol As UInteger      ' number of input/output control
        Public xpixsz As Single         ' physical pixel size
        Public ypixsz As Single         ' physical pixel size
        Public res As Resolution()
    End Structure
    Public Structure DeviceV2
        Public displayname As String    ' display name
        Public id As String             ' unique and opaque id of a connected camera
        Public model As ModelV2
    End Structure
    <StructLayout(LayoutKind.Sequential)>
    Public Structure FrameInfoV2
        Public width As UInteger
        Public height As UInteger
        Public flag As UInteger         ' FRAMEINFO_FLAG_xxxx
        Public seq As UInteger          ' sequence number
        Public timestamp As ULong       ' microsecond
    End Structure
    <StructLayout(LayoutKind.Sequential)>
    Public Structure AfParam
        Public imax As Integer          ' maximum auto focus sensor board positon
        Public imin As Integer          ' minimum auto focus sensor board positon
        Public idef As Integer          ' conjugate calibration positon
        Public imaxabs As Integer       ' maximum absolute auto focus sensor board positon, micrometer
        Public iminabs As Integer       ' maximum absolute auto focus sensor board positon, micrometer
        Public zoneh As Integer         ' zone horizontal
        Public zonev As Integer         ' zone vertical
    End Structure
    <Obsolete("Use ModelV2")>
    Public Structure Model
        Public name As String
        Public flag As UInteger
        Public maxspeed As UInteger
        Public preview As UInteger
        Public still As UInteger
        Public res As Resolution()
    End Structure
    <Obsolete("Use DeviceV2")>
    Public Structure Device
        Public displayname As String
        Public id As String
        Public model As Model
    End Structure
    
#If Not (NETFX_CORE OrElse WINDOWS_UWP) Then
    <DllImport("kernel32.dll", EntryPoint:="CopyMemory")>
    Public Shared Sub CopyMemory(Destination As IntPtr, Source As IntPtr, Length As IntPtr)
    End Sub
#End If
    
    Public Delegate Sub DelegateEventCallback(nEvent As eEVENT)
    Public Delegate Sub DelegateDataCallbackV3(pData As IntPtr, ByRef info As FrameInfoV2, bSnap As Boolean)
    Public Delegate Sub DelegateHistogramCallback(aHistY As Single(), aHistR As Single(), aHistG As Single(), aHistB As Single())
    Public Delegate Sub DelegateProgressCallback(percent As Integer)

    <UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)>
    Private Delegate Sub EVENT_CALLBACK(nEvent As eEVENT, pCtx As IntPtr)
    <UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)>
    Private Delegate Sub DATA_CALLBACK_V3(pData As IntPtr, pInfo As IntPtr, bSnap As Boolean, pCtx As IntPtr)
    <UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)>
    Private Delegate Sub HISTOGRAM_CALLBACK(aHistY As IntPtr, aHistR As IntPtr, aHistG As IntPtr, aHistB As IntPtr, pCtx As IntPtr)
    <UnmanagedFunctionPointerAttribute(CallingConvention.StdCall)>
    Private Delegate Sub PROGRESS_CALLBACK(percent As Integer, pCtx As IntPtr)

    <StructLayout(LayoutKind.Sequential)>
    Private Structure RECT
        Public left As Integer, top As Integer, right As Integer, bottom As Integer
    End Structure
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_Version() As IntPtr
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall), Obsolete("Use Starshootg_EnumV2")>
    Private Shared Function Starshootg_Enum(ti As IntPtr) As UInteger
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_EnumV2(ti As IntPtr) As UInteger
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_Open(<MarshalAs(UnmanagedType.LPWStr)> id As String) As SafeCamHandle
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_OpenByIndex(index As UInteger) As SafeCamHandle
    End Function
#If Not (NETFX_CORE OrElse WINDOWS_UWP) Then    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_StartPullModeWithWndMsg(h As SafeCamHandle, hWnd As IntPtr, nMsg As UInteger) As Integer
    End Function
#End If 
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_StartPullModeWithCallback(h As SafeCamHandle, pEventCallback As EVENT_CALLBACK, pCallbackCtx As IntPtr) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_PullImage(h As SafeCamHandle, pImageData As IntPtr, bits As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_PullStillImage(h As SafeCamHandle, pImageData As IntPtr, bits As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_PullImageWithRowPitch(h As SafeCamHandle, pImageData As IntPtr, bits As Integer, rowPitch As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_PullStillImageWithRowPitch(h As SafeCamHandle, pImageData As IntPtr, bits As Integer, rowPitch As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_PullImageV2(h As SafeCamHandle, pImageData As IntPtr, bits As Integer, ByRef pInfo As FrameInfoV2) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_PullStillImageV2(h As SafeCamHandle, pImageData As IntPtr, bits As Integer, ByRef pInfo As FrameInfoV2) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_PullImageWithRowPitchV2(h As SafeCamHandle, pImageData As IntPtr, bits As Integer, rowPitch As Integer, ByRef pInfo As FrameInfoV2) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_PullStillImageWithRowPitchV2(h As SafeCamHandle, pImageData As IntPtr, bits As Integer, rowPitch As Integer, ByRef pInfo As FrameInfoV2) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_StartPushModeV3(h As SafeCamHandle, pDataCallback As DATA_CALLBACK_V3, pDataCallbackCtx As IntPtr, pEventCallback As EVENT_CALLBACK, pEventCallbackCtx As IntPtr) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_Stop(h As SafeCamHandle) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_Pause(h As SafeCamHandle, bPause As Integer) As Integer
    End Function
    
    ' for still image snap
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_Snap(h As SafeCamHandle, nResolutionIndex As UInteger) As Integer
    End Function
    ' multiple still image snap
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_SnapN(h As SafeCamHandle, nResolutionIndex As UInteger, nNumber As UInteger) As Integer
    End Function
    
    '
    '    soft trigger:
    '    nNumber:    0xffff:     trigger continuously
    '                0:          cancel trigger
    '                others:     number of images to be triggered
    '
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_Trigger(h As SafeCamHandle, nNumber As UShort) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Size(h As SafeCamHandle, nWidth As Integer, nHeight As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Size(h As SafeCamHandle, ByRef nWidth As Integer, ByRef nHeight As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_eSize(h As SafeCamHandle, nResolutionIndex As UInteger) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_eSize(h As SafeCamHandle, ByRef nResolutionIndex As UInteger) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_FinalSize(h As SafeCamHandle, ByRef nWidth As Integer, ByRef nHeight As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_ResolutionNumber(h As SafeCamHandle) As UInteger
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Resolution(h As SafeCamHandle, nResolutionIndex As UInteger, ByRef pWidth As Integer, ByRef pHeight As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_ResolutionRatio(h As SafeCamHandle, nResolutionIndex As UInteger, ByRef pNumerator As Integer, ByRef pDenominator As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Field(h As SafeCamHandle) As UInteger
    End Function

    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_RawFormat(h As SafeCamHandle, ByRef nFourCC As UInteger, ByRef bitdepth As UInteger) As Integer
    End Function

    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_RealTime(h As SafeCamHandle, val As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_RealTime(h As SafeCamHandle, ByRef val As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_Flush(h As SafeCamHandle) As Integer
    End Function
    
    ' sensor Temperature
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Temperature(h As SafeCamHandle, ByRef pTemperature As Short) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Temperature(h As SafeCamHandle, nTemperature As Short) As Integer
    End Function
    
    ' ROI
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Roi(h As SafeCamHandle, ByRef xOffsett As UInteger, ByRef yOffsett As UInteger, ByRef xWidtht As UInteger, ByRef yHeightt As UInteger) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Roi(h As SafeCamHandle, pxOffset As UInteger, pyOffset As UInteger, pxWidth As UInteger, pyHeight As UInteger) As Integer
    End Function
    
    '
    '  ------------------------------------------------------------------|
    '  | Parameter               |   Range       |   Default             |
    '  |-----------------------------------------------------------------|
    '  | Auto Exposure Target    |   16~235      |   120                 |
    '  | Temp                    |   2000~15000  |   6503                |
    '  | Tint                    |   200~2500    |   1000                |
    '  | LevelRange              |   0~255       |   Low = 0, High = 255 |
    '  | Contrast                |   -100~100    |   0                   |
    '  | Hue                     |   -180~180    |   0                   |
    '  | Saturation              |   0~255       |   128                 |
    '  | Brightness              |   -64~64      |   0                   |
    '  | Gamma                   |   20~180      |   100                 |
    '  | WBGain                  |   -127~127    |   0                   |
    '  ------------------------------------------------------------------|
    '
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_AutoExpoEnable(h As SafeCamHandle, ByRef bAutoExposure As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_AutoExpoEnable(h As SafeCamHandle, bAutoExposure As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_AutoExpoTarget(h As SafeCamHandle, ByRef Target As UShort) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_AutoExpoTarget(h As SafeCamHandle, Target As UShort) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_MaxAutoExpoTimeAGain(h As SafeCamHandle, maxTime As UInteger, maxAGain As UShort) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_MaxAutoExpoTimeAGain(h As SafeCamHandle, ByRef maxTime As UInteger, ByRef maxAGain As UShort) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_MinAutoExpoTimeAGain(h As SafeCamHandle, minTime As UInteger, minAGain As UShort) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_MinAutoExpoTimeAGain(h As SafeCamHandle, ByRef minTime As UInteger, ByRef minAGain As UShort) As Integer
    End Function
    
    ' in microseconds
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_ExpoTime(h As SafeCamHandle, ByRef Time As UInteger) As Integer
    End Function
    
    ' inmicroseconds
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_ExpoTime(h As SafeCamHandle, Time As UInteger) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_ExpTimeRange(h As SafeCamHandle, ByRef nMin As UInteger, ByRef nMax As UInteger, ByRef nDef As UInteger) As Integer
    End Function
    
    ' percent, such as 300 
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_ExpoAGain(h As SafeCamHandle, ByRef AGain As UShort) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_ExpoAGain(h As SafeCamHandle, AGain As UShort) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_ExpoAGainRange(h As SafeCamHandle, ByRef nMin As UShort, ByRef nMax As UShort, ByRef nDef As UShort) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_LevelRange(h As SafeCamHandle, aLow As UShort(),  aHigh As UShort()) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_LevelRange(h As SafeCamHandle, aLow As UShort(), aHigh As UShort()) As Integer
    End Function

    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_LevelRangeV2(h As SafeCamHandle, mode As UShort, ByRef pRoiRect As RECT, aLow As UShort(), aHigh As UShort()) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_LevelRangeV2(h As SafeCamHandle, ByRef pMode As UShort, ByRef pRoiRect As RECT, aLow As UShort(), aHigh As UShort()) As Integer
    End Function

    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Hue(h As SafeCamHandle, Hue As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Hue(h As SafeCamHandle, ByRef Hue As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Saturation(h As SafeCamHandle, Saturation As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Saturation(h As SafeCamHandle, ByRef Saturation As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Brightness(h As SafeCamHandle, Brightness As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Brightness(h As SafeCamHandle, ByRef Brightness As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Contrast(h As SafeCamHandle, ByRef Contrast As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Contrast(h As SafeCamHandle, Contrast As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Gamma(h As SafeCamHandle, ByRef Gamma As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Gamma(h As SafeCamHandle, Gamma As Integer) As Integer
    End Function
    
    ' monochromatic mode
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Chrome(h As SafeCamHandle, ByRef bChrome As Integer) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Chrome(h As SafeCamHandle, bChrome As Integer) As Integer
    End Function
    
    ' vertical flip
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_VFlip(h As SafeCamHandle, ByRef bVFlip As Integer) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_VFlip(h As SafeCamHandle, bVFlip As Integer) As Integer
    End Function
    
    ' horizontal flip
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_HFlip(h As SafeCamHandle, ByRef bHFlip As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_HFlip(h As SafeCamHandle, bHFlip As Integer) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Negative(h As SafeCamHandle, ByRef bNegative As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Negative(h As SafeCamHandle, bNegative As Integer) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Speed(h As SafeCamHandle, nSpeed As UShort) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Speed(h As SafeCamHandle, ByRef pSpeed As UShort) As Integer
    End Function
    
    ' get the maximum speed, "Frame Speed Level", speed range = [0, max]
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_MaxSpeed(h As SafeCamHandle) As UInteger
    End Function
    
    ' get the max bit depth of this camera, such as 8, 10, 12, 14, 16
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_MaxBitDepth(h As SafeCamHandle) As UInteger
    End Function
    
    ' get the maximum fan speed, the fan speed range = [0, max], closed interval
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_FanMaxSpeed(h As SafeCamHandle) As UInteger
    End Function
    
    ' power supply:
    '   0 -> 60HZ AC
    '   1 -> 50Hz AC
    '   2 -> DC
    '
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_HZ(h As SafeCamHandle, nHZ As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_HZ(h As SafeCamHandle, ByRef nHZ As Integer) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Mode(h As SafeCamHandle, bSkip As Integer) As Integer
    End Function
    ' skip or bin
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Mode(h As SafeCamHandle, ByRef bSkip As Integer) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_TempTint(h As SafeCamHandle, nTemp As Integer, nTint As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_TempTint(h As SafeCamHandle, ByRef nTemp As Integer, ByRef nTint As Integer) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_WhiteBalanceGain(h As SafeCamHandle, aGain As Integer()) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_WhiteBalanceGain(h As SafeCamHandle, aGain As Integer()) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_BlackBalance(h As SafeCamHandle, aSub As UShort()) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_BlackBalance(h As SafeCamHandle, aSub As UShort()) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_AWBAuxRect(h As SafeCamHandle, ByRef pAuxRect As RECT) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_AWBAuxRect(h As SafeCamHandle, ByRef pAuxRect As RECT) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_AEAuxRect(h As SafeCamHandle, ByRef pAuxRect As RECT) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_AEAuxRect(h As SafeCamHandle, ByRef pAuxRect As RECT) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_ABBAuxRect(h As SafeCamHandle, ByRef pAuxRect As RECT) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_ABBAuxRect(h As SafeCamHandle, ByRef pAuxRect As RECT) As Integer
    End Function
    
    '
    '  S_FALSE:    color mode
    '  S_OK:       mono mode, such as EXCCD00300KMA and UHCCD01400KMA
    '
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_MonoMode(h As SafeCamHandle) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_StillResolutionNumber(h As SafeCamHandle) As UInteger
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_StillResolution(h As SafeCamHandle, nResolutionIndex As UInteger, ByRef pWidth As Integer, ByRef pHeight As Integer) As Integer
    End Function
    
    '
    ' get the revision
    '
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Revision(h As SafeCamHandle, ByRef pRevision As UShort) As Integer
    End Function
    
    '
    ' get the serial number which is always 32 chars which is zero-terminated such as "TP110826145730ABCD1234FEDC56787"
    '
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_SerialNumber(h As SafeCamHandle, sn As IntPtr) As Integer
    End Function
    
    '
    ' get the firmware version, such as: 3.2.1.20140922
    '
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_FwVersion(h As SafeCamHandle, fwver As IntPtr) As Integer
    End Function
    
    '
    ' get the hardware version, such as: 3.2.1.20140922
    '
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_HwVersion(h As SafeCamHandle, hwver As IntPtr) As Integer
    End Function
    
    '
    ' get FPGA version, such as 1.3
    '
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_FpgaVersion(h As SafeCamHandle, fpgaver As IntPtr) As Integer
    End Function
    
    '
    ' get the production date, such as: 20150327
    '
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_ProductionDate(h As SafeCamHandle, pdate As IntPtr) As Integer
    End Function
    
    '
    ' get the sensor pixel size, such as: 2.4um
    ' 
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_PixelSize(h As SafeCamHandle, nResolutionIndex As UInteger, ByRef x As Single, ByRef y As Single) As Integer
    End Function
    
    '
    '  ------------------------------------------------------------|
    '  | Parameter         |   Range       |   Default             |
    '  |-----------------------------------------------------------|
    '  | VidgetAmount      |   -100~100    |   0                   |
    '  | VignetMidPoint    |   0~100       |   50                  |
    '  -------------------------------------------------------------
    '
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_VignetEnable(h As SafeCamHandle, bEnable As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_VignetEnable(h As SafeCamHandle, ByRef bEnable As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_VignetAmountInt(h As SafeCamHandle, nAmount As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_VignetAmountInt(h As SafeCamHandle, ByRef nAmount As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_VignetMidPointInt(h As SafeCamHandle, nMidPoint As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_VignetMidPointInt(h As SafeCamHandle, ByRef nMidPoint As Integer) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_AwbOnce(h As SafeCamHandle, fnTTProc As IntPtr, pTTCtx As IntPtr) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_AwbInit(h As SafeCamHandle, fnWBProc As IntPtr, pWBCtx As IntPtr) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_LevelRangeAuto(h As SafeCamHandle) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_GetHistogram(h As SafeCamHandle, fnHistogramProc As HISTOGRAM_CALLBACK, pHistogramCtx As IntPtr) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_AbbOnce(h As SafeCamHandle, fnBBProc As IntPtr, pBBCtx As IntPtr) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_LEDState(h As SafeCamHandle, iLed As UShort, iState As UShort, iPeriod As UShort) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_write_EEPROM(h As SafeCamHandle, addr As UInteger, pBuffer As IntPtr, nBufferLen As UInteger) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_read_EEPROM(h As SafeCamHandle, addr As UInteger, pBuffer As IntPtr, nBufferLen As UInteger) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_write_UART(h As SafeCamHandle, pBuffer As IntPtr, nBufferLen As UInteger) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_read_UART(h As SafeCamHandle, pBuffer As IntPtr, nBufferLen As UInteger) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Option(h As SafeCamHandle, iOption As eOPTION, iValue As Integer) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_Option(h As SafeCamHandle, iOption As eOPTION, ByRef iValue As Integer) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Linear(h As SafeCamHandle, v8 As Byte(), v16 As UShort()) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_Curve(h As SafeCamHandle, v8 As Byte(), v16 As UShort()) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_ColorMatrix(h As SafeCamHandle, v As Double()) As Integer
    End Function
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_put_InitWBGain(h As SafeCamHandle, v As UShort()) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_FrameRate(h As SafeCamHandle, ByRef nFrame As UInteger, ByRef nTime As UInteger, ByRef nTotalFrame As UInteger) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_FfcOnce(h As SafeCamHandle) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_DfcOnce(h As SafeCamHandle) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_FfcImport(h As SafeCamHandle, <MarshalAs(UnmanagedType.LPWStr)> filepath As String) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_FfcExport(h As SafeCamHandle, <MarshalAs(UnmanagedType.LPWStr)> filepath As String) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_DfcImport(h As SafeCamHandle, <MarshalAs(UnmanagedType.LPWStr)> filepath As String) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_DfcExport(h As SafeCamHandle, <MarshalAs(UnmanagedType.LPWStr)> filepath As String) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_IoControl(h As SafeCamHandle, index As UInteger, eType As eIoControType, inVal As Integer, ByRef outVal As UInteger) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_get_AfParam(h As SafeCamHandle, ByRef pAfParam As AfParam) As Integer
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_calc_ClarityFactor(pImageData As IntPtr, bits As Integer, nImgWidth As UInteger, nImgHeight As UInteger) As Double
    End Function
    
    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_deBayerV2(nBayer As UInteger, nW As Integer, nH As Integer, input As IntPtr, output As IntPtr, nBitDepth As Byte, nBitCount As Byte)
    End Function

    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_Replug(<MarshalAs(UnmanagedType.LPWStr)> id As String) As Integer
    End Function

    <DllImport("starshootg.dll", ExactSpelling:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function Starshootg_Update(<MarshalAs(UnmanagedType.LPWStr)> camId As String, <MarshalAs(UnmanagedType.LPWStr)> filePath As String, pProgressCallback As PROGRESS_CALLBACK, pCallbackCtx As IntPtr) As Integer
    End Function

    Private Shared _sid As Integer = 0
    Private Shared _map As Dictionary(Of Integer, Object) = New Dictionary(Of Integer, Object)()
    Private _handle As SafeCamHandle
    Private _id As IntPtr
    Private _dDataCallbackV3 As DelegateDataCallbackV3
    Private _dEventCallback As DelegateEventCallback
    Private _dHistogramCallback As DelegateHistogramCallback
    Private _pDataCallbackV3 As DATA_CALLBACK_V3
    Private _pEventCallback As EVENT_CALLBACK
    Private _pHistogramCallback As HISTOGRAM_CALLBACK
    Private _hResult As Integer

    Private Sub EventCallback(nEvent As eEVENT)
        _dEventCallback(nEvent)
    End Sub
    
    Private Sub DataCallbackV3(pData As IntPtr, pInfo As IntPtr, bSnap As Boolean)
        If pData = IntPtr.Zero OrElse pInfo = IntPtr.Zero Then
            ' pData == 0 means that something error, we callback to tell the application 
            If _dDataCallbackV3 IsNot Nothing Then
                Dim info As New FrameInfoV2()
                _dDataCallbackV3(IntPtr.Zero, info, bSnap)
            End If
        Else
#If Not (NETFX_CORE OrElse WINDOWS_UWP) Then
            Dim info As FrameInfoV2 = CType(Marshal.PtrToStructure(pInfo, GetType(FrameInfoV2)), FrameInfoV2)
#Else
            Dim info As FrameInfoV2 = Marshal.PtrToStructure(Of FrameInfoV2)(pInfo)
#End If
            _dDataCallbackV3(pData, info, bSnap)
        End If
    End Sub
    
    Private Sub HistogramCallback(aHistY As Single(), aHistR As Single(), aHistG As Single(), aHistB As Single())
        If _dHistogramCallback IsNot Nothing Then
            _dHistogramCallback(aHistY, aHistR, aHistG, aHistB)
            _dHistogramCallback = Nothing
        End If
        _pHistogramCallback = Nothing
    End Sub
    
    Private Shared Sub DataCallbackV3(pData As IntPtr, pInfo As IntPtr, bSnap As Boolean, pCallbackCtx As IntPtr)
        Dim pthis As Starshootg = Nothing
        _map.TryGetValue(pCallbackCtx.ToInt32(), pthis)
        If pthis IsNot Nothing Then
            pthis.DataCallbackV3(pData, pInfo, bSnap)
        End If
    End Sub
    
    Private Shared Sub EventCallback(nEvent As eEVENT, pCallbackCtx As IntPtr)
        Dim pthis As Starshootg = Nothing
        _map.TryGetValue(pCallbackCtx.ToInt32(), pthis)
        If pthis IsNot Nothing Then
            pthis.EventCallback(nEvent)
        End If
    End Sub
    
    Private Shared Sub HistogramCallback(aHistY As IntPtr, aHistR As IntPtr, aHistG As IntPtr, aHistB As IntPtr, pCallbackCtx As IntPtr)
        Dim pthis As Starshootg = Nothing
        _map.TryGetValue(pCallbackCtx.ToInt32(), pthis)
        If pthis IsNot Nothing Then
            Dim arrHistY As Single() = New Single(255) {}
            Dim arrHistR As Single() = New Single(255) {}
            Dim arrHistG As Single() = New Single(255) {}
            Dim arrHistB As Single() = New Single(255) {}
            Marshal.Copy(aHistY, arrHistY, 0, 256)
            Marshal.Copy(aHistR, arrHistR, 0, 256)
            Marshal.Copy(aHistG, arrHistG, 0, 256)
            Marshal.Copy(aHistB, arrHistB, 0, 256)
            pthis.HistogramCallback(arrHistY, arrHistR, arrHistG, arrHistB)
        End If
    End Sub
    
    Protected Overrides Sub Finalize()
        Try
            Dispose(False)
        Finally
            MyBase.Finalize()
        End Try
    End Sub
    
#If Not (NETFX_CORE OrElse WINDOWS_UWP) Then
    <SecurityPermission(SecurityAction.Demand, UnmanagedCode:=True)>
    Protected Overridable Sub Dispose(disposing As Boolean)
#Else
    Protected Overridable Sub Dispose(disposing As Boolean)
#End If
        ' Note there are three interesting states here:
        ' 1) CreateFile failed, _handle contains an invalid handle
        ' 2) We called Dispose already, _handle is closed.
        ' 3) _handle is null, due to an async exception before
        '    calling CreateFile. Note that the finalizer runs
        '    if the constructor fails.
        If _handle IsNot Nothing AndAlso Not _handle.IsInvalid Then
            ' Free the handle
            _handle.Dispose()
        End If
        ' SafeHandle records the fact that we've called Dispose.
    End Sub
    
    '
    '   the object of Starshootg must be obtained by static mothod Open or OpenByIndex, it cannot be obtained by obj = New Starshootg (The constructor is private on purpose)
    '
    Private Sub New(h As SafeCamHandle)
        _handle = h
        _id = New IntPtr(Interlocked.Increment(_sid))
        _map.Add(_id.ToInt32(), Me)
    End Sub

    Private Function CheckHResult(r As Integer) As Boolean
        _hResult = r
        Return (_hResult >= 0)
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Follow the Dispose pattern - public nonvirtual
        Dispose(True)
        _map.Remove(_id.ToInt32())
        GC.SuppressFinalize(Me)
    End Sub
    
    Public Sub Close()
        Dispose()
    End Sub
    
    ' get the version of this dll, which is: 50.19915.20211127
    Public Shared Function Version() As String
        Return Marshal.PtrToStringUni(Starshootg_Version())
    End Function

    Public Shared Function put_GlobalOption(iOption As eOPTION, iValue As Integer) As Boolean
        Return (Starshootg_put_Option(Nothing, iOption, iValue) >= 0)
    End Function

    Public Shared Function get_GlobalOption(iOption As eOPTION, ByRef iValue As Integer) As Boolean
        Return (Starshootg_get_Option(Nothing, iOption, iValue) >= 0)
    End Function

    ' only for compatibility with .Net 4.0 And below
    Private Shared Function IncIntPtr(p As IntPtr, offset As Integer) As IntPtr
        Return New IntPtr(p.ToInt64() + offset)
    End Function

    ' enumerate Starshootg cameras that are currently connected to computer
    Public Shared Function [EnumV2]() As DeviceV2()
        Dim p As IntPtr = Marshal.AllocHGlobal(512 * 128)
        Dim ti As IntPtr = p
        Dim cnt As UInteger = Starshootg_EnumV2(p)
        Dim arr As DeviceV2() = New DeviceV2(cnt - 1) {}
        If cnt <> 0 Then
            Dim tmp As Single() = New Single(0) {}
            For i As UInteger = 0 To cnt - 1
                arr(i).displayname = Marshal.PtrToStringUni(p)
                p = IncIntPtr(p, 2 * 64)
                arr(i).id = Marshal.PtrToStringUni(p)
                p = IncIntPtr(p, 2 * 64)

                Dim q As IntPtr = Marshal.ReadIntPtr(p)
                p = IncIntPtr(p, IntPtr.Size)

                If True Then
                    arr(i).model.name = Marshal.PtrToStringUni(Marshal.ReadIntPtr(q))
                    q = IncIntPtr(q, IntPtr.Size)
                    If (4 = IntPtr.Size) Then
                        q = IncIntPtr(q, 4)
                    End If
                    arr(i).model.flag = Marshal.ReadInt64(q)
                    q = IncIntPtr(q, 8)
                    arr(i).model.maxspeed = CUInt(Marshal.ReadInt32(q))
                    q = IncIntPtr(q, 4)
                    arr(i).model.preview = CUInt(Marshal.ReadInt32(q))
                    q = IncIntPtr(q, 4)
                    arr(i).model.still = CUInt(Marshal.ReadInt32(q))
                    q = IncIntPtr(q, 4)
                    arr(i).model.maxfanspeed = CUInt(Marshal.ReadInt32(q))
                    q = IncIntPtr(q, 4)
                    arr(i).model.ioctrol = CUInt(Marshal.ReadInt32(q))
                    q = IncIntPtr(q, 4)
                    Marshal.Copy(q, tmp, 0, 1)
                    arr(i).model.xpixsz = tmp(0)
                    q = IncIntPtr(q, 4)
                    Marshal.Copy(q, tmp, 0, 1)
                    arr(i).model.ypixsz = tmp(0)
                    q = IncIntPtr(q, 4)
                    Dim resn As UInteger = Math.Max(arr(i).model.preview, arr(i).model.still)
                    arr(i).model.res = New Resolution(resn - 1) {}
                    For j As UInteger = 0 To resn - 1
                        arr(i).model.res(j).width = CUInt(Marshal.ReadInt32(q))
                        q = IncIntPtr(q, 4)
                        arr(i).model.res(j).height = CUInt(Marshal.ReadInt32(q))
                        q = IncIntPtr(q, 4)
                    Next
                End If
            Next
        End If
        Marshal.FreeHGlobal(ti)
        Return arr
    End Function
    
    <Obsolete("Use EnumV2")>
    Public Shared Function [Enum]() As Device()
        Dim p As IntPtr = Marshal.AllocHGlobal(512 * 128)
        Dim ti As IntPtr = p
        Dim cnt As UInteger = Starshootg_Enum(ti)
        Dim arr As Device() = New Device(cnt - 1) {}
        If cnt <> 0 Then
            For i As UInteger = 0 To cnt - 1
                arr(i).displayname = Marshal.PtrToStringUni(p)
                p = IncIntPtr(p, 2 * 64)
                arr(i).id = Marshal.PtrToStringUni(p)
                p = IncIntPtr(p, 2 * 64)

                Dim q As IntPtr = Marshal.ReadIntPtr(p)
                p = IncIntPtr(p, IntPtr.Size)

                If True Then
                    arr(i).model.name = Marshal.PtrToStringUni(Marshal.ReadIntPtr(q))
                    q = IncIntPtr(q, IntPtr.Size)
                    arr(i).model.flag = CUInt(Marshal.ReadInt32(q))
                    q = IncIntPtr(q, 4)
                    arr(i).model.maxspeed = CUInt(Marshal.ReadInt32(q))
                    q = IncIntPtr(q, 4)
                    arr(i).model.preview = CUInt(Marshal.ReadInt32(q))
                    q = IncIntPtr(q, 4)
                    arr(i).model.still = CUInt(Marshal.ReadInt32(q))
                    q = IncIntPtr(q, 4)

                    Dim resn As UInteger = Math.Max(arr(i).model.preview, arr(i).model.still)
                    arr(i).model.res = New Resolution(resn - 1) {}
                    For j As UInteger = 0 To resn - 1
                        arr(i).model.res(j).width = CUInt(Marshal.ReadInt32(q))
                        q = IncIntPtr(q, 4)
                        arr(i).model.res(j).height = CUInt(Marshal.ReadInt32(q))
                        q = IncIntPtr(q, 4)
                    Next
                End If
            Next
        End If
        Marshal.FreeHGlobal(ti)
        Return arr
    End Function
    
    '
    ' the object of Starshootg must be obtained by static mothod Open or OpenByIndex, it cannot be obtained by obj = New Starshootg (The constructor is private on purpose)
    '
    ' id: enumerated by EnumV2, Nothing means the first camera
    Public Shared Function Open(id As String) As Starshootg
        Dim tmphandle As SafeCamHandle = Starshootg_Open(id)
        If tmphandle Is Nothing OrElse tmphandle.IsInvalid OrElse tmphandle.IsClosed Then
            Return Nothing
        End If
        Return New Starshootg(tmphandle)
    End Function
    
    '
    ' the object of Starshootg must be obtained by static mothod Open or OpenByIndex, it cannot be obtained by obj = New Starshootg (The constructor is private on purpose)
    '
    ' the same with Open, but use the index as the parameter. such as:
    ' index == 0, open the first camera,
    ' index == 1, open the second camera,
    ' etc
    Public Shared Function OpenByIndex(index As UInteger) As Starshootg
        Dim tmphandle As SafeCamHandle = Starshootg_OpenByIndex(index)
        If tmphandle Is Nothing OrElse tmphandle.IsInvalid OrElse tmphandle.IsClosed Then
            Return Nothing
        End If
        Return New Starshootg(tmphandle)
    End Function
    
    Public ReadOnly Property Handle() As SafeCamHandle
        Get
            Return _handle
        End Get
    End Property

    ' the last HRESULT return code of api call
    Public ReadOnly Property HResult() As Integer
        Get
            Return _hResult
        End Get
    End Property

    Public ReadOnly Property ResolutionNumber() As UInteger
        Get
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return 0
            End If
            Return Starshootg_get_ResolutionNumber(_handle)
        End Get
    End Property
    
    Public ReadOnly Property StillResolutionNumber() As UInteger
        Get
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return 0
            End If
            Return Starshootg_get_StillResolutionNumber(_handle)
        End Get
    End Property
    
    Public ReadOnly Property MonoMode() As Boolean
        Get
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return False
            End If
            Return (0 = Starshootg_get_MonoMode(_handle))
        End Get
    End Property
    
    ' get the maximum speed, see "Frame Speed Level"
    Public ReadOnly Property MaxSpeed() As UInteger
        Get
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return 0
            End If
            Return Starshootg_get_MaxSpeed(_handle)
        End Get
    End Property
    
    ' get the max bit depth of this camera, such as 8, 10, 12, 14, 16
    Public ReadOnly Property MaxBitDepth() As UInteger
        Get
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return 0
            End If
            Return Starshootg_get_MaxBitDepth(_handle)
        End Get
    End Property
    
    ' get the maximum fan speed, the fan speed range = [0, max], closed interval
    Public ReadOnly Property FanMaxSpeed() As UInteger
        Get
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return 0
            End If
            Return Starshootg_get_FanMaxSpeed(_handle)
        End Get
    End Property
    
    ' get the revision
    Public ReadOnly Property Revision() As UShort
        Get
            Dim rev As UShort = 0
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return rev
            End If
            Starshootg_get_Revision(_handle, rev)
            Return rev
        End Get
    End Property
    
    ' get the serial number which is always 32 chars which is zero-terminated such as "TP110826145730ABCD1234FEDC56787"
    Public ReadOnly Property SerialNumber() As String
        Get
            Dim str As String = ""
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return str
            End If
            Dim ptr As IntPtr = Marshal.AllocHGlobal(64)
            If Starshootg_get_SerialNumber(_handle, ptr) >= 0 Then
                str = Marshal.PtrToStringAnsi(ptr)
            End If
            Marshal.FreeHGlobal(ptr)
            Return str
        End Get
    End Property
    
    ' get the camera firmware version, such as: 3.2.1.20140922
    Public ReadOnly Property FwVersion() As String
        Get
            Dim str As String = ""
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return str
            End If
            Dim ptr As IntPtr = Marshal.AllocHGlobal(32)
            If Starshootg_get_FwVersion(_handle, ptr) >= 0 Then
                str = ""
            Else
                str = Marshal.PtrToStringAnsi(ptr)
            End If
            Marshal.FreeHGlobal(ptr)
            Return str
        End Get
    End Property
    
    ' get the camera hardware version, such as: 3.2.1.20140922
    Public ReadOnly Property HwVersion() As String
        Get
            Dim str As String = ""
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return str
            End If
            Dim ptr As IntPtr = Marshal.AllocHGlobal(32)
            If Starshootg_get_HwVersion(_handle, ptr) >= 0 Then
                str = ""
            Else
                str = Marshal.PtrToStringAnsi(ptr)
            End If
            Marshal.FreeHGlobal(ptr)
            Return str
        End Get
    End Property
    
    ' get FPGA version, such as: 1.3
    Public ReadOnly Property FpgaVersion() As String
        Get
            Dim str As String = ""
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return str
            End If
            Dim ptr As IntPtr = Marshal.AllocHGlobal(32)
            If Starshootg_get_FpgaVersion(_handle, ptr) >= 0 Then
                str = ""
            Else
                str = Marshal.PtrToStringAnsi(ptr)
            End If
            Marshal.FreeHGlobal(ptr)
            Return str
        End Get
    End Property
    
    ' such as: 20150327
    Public ReadOnly Property ProductionDate() As String
        Get
            Dim str As String = ""
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return str
            End If
            Dim ptr As IntPtr = Marshal.AllocHGlobal(32)
            If Starshootg_get_ProductionDate(_handle, ptr) >= 0 Then
                str = ""
            Else
                str = Marshal.PtrToStringAnsi(ptr)
            End If
            Marshal.FreeHGlobal(ptr)
            Return str
        End Get
    End Property
    
    Public ReadOnly Property Field() As UInteger
        Get
            If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
                Return 0
            End If
            Return Starshootg_get_Field(_handle)
        End Get
    End Property
    
#If Not (NETFX_CORE OrElse WINDOWS_UWP) Then
    Public Function StartPullModeWithWndMsg(hWnd As IntPtr, nMsg As UInteger) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If

        Return CheckHResult(Starshootg_StartPullModeWithWndMsg(_handle, hWnd, nMsg))
    End Function
#End If
    
    Public Function StartPullModeWithCallback(edelegate As DelegateEventCallback) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        _dEventCallback = edelegate
        If edelegate Is Nothing Then
            Return CheckHResult(Starshootg_StartPullModeWithCallback(_handle, Nothing, IntPtr.Zero))
        Else
            _pEventCallback = New EVENT_CALLBACK(AddressOf EventCallback)
            Return CheckHResult(Starshootg_StartPullModeWithCallback(_handle, _pEventCallback, _id))
        End If
    End Function
    
    '  bits: 24 (RGB24), 32 (RGB32), 8 (Gray) or 16 (Gray)
    Public Function PullImage(pImageData As IntPtr, bits As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            pnWidth = pnHeight = 0
            Return False
        End If

        Return CheckHResult(Starshootg_PullImage(_handle, pImageData, bits, pnWidth, pnHeight))
    End Function

    Public Function PullImage(pImageData As Byte(), bits As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Integer
        Dim gch As GCHandle = GCHandle.Alloc(pImageData, GCHandleType.Pinned)
        Try
            Return PullImage(gch.AddrOfPinnedObject(), bits, pnWidth, pnHeight)
        Finally
            gch.Free()
        End Try
    End Function

    Public Function PullImageV2(pImageData As IntPtr, bits As Integer, ByRef pInfo As FrameInfoV2) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            pInfo.width = pInfo.height = pInfo.flag = pInfo.seq = 0
            pInfo.timestamp = 0
            Return False
        End If

        Return CheckHResult(Starshootg_PullImageV2(_handle, pImageData, bits, pInfo))
    End Function

    Public Function PullImageV2(pImageData As Byte(), bits As Integer, ByRef pInfo As FrameInfoV2) As Integer
        Dim gch As GCHandle = GCHandle.Alloc(pImageData, GCHandleType.Pinned)
        Try
            Return PullImageV2(gch.AddrOfPinnedObject(), bits, pInfo)
        Finally
            gch.Free()
        End Try
    End Function

    '  bits: 24 (RGB24), 32 (RGB32), 8 (Gray) or 16 (Gray)
    Public Function PullStillImage(pImageData As IntPtr, bits As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            pnWidth = pnHeight = 0
            Return False
        End If

        Return CheckHResult(Starshootg_PullStillImage(_handle, pImageData, bits, pnWidth, pnHeight))
    End Function

    Public Function PullStillImage(pImageData As Byte(), bits As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Integer
        Dim gch As GCHandle = GCHandle.Alloc(pImageData, GCHandleType.Pinned)
        Try
            Return PullStillImage(gch.AddrOfPinnedObject(), bits, pnWidth, pnHeight)
        Finally
            gch.Free()
        End Try
    End Function

    Public Function PullStillImageV2(pImageData As IntPtr, bits As Integer, ByRef pInfo As FrameInfoV2) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            pInfo.width = pInfo.height = pInfo.flag = pInfo.seq = 0
            pInfo.timestamp = 0
            Return False
        End If

        Return CheckHResult(Starshootg_PullStillImageV2(_handle, pImageData, bits, pInfo))
    End Function

    Public Function PullStillImageV2(pImageData As Byte(), bits As Integer, ByRef pInfo As FrameInfoV2) As Integer
        Dim gch As GCHandle = GCHandle.Alloc(pImageData, GCHandleType.Pinned)
        Try
            Return PullStillImageV2(gch.AddrOfPinnedObject(), bits, pInfo)
        Finally
            gch.Free()
        End Try
    End Function

    ' bits: 24 (RGB24), 32 (RGB32), 48 (RGB48), 8 (Gray) or 16 (Gray). In RAW mode, this parameter is ignored.
    ' rowPitch: The distance from one row to the next row. rowPitch = 0 means using the default row pitch. rowPitch = -1 means zero padding
    ' ----------------------------------------------------------------------------------------------
    ' | format                             | 0 means default row pitch     | -1 means zero padding |
    ' |------------------------------------|-------------------------------|-----------------------|
    ' | RGB       | RGB24                  | TDIBWIDTHBYTES(24 * Width)    | Width * 3             |
    ' |           | RGB32                  | Width * 4                     | Width * 4             |
    ' |           | RGB48                  | TDIBWIDTHBYTES(48 * Width)    | Width * 6             |
    ' |           | GREY8                  | TDIBWIDTHBYTES(8 * Width)     | Width                 |
    ' |           | GREY16                 | TDIBWIDTHBYTES(16 * Width)    | Width * 2             |
    ' |-----------|------------------------|-------------------------------|-----------------------|
    ' | RAW       | 8bits Mode             | Width                         | Width                 |
    ' |           | 10/12/14/16bits Mode   | Width * 2                     | Width * 2             |
    ' |-----------|------------------------|-------------------------------|-----------------------|    
    Public Function PullImageWithRowPitch(pImageData As IntPtr, bits As Integer, rowPitch As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            pnWidth = pnHeight = 0
            Return False
        End If

        Return CheckHResult(Starshootg_PullImageWithRowPitch(_handle, pImageData, bits, rowPitch, pnWidth, pnHeight))
    End Function

    Public Function PullImageWithRowPitch(pImageData As Byte(), bits As Integer, rowPitch As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Integer
        Dim gch As GCHandle = GCHandle.Alloc(pImageData, GCHandleType.Pinned)
        Try
            Return PullImageWithRowPitch(gch.AddrOfPinnedObject(), bits, rowPitch, pnWidth, pnHeight)
        Finally
            gch.Free()
        End Try
    End Function

    Public Function PullImageWithRowPitchV2(pImageData As IntPtr, bits As Integer, rowPitch As Integer, ByRef pInfo As FrameInfoV2) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            pInfo.width = pInfo.height = pInfo.flag = pInfo.seq = 0
            pInfo.timestamp = 0
            Return False
        End If

        Return CheckHResult(Starshootg_PullImageWithRowPitchV2(_handle, pImageData, bits, rowPitch, pInfo))
    End Function

    Public Function PullImageWithRowPitchV2(pImageData As Byte(), bits As Integer, rowPitch As Integer, ByRef pInfo As FrameInfoV2) As Integer
        Dim gch As GCHandle = GCHandle.Alloc(pImageData, GCHandleType.Pinned)
        Try
            Return PullImageWithRowPitchV2(gch.AddrOfPinnedObject(), bits, rowPitch, pInfo)
        Finally
            gch.Free()
        End Try
    End Function

    Public Function PullStillImageWithRowPitch(pImageData As IntPtr, bits As Integer, rowPitch As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            pnWidth = pnHeight = 0
            Return False
        End If

        Return CheckHResult(Starshootg_PullStillImageWithRowPitch(_handle, pImageData, bits, rowPitch, pnWidth, pnHeight))
    End Function

    Public Function PullStillImageWithRowPitch(pImageData As Byte(), bits As Integer, rowPitch As Integer, ByRef pnWidth As UInteger, ByRef pnHeight As UInteger) As Integer
        Dim gch As GCHandle = GCHandle.Alloc(pImageData, GCHandleType.Pinned)
        Try
            Return PullStillImageWithRowPitch(gch.AddrOfPinnedObject(), bits, rowPitch, pnWidth, pnHeight)
        Finally
            gch.Free()
        End Try
    End Function

    Public Function PullStillImageWithRowPitchV2(pImageData As IntPtr, bits As Integer, rowPitch As Integer, ByRef pInfo As FrameInfoV2) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            pInfo.width = pInfo.height = pInfo.flag = pInfo.seq = 0
            pInfo.timestamp = 0
            Return False
        End If

        Return CheckHResult(Starshootg_PullStillImageWithRowPitchV2(_handle, pImageData, bits, rowPitch, pInfo))
    End Function

    Public Function PullStillImageWithRowPitchV2(pImageData As Byte(), bits As Integer, rowPitch As Integer, ByRef pInfo As FrameInfoV2) As Integer
        Dim gch As GCHandle = GCHandle.Alloc(pImageData, GCHandleType.Pinned)
        Try
            Return PullStillImageWithRowPitchV2(gch.AddrOfPinnedObject(), bits, rowPitch, pInfo)
        Finally
            gch.Free()
        End Try
    End Function

    Public Function StartPushModeV3(ddelegate As DelegateDataCallbackV3, edelegate As DelegateEventCallback) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        _dDataCallbackV3 = ddelegate
        _dEventCallback = edelegate
        _pDataCallbackV3 = New DATA_CALLBACK_V3(AddressOf DataCallbackV3)
        _pEventCallback = New EVENT_CALLBACK(AddressOf EventCallback)
        Return CheckHResult(Starshootg_StartPushModeV3(_handle, _pDataCallbackV3, _id, _pEventCallback, _id))
    End Function
    
    Public Function [Stop]() As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_Stop(_handle))
    End Function
    
    Public Function Pause(bPause As Boolean) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_Pause(_handle, If(bPause, 1, 0)))
    End Function
    
    Public Function Snap(nResolutionIndex As UInteger) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_Snap(_handle, nResolutionIndex))
    End Function
    
    ' multiple still image snap
    Public Function SnapN(nResolutionIndex As UInteger, nNumber As UInteger) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_SnapN(_handle, nResolutionIndex, nNumber))
    End Function
    
    '
    '    soft trigger:
    '    nNumber:    0xffff:     trigger continuously
    '                0:          cancel trigger
    '                others:     number of images to be triggered
    '
    Public Function Trigger(nNumber As UShort) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_Trigger(_handle, nNumber))
    End Function
    
    '
    '  put_Size, put_eSize, can be used to set the video output resolution BEFORE Start.
    '  put_Size use width and height parameters, put_eSize use the index parameter.
    '  for example, UCMOS03100KPA support the following resolutions:
    '      index 0:    2048,   1536
    '      index 1:    1024,   768
    '      index 2:    680,    510
    '  so, we can use put_Size(h, 1024, 768) or put_eSize(h, 1). Both have the same effect.
    ' 
    Public Function put_Size(nWidth As Integer, nHeight As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Size(_handle, nWidth, nHeight))
    End Function
    
    Public Function get_Size(ByRef nWidth As Integer, ByRef nHeight As Integer) As Boolean
        nWidth = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_Size(_handle, nWidth, nHeight))
    End Function
    
    Public Function put_eSize(nResolutionIndex As UInteger) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_eSize(_handle, nResolutionIndex))
    End Function
    
    Public Function get_eSize(ByRef nResolutionIndex As UInteger) As Boolean
        nResolutionIndex = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_eSize(_handle, nResolutionIndex))
    End Function
    
    '
    ' final size after ROI, rotate, binning
    '
    Public Function get_FinalSize(ByRef nWidth As Integer, ByRef nHeight As Integer) As Boolean
        nWidth = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_FinalSize(_handle, nWidth, nHeight))
    End Function
    
    Public Function get_Resolution(nResolutionIndex As UInteger, ByRef pWidth As Integer, ByRef pHeight As Integer) As Boolean
        pWidth = pHeight = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_Resolution(_handle, nResolutionIndex, pWidth, pHeight))
    End Function
    
    '
    ' get the sensor pixel size, such as: 2.4um
    '
    Public Function get_PixelSize(nResolutionIndex As UInteger, ByRef x As Single, ByRef y As Single) As Boolean
        x = y = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_PixelSize(_handle, nResolutionIndex, x, y))
    End Function
    
    '
    ' numerator/denominator, such as: 1/1, 1/2, 1/3
    '
    Public Function get_ResolutionRatio(nResolutionIndex As UInteger, ByRef pNumerator As Integer, ByRef pDenominator As Integer) As Boolean
        pNumerator = pDenominator = 1
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_ResolutionRatio(_handle, nResolutionIndex, pNumerator, pDenominator))
    End Function
    
    '
    ' see: http://www.fourcc.org
    ' FourCC:
    '     MAKEFOURCC('G', 'B', 'R', 'G'), see http://www.siliconimaging.com/RGB%20Bayer.htm
    '     MAKEFOURCC('R', 'G', 'G', 'B')
    '     MAKEFOURCC('B', 'G', 'G', 'R')
    '     MAKEFOURCC('G', 'R', 'B', 'G')
    '     MAKEFOURCC('Y', 'Y', 'Y', 'Y'), monochromatic sensor
    '     MAKEFOURCC('Y', '4', '1', '1'), yuv411
    '     MAKEFOURCC('V', 'U', 'Y', 'Y'), yuv422
    '     MAKEFOURCC('U', 'Y', 'V', 'Y'), yuv422
    '     MAKEFOURCC('Y', '4', '4', '4'), yuv444
    '     MAKEFOURCC('R', 'G', 'B', '8'), RGB888
    '
    Public Function get_RawFormat(ByRef nFourCC As UInteger, ByRef bitdepth As UInteger) As Boolean
        nFourCC = bitdepth = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_RawFormat(_handle, nFourCC, bitdepth))
    End Function
    
    ' 0: stop grab frame when frame buffer deque is full, until the frames in the queue are pulled away and the queue is not full
    ' 1: realtime
    '       use minimum frame buffer. When new frame arrive, drop all the pending frame regardless of whether the frame buffer is full.
    '       If DDR present, also limit the DDR frame buffer to only one frame.
    ' 2: soft realtime
    '       Drop the oldest frame when the queue is full and then enqueue the new frame
    ' default: 0
    '
    Public Function put_RealTime(val As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_RealTime(_handle, val))
    End Function
    
    Public Function get_RealTime(ByRef val As Integer) As Boolean
        val = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_RealTime(_handle, val))
    End Function
    
    ' Flush is obsolete, it's a synonyms for put_Option(OPTION_FLUSH, 3)
    Public Function Flush() As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_Flush(_handle))
    End Function
    
    Public Function get_AutoExpoEnable(ByRef bAutoExposure As Boolean) As Boolean
        bAutoExposure = False
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim iEnable As Integer = 0
        If Not CheckHResult(Starshootg_get_AutoExpoEnable(_handle, iEnable)) Then
            Return False
        End If

        bAutoExposure = (iEnable <> 0)
        Return True
    End Function
    
    Public Function put_AutoExpoEnable(bAutoExposure As Boolean) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_AutoExpoEnable(_handle, If(bAutoExposure, 1, 0)))
    End Function
    
    Public Function get_AutoExpoTarget(ByRef Target As UShort) As Boolean
        Target = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_AutoExpoTarget(_handle, Target))
    End Function
    
    Public Function put_AutoExpoTarget(Target As UShort) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_AutoExpoTarget(_handle, Target))
    End Function
    
    Public Function put_MaxAutoExpoTimeAGain(maxTime As UInteger, maxAGain As UShort) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_MaxAutoExpoTimeAGain(_handle, maxTime, maxAGain))
    End Function
    
    Public Function get_MinAutoExpoTimeAGain(ByRef minTime As UInteger, ByRef minAGain As UShort) As Boolean
        minTime = 0
        minAGain = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_MinAutoExpoTimeAGain(_handle, minTime, minAGain))
    End Function
    
    Public Function put_MinAutoExpoTimeAGain(minTime As UInteger, minAGain As UShort) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_MinAutoExpoTimeAGain(_handle, minTime, minAGain))
    End Function
    
    Public Function get_MaxAutoExpoTimeAGain(ByRef maxTime As UInteger, ByRef maxAGain As UShort) As Boolean
        maxTime = 0
        maxAGain = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_MaxAutoExpoTimeAGain(_handle, maxTime, maxAGain))
    End Function
    
    Public Function get_ExpoTime(ByRef Time As UInteger) As Boolean
        ' in microseconds
        Time = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_ExpoTime(_handle, Time))
    End Function
    
    Public Function put_ExpoTime(Time As UInteger) As Boolean
        ' in microseconds
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_ExpoTime(_handle, Time))
    End Function
    
    Public Function get_ExpTimeRange(ByRef nMin As UInteger, ByRef nMax As UInteger, ByRef nDef As UInteger) As Boolean
        nMin = nMax = nDef = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_ExpTimeRange(_handle, nMin, nMax, nDef))
    End Function
    
    Public Function get_ExpoAGain(ByRef AGain As UShort) As Boolean
        ' percent, such as 300 
        AGain = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_ExpoAGain(_handle, AGain))
    End Function
    
    Public Function put_ExpoAGain(AGain As UShort) As Boolean
        ' percent 
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_ExpoAGain(_handle, AGain))
    End Function
    
    Public Function get_ExpoAGainRange(ByRef nMin As UShort, ByRef nMax As UShort, ByRef nDef As UShort) As Boolean
        nMin = nMax = nDef = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_ExpoAGainRange(_handle, nMin, nMax, nDef))
    End Function
    
    Public Function put_LevelRange(aLow As UShort(), aHigh As UShort()) As Boolean
        If aLow.Length <> 4 OrElse aHigh.Length <> 4 Then
            Return False
        End If
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_LevelRange(_handle, aLow, aHigh))
    End Function
    
    Public Function get_LevelRange(aLow As UShort(), aHigh As UShort()) As Boolean
        If aLow.Length <> 4 OrElse aHigh.Length <> 4 Then
            Return False
        End If
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_LevelRange(_handle, aLow, aHigh))
    End Function

    Public Function put_LevelRangeV2(mode As UShort, roiX As Integer, roiY As Integer, roiWidth As Integer, roiHeight As Integer, aLow As UShort(), aHigh As UShort()) As Boolean
        If aLow.Length <> 4 OrElse aHigh.Length <> 4 Then
            Return False
        End If
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Dim rc As New RECT()
        rc.left = roiX
        rc.right = roiX + roiWidth
        rc.top = roiY
        rc.bottom = roiY + roiHeight
        Return CheckHResult(Starshootg_put_LevelRangeV2(_handle, mode, rc, aLow, aHigh))
    End Function

    Public Function get_LevelRangeV2(mode As UShort, ByRef roiX As Integer, ByRef roiY As Integer, ByRef roiWidth As Integer, ByRef roiHeight As Integer, aLow As UShort(), aHigh As UShort()) As Boolean
        mode = 0
        roiX = roiY = roiWidth = roiHeight = 0
        If aLow.Length <> 4 OrElse aHigh.Length <> 4 Then
            Return False
        End If
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If

        Dim rc As New RECT()
        If Not CheckHResult(Starshootg_get_LevelRangeV2(_handle, mode, rc, aLow, aHigh)) Then
            Return False
        End If

        roiX = rc.left
        roiY = rc.top
        roiWidth = rc.right - rc.left
        roiHeight = rc.bottom - rc.top
        Return True
    End Function

    Public Function put_Hue(Hue As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Hue(_handle, Hue))
    End Function
    
    Public Function get_Hue(ByRef Hue As Integer) As Boolean
        Hue = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_Hue(_handle, Hue))
    End Function
    
    Public Function put_Saturation(Saturation As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Saturation(_handle, Saturation))
    End Function
    
    Public Function get_Saturation(ByRef Saturation As Integer) As Boolean
        Saturation = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_Saturation(_handle, Saturation))
    End Function
    
    Public Function put_Brightness(Brightness As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Brightness(_handle, Brightness))
    End Function
    
    Public Function get_Brightness(ByRef Brightness As Integer) As Boolean
        Brightness = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_Brightness(_handle, Brightness))
    End Function
    
    Public Function get_Contrast(ByRef Contrast As Integer) As Boolean
        Contrast = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_Contrast(_handle, Contrast))
    End Function
    
    Public Function put_Contrast(Contrast As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Contrast(_handle, Contrast))
    End Function
    
    Public Function get_Gamma(ByRef Gamma As Integer) As Boolean
        Gamma = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_Gamma(_handle, Gamma))
    End Function
    
    Public Function put_Gamma(Gamma As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Gamma(_handle, Gamma))
    End Function
    
    Public Function get_Chrome(ByRef bChrome As Boolean) As Boolean
        ' monochromatic mode 
        bChrome = False
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim iEnable As Integer = 0
        If Not CheckHResult(Starshootg_get_Chrome(_handle, iEnable)) Then
            Return False
        End If

        bChrome = (iEnable <> 0)
        Return True
    End Function
    
    Public Function put_Chrome(bChrome As Boolean) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Chrome(_handle, If(bChrome, 1, 0)))
    End Function
    
    Public Function get_VFlip(ByRef bVFlip As Boolean) As Boolean
        ' vertical flip 
        bVFlip = False
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim iVFlip As Integer = 0
        If Not CheckHResult(Starshootg_get_VFlip(_handle, iVFlip)) Then
            Return False
        End If

        bVFlip = (iVFlip <> 0)
        Return True
    End Function
    
    Public Function put_VFlip(bVFlip As Boolean) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_VFlip(_handle, If(bVFlip, 1, 0)))
    End Function
    
    Public Function get_HFlip(ByRef bHFlip As Boolean) As Boolean
        bHFlip = False
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim iHFlip As Integer = 0
        If Not CheckHResult(Starshootg_get_HFlip(_handle, iHFlip)) Then
            Return False
        End If

        bHFlip = (iHFlip <> 0)
        Return True
    End Function
    
    Public Function put_HFlip(bHFlip As Boolean) As Boolean
        ' horizontal flip 
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_HFlip(_handle, If(bHFlip, 1, 0)))
    End Function
    
    ' negative film
    Public Function get_Negative(ByRef bNegative As Boolean) As Boolean
        bNegative = False
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim iNegative As Integer = 0
        If Not CheckHResult(Starshootg_get_Negative(_handle, iNegative)) Then
            Return False
        End If

        bNegative = (iNegative <> 0)
        Return True
    End Function
    
    ' negative film
    Public Function put_Negative(bNegative As Boolean) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Negative(_handle, If(bNegative, 1, 0)))
    End Function
    
    Public Function put_Speed(nSpeed As UShort) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Speed(_handle, nSpeed))
    End Function
    
    Public Function get_Speed(ByRef pSpeed As UShort) As Boolean
        pSpeed = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_Speed(_handle, pSpeed))
    End Function
    
    Public Function put_HZ(nHZ As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_HZ(_handle, nHZ))
    End Function
    
    Public Function get_HZ(ByRef nHZ As Integer) As Boolean
        nHZ = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_HZ(_handle, nHZ))
    End Function
    
    Public Function put_Mode(bSkip As Boolean) As Boolean
        ' skip or bin 
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Mode(_handle, If(bSkip, 1, 0)))
    End Function
    
    Public Function get_Mode(ByRef bSkip As Boolean) As Boolean
        bSkip = False
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim iSkip As Integer = 0
        If Not CheckHResult(Starshootg_get_Mode(_handle, iSkip)) Then
            Return False
        End If

        bSkip = (iSkip <> 0)
        Return True
    End Function
    
    ' White Balance, Temp/Tint mode
    Public Function put_TempTint(nTemp As Integer, nTint As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_TempTint(_handle, nTemp, nTint))
    End Function
    
    ' White Balance, Temp/Tint mode
    Public Function get_TempTint(ByRef nTemp As Integer, ByRef nTint As Integer) As Boolean
        nTemp = nTint = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_TempTint(_handle, nTemp, nTint))
    End Function
    
    ' White Balance, RGB Gain Mode
    Public Function put_WhiteBalanceGain(aGain As Integer()) As Boolean
        If aGain.Length <> 3 Then
            Return False
        End If
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_WhiteBalanceGain(_handle, aGain))
    End Function
    
    ' White Balance, RGB Gain Mode
    Public Function get_WhiteBalanceGain(aGain As Integer()) As Boolean
        If aGain.Length <> 3 Then
            Return False
        End If
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_WhiteBalanceGain(_handle, aGain))
    End Function
    
    Public Function put_AWBAuxRect(X As Integer, Y As Integer, Width As Integer, Height As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim rc As New RECT()
        rc.left = X
        rc.right = X + Width
        rc.top = Y
        rc.bottom = Y + Height
        Return CheckHResult(Starshootg_put_AWBAuxRect(_handle, rc))
    End Function
    
    Public Function get_AWBAuxRect(ByRef X As Integer, ByRef Y As Integer, ByRef Width As Integer, ByRef Height As Integer) As Boolean
        X = Y = Width = Height = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim rc As New RECT()
        If Not CheckHResult(Starshootg_get_AWBAuxRect(_handle, rc)) Then
            Return False
        End If

        X = rc.left
        Y = rc.top
        Width = rc.right - rc.left
        Height = rc.bottom - rc.top
        Return True
    End Function
    
    Public Function put_BlackBalance(aSub As UShort()) As Boolean
        If aSub.Length <> 3 Then
            Return False
        End If
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_BlackBalance(_handle, aSub))
    End Function
    
    Public Function get_BlackBalance(aSub As UShort()) As Boolean
        If aSub.Length <> 3 Then
            Return False
        End If
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_BlackBalance(_handle, aSub))
    End Function
    
    Public Function put_ABBAuxRect(X As Integer, Y As Integer, Width As Integer, Height As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim rc As New RECT()
        rc.left = X
        rc.right = X + Width
        rc.top = Y
        rc.bottom = Y + Height
        Return CheckHResult(Starshootg_put_ABBAuxRect(_handle, rc))
    End Function
    
    Public Function get_ABBAuxRect(ByRef X As Integer, ByRef Y As Integer, ByRef Width As Integer, ByRef Height As Integer) As Boolean
        X = Y = Width = Height = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim rc As New RECT()
        If Not CheckHResult(Starshootg_get_ABBAuxRect(_handle, rc)) Then
            Return False
        End If

        X = rc.left
        Y = rc.top
        Width = rc.right - rc.left
        Height = rc.bottom - rc.top
        Return True
    End Function
    
    Public Function put_AEAuxRect(X As Integer, Y As Integer, Width As Integer, Height As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim rc As New RECT()
        rc.left = X
        rc.right = X + Width
        rc.top = Y
        rc.bottom = Y + Height
        Return CheckHResult(Starshootg_put_AEAuxRect(_handle, rc))
    End Function
    
    Public Function get_AEAuxRect(ByRef X As Integer, ByRef Y As Integer, ByRef Width As Integer, ByRef Height As Integer) As Boolean
        X = Y = Width = Height = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim rc As New RECT()
        If Not CheckHResult(Starshootg_get_AEAuxRect(_handle, rc)) Then
            Return False
        End If

        X = rc.left
        Y = rc.top
        Width = rc.right - rc.left
        Height = rc.bottom - rc.top
        Return True
    End Function
    
    Public Function get_StillResolution(nResolutionIndex As UInteger, ByRef pWidth As Integer, ByRef pHeight As Integer) As Boolean
        pWidth = pHeight = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_StillResolution(_handle, nResolutionIndex, pWidth, pHeight))
    End Function
    
    Public Function put_VignetEnable(bEnable As Boolean) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_VignetEnable(_handle, If(bEnable, 1, 0)))
    End Function
    
    Public Function get_VignetEnable(ByRef bEnable As Boolean) As Boolean
        bEnable = False
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        
        Dim iEanble As Integer = 0
        If Not CheckHResult(Starshootg_get_VignetEnable(_handle, iEanble)) Then
            Return False
        End If

        bEnable = (iEanble <> 0)
        Return True
    End Function
    
    Public Function put_VignetAmountInt(nAmount As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_VignetAmountInt(_handle, nAmount))
    End Function
    
    Public Function get_VignetAmountInt(ByRef nAmount As Integer) As Boolean
        nAmount = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_VignetAmountInt(_handle, nAmount))
    End Function
    
    Public Function put_VignetMidPointInt(nMidPoint As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_VignetMidPointInt(_handle, nMidPoint))
    End Function
    
    Public Function get_VignetMidPointInt(ByRef nMidPoint As Integer) As Boolean
        nMidPoint = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_VignetMidPointInt(_handle, nMidPoint))
    End Function
    
    Public Function LevelRangeAuto() As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_LevelRangeAuto(_handle))
    End Function
    
    ' led state:
    '    iLed: Led index, (0, 1, 2, ...)
    '    iState: 1 -> Ever bright; 2 -> Flashing; other -> Off
    '    iPeriod: Flashing Period (>= 500ms)
    Public Function put_LEDState(iLed As UShort, iState As UShort, iPeriod As UShort) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_LEDState(_handle, iLed, iState, iPeriod))
    End Function
    
    Public Function write_EEPROM(addr As UInteger, pBuffer As IntPtr, nBufferLen As UInteger) As Integer
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return 0
        End If
        Return Starshootg_write_EEPROM(_handle, addr, pBuffer, nBufferLen)
    End Function
    
    Public Function read_EEPROM(addr As UInteger, pBuffer As IntPtr, nBufferLen As UInteger) As Integer
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return 0
        End If
        Return Starshootg_read_EEPROM(_handle, addr, pBuffer, nBufferLen)
    End Function
    
    Public Function write_UART(pBuffer As IntPtr, nBufferLen As UInteger) As Integer
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return 0
        End If
        Return Starshootg_write_UART(_handle, pBuffer, nBufferLen)
    End Function
    
    Public Function read_UART(pBuffer As IntPtr, nBufferLen As UInteger) As Integer
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return 0
        End If
        Return Starshootg_read_UART(_handle, pBuffer, nBufferLen)
    End Function
    
    Public Function put_Option(iOption As eOPTION, iValue As Integer) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Option(_handle, iOption, iValue))
    End Function
    
    Public Function get_Option(iOption As eOPTION, ByRef iValue As Integer) As Boolean
        iValue = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_Option(_handle, iOption, iValue))
    End Function
    
    Public Function put_Linear(v8 As Byte(), v16 As UShort()) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Linear(_handle, v8, v16))
    End Function
    
    Public Function put_Curve(v8 As Byte(), v16 As UShort()) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Curve(_handle, v8, v16))
    End Function
    
    Public Function put_ColorMatrix(v As Double()) As Boolean
        If v.Length <> 9 Then
            Return False
        End If
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_ColorMatrix(_handle, v))
    End Function
    
    Public Function put_InitWBGain(v As UShort()) As Boolean
        If v.Length <> 3 Then
            Return False
        End If
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_InitWBGain(_handle, v))
    End Function
    
    ' get the temperature of the sensor, in 0.1 degrees Celsius (32 means 3.2 degrees Celsius, -35 means -3.5 degree Celsius)
    Public Function get_Temperature(ByRef pTemperature As Short) As Boolean
        pTemperature = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_Temperature(_handle, pTemperature))
    End Function
    
    ' set the target temperature of the sensor or TEC, in 0.1 degrees Celsius (32 means 3.2 degrees Celsius, -35 means -3.5 degree Celsius)
    Public Function put_Temperature(nTemperature As Short) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Temperature(_handle, nTemperature))
    End Function
    
    Public Function get_Roi(ByRef pxOffset As UInteger, ByRef pyOffset As UInteger, ByRef pxWidth As UInteger, ByRef pyHeight As UInteger) As Boolean
        pxOffset = pyOffset = pxWidth = pyHeight = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_Roi(_handle, pxOffset, pyOffset, pxWidth, pyHeight))
    End Function
    
    '
    ' xOffset, yOffset, xWidth, yHeight: must be even numbers
    '
    Public Function put_Roi(xOffset As UInteger, yOffset As UInteger, xWidth As UInteger, yHeight As UInteger) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_put_Roi(_handle, xOffset, yOffset, xWidth, yHeight))
    End Function
    
    ' get the frame rate: framerate (fps) = Frame * 1000.0 / nTime
    Public Function get_FrameRate(ByRef nFrame As UInteger, ByRef nTime As UInteger, ByRef nTotalFrame As UInteger) As Boolean
        nFrame = nTime = nTotalFrame = 0
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_FrameRate(_handle, nFrame, nTime, nTotalFrame))
    End Function
    
    ' Auto White Balance "Once", Temp/Tint Mode
    Public Function AwbOnce() As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_AwbOnce(_handle, IntPtr.Zero, IntPtr.Zero))
    End Function

    <Obsolete("Use AwbOnce")>
    Public Function AwbOnePush() As Boolean
        Return AwbOnce()
    End Function
    
    ' Auto White Balance, RGB Gain Mode
    Public Function AwbInit() As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_AwbInit(_handle, IntPtr.Zero, IntPtr.Zero))
    End Function
    
    Public Function AbbOnce() As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_AbbOnce(_handle, IntPtr.Zero, IntPtr.Zero))
    End Function
    
    <Obsolete("Use AbbOnce")>
    Public Function AbbOnePush() As Boolean
        Return AbbOnce()
    End Function
    
    Public Function FfcOnce() As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_FfcOnce(_handle))
    End Function

    <Obsolete("Use FfcOnce")>
    Public Function FfcOnePush() As Boolean
        Return FfcOnce()
    End Function
    
    Public Function DfcOnce() As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_DfcOnce(_handle))
    End Function
    
    <Obsolete("Use DfcOnce")>
    Public Function DfcOnePush() As Boolean
        Return DfcOnce()
    End Function
    
    Public Function FfcExport(filepath As String) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_FfcExport(_handle, filepath))
    End Function
    
    Public Function FfcImport(filepath As String) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_FfcImport(_handle, filepath))
    End Function
    
    Public Function DfcExport(filepath As String) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_DfcExport(_handle, filepath))
    End Function
    
    Public Function DfcImport(filepath As String) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_DfcImport(_handle, filepath))
    End Function
    
    Public Function IoControl(ioLineNumber As UInteger, eType As eIoControType, inVal As Integer, ByRef outVal As UInteger) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_IoControl(_handle, ioLineNumber, eType, inVal, outVal))
    End Function
    
    Public Function get_AfParam(ByRef pAfParam As AfParam) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        Return CheckHResult(Starshootg_get_AfParam(_handle, pAfParam))
    End Function
    
    Public Function GetHistogram(fnHistogramProc As DelegateHistogramCallback) As Boolean
        If _handle Is Nothing OrElse _handle.IsInvalid OrElse _handle.IsClosed Then
            Return False
        End If
        _dHistogramCallback = fnHistogramProc
        _pHistogramCallback = New HISTOGRAM_CALLBACK(AddressOf HistogramCallback)
        Return CheckHResult(Starshootg_GetHistogram(_handle, _pHistogramCallback, _id))
    End Function
    
    Public Shared Function calcClarityFactor(pImageData As IntPtr, bits As Integer, nImgWidth As UInteger, nImgHeight As UInteger) As Double
        Return Starshootg_calc_ClarityFactor(pImageData, bits, nImgWidth, nImgHeight)
    End Function
    
    Public Shared Sub deBayerV2(nBayer As UInteger, nW As Integer, nH As Integer, input As IntPtr, output As IntPtr, nBitDepth As Byte, nBitCount As Byte)
        Starshootg_deBayerV2(nBayer, nW, nH, input, output, nBitDepth, nBitCount)
    End Sub

    '
    '   simulate replug:
    '   return > 0, the number of device has been replug
    '   return = 0, no device found
    '   return E_ACCESSDENIED if without UAC Administrator privileges
    '   for each device found, it will take about 3 seconds
    '
    Public Shared Function Replug(id As String) As Integer
        Return Starshootg_Replug(id)
    End Function

    Private Shared Sub ProgressCallback(percent As Integer, pCallbackCtx As IntPtr)
        Dim obj As Object = Nothing
        _map.TryGetValue(pCallbackCtx.ToInt32(), obj)
        Dim pdelegate As DelegateProgressCallback = TryCast(obj, DelegateProgressCallback)
        If pdelegate IsNot Nothing Then
            pdelegate(percent)
        End If
    End Sub

    ' firmware update
    '   camId: camera ID
    '   filePath: ufw file full path
    '   pFun, pCtx: progress percent callback
    ' Please do Not unplug the camera Or lost power during the upgrade process, this Is very very important.
    ' Once an unplugging Or power outage occurs during the upgrade process, the camera will no longer be available And can only be returned To the factory For repair.
    '
    Public Shared Function Update(camId As String, filePath As String, pdelegate As DelegateProgressCallback) As Integer
        Dim pCallback As PROGRESS_CALLBACK = New PROGRESS_CALLBACK(AddressOf ProgressCallback)
        Dim id As IntPtr = New IntPtr(Interlocked.Increment(_sid))
        _map.Add(id.ToInt32, pCallback)
        Dim ret As Integer = Starshootg_Update(camId, filePath, pCallback, id)
        Return ret
    End Function
End Class
