import java.util.Hashtable;
import java.util.HashMap;
import java.util.Map;
import java.lang.reflect.Method;
import java.nio.ByteBuffer;
import com.sun.jna.*;
import com.sun.jna.ptr.*;
import com.sun.jna.win32.*;
import com.sun.jna.Structure.FieldOrder;

/*
    Versin: 50.19915.20211127

    We use JNA (https://github.com/java-native-access/jna) to call into the starshootg.dll/so/dylib API, the java class starshootg is a thin wrapper class to the native api.
    So the manual en.html(English) and hans.html(Simplified Chinese) are also applicable for programming with starshootg.java.
    See them in the 'doc' directory:
       (1) en.html, English
       (2) hans.html, Simplified Chinese
*/
public class starshootg implements AutoCloseable {
    public final static long FLAG_CMOS                    = 0x00000001L;   /* cmos sensor */
    public final static long FLAG_CCD_PROGRESSIVE         = 0x00000002L;   /* progressive ccd sensor */
    public final static long FLAG_CCD_INTERLACED          = 0x00000004L;   /* interlaced ccd sensor */
    public final static long FLAG_ROI_HARDWARE            = 0x00000008L;   /* support hardware ROI */
    public final static long FLAG_MONO                    = 0x00000010L;   /* monochromatic */
    public final static long FLAG_BINSKIP_SUPPORTED       = 0x00000020L;   /* support bin/skip mode */
    public final static long FLAG_USB30                   = 0x00000040L;   /* usb3.0 */
    public final static long FLAG_TEC                     = 0x00000080L;   /* Thermoelectric Cooler */
    public final static long FLAG_USB30_OVER_USB20        = 0x00000100L;   /* usb3.0 camera connected to usb2.0 port */
    public final static long FLAG_ST4                     = 0x00000200L;   /* ST4 */
    public final static long FLAG_GETTEMPERATURE          = 0x00000400L;   /* support to get the temperature of the sensor */
    public final static long FLAG_RAW10                   = 0x00001000L;   /* pixel format, RAW 10bits */
    public final static long FLAG_RAW12                   = 0x00002000L;   /* pixel format, RAW 12bits */
    public final static long FLAG_RAW14                   = 0x00004000L;   /* pixel format, RAW 14bits */
    public final static long FLAG_RAW16                   = 0x00008000L;   /* pixel format, RAW 16bits */
    public final static long FLAG_FAN                     = 0x00010000L;   /* cooling fan */
    public final static long FLAG_TEC_ONOFF               = 0x00020000L;   /* Thermoelectric Cooler can be turn on or off, support to set the target temperature of TEC */
    public final static long FLAG_ISP                     = 0x00040000L;   /* ISP (Image Signal Processing) chip */
    public final static long FLAG_TRIGGER_SOFTWARE        = 0x00080000L;   /* support software trigger */
    public final static long FLAG_TRIGGER_EXTERNAL        = 0x00100000L;   /* support external trigger */
    public final static long FLAG_TRIGGER_SINGLE          = 0x00200000L;   /* only support trigger single: one trigger, one image */
    public final static long FLAG_BLACKLEVEL              = 0x00400000L;   /* support set and get the black level */
    public final static long FLAG_AUTO_FOCUS              = 0x00800000L;   /* support auto focus */
    public final static long FLAG_BUFFER                  = 0x01000000L;   /* frame buffer */
    public final static long FLAG_DDR                     = 0x02000000L;   /* use very large capacity DDR (Double Data Rate SDRAM) for frame buffer. The capacity is not less than one full frame */
    public final static long FLAG_CG                      = 0x04000000L;   /* support Conversion Gain mode: HCG, LCG */
    public final static long FLAG_YUV411                  = 0x08000000L;   /* pixel format, yuv411 */
    public final static long FLAG_VUYY                    = 0x10000000L;   /* pixel format, yuv422, VUYY */
    public final static long FLAG_YUV444                  = 0x20000000L;   /* pixel format, yuv444 */
    public final static long FLAG_RGB888                  = 0x40000000L;   /* pixel format, RGB888 */
    public final static long FLAG_RAW8                    = 0x80000000L;   /* pixel format, RAW 8 bits */
    public final static long FLAG_GMCY8                   = 0x0000000100000000L;  /* pixel format, GMCY, 8 bits */
    public final static long FLAG_GMCY12                  = 0x0000000200000000L;  /* pixel format, GMCY, 12 bits */
    public final static long FLAG_UYVY                    = 0x0000000400000000L;  /* pixel format, yuv422, UYVY */
    public final static long FLAG_CGHDR                   = 0x0000000800000000L;  /* Conversion Gain: HCG, LCG, HDR */
    public final static long FLAG_GLOBALSHUTTER           = 0x0000001000000000L;  /* global shutter */
    public final static long FLAG_FOCUSMOTOR              = 0x0000002000000000L;  /* support focus motor */
    public final static long FLAG_PRECISE_FRAMERATE       = 0x0000004000000000L;  /* support precise framerate & bandwidth, see OPTION_PRECISE_FRAMERATE & OPTION_BANDWIDTH */
    public final static long FLAG_HEAT                    = 0x0000008000000000L;  /* heat to prevent fogging up */
    public final static long FLAG_LOW_NOISE               = 0x0000010000000000L;  /* support low noise mode (Higher signal noise ratio, lower frame rate) */
    public final static long FLAG_LEVELRANGE_HARDWARE     = 0x0000020000000000L;  /* hardware level range, put(get)_LevelRangeV2 */
    public final static long FLAG_EVENT_HARDWARE          = 0x0000040000000000L;  /* hardware event, such as exposure start & stop */
    public final static long FLAG_LIGHTSOURCE             = 0x0000080000000000L;  /* light source */
    
    public final static int EVENT_EXPOSURE                = 0x0001; /* exposure time or gain changed */
    public final static int EVENT_TEMPTINT                = 0x0002; /* white balance changed, Temp/Tint mode */
    public final static int EVENT_CHROME                  = 0x0003; /* reversed, do not use it */
    public final static int EVENT_IMAGE                   = 0x0004; /* live image arrived, use Starshootg_PullImage to get this image */
    public final static int EVENT_STILLIMAGE              = 0x0005; /* snap (still) frame arrived, use Starshootg_PullStillImage to get this frame */
    public final static int EVENT_WBGAIN                  = 0x0006; /* white balance changed, RGB Gain mode */
    public final static int EVENT_TRIGGERFAIL             = 0x0007; /* trigger failed */
    public final static int EVENT_BLACK                   = 0x0008; /* black balance changed */
    public final static int EVENT_FFC                     = 0x0009; /* flat field correction status changed */
    public final static int EVENT_DFC                     = 0x000a; /* dark field correction status changed */
    public final static int EVENT_ROI                     = 0x000b; /* roi changed */
    public final static int EVENT_LEVELRANGE              = 0x000c; /* level range changed */
    public final static int EVENT_ERROR                   = 0x0080; /* generic error */
    public final static int EVENT_DISCONNECTED            = 0x0081; /* camera disconnected */
    public final static int EVENT_NOFRAMETIMEOUT          = 0x0082; /* no frame timeout error */
    public final static int EVENT_AFFEEDBACK              = 0x0083; /* auto focus feedback information */
    public final static int EVENT_MOTORPOS                = 0x0084; /* focus motor positon */
    public final static int EVENT_NOPACKETTIMEOUT         = 0x0085; /* no packet timeout */
    public final static int EVENT_EXPO_START              = 0x4000; /* hardware event: exposure start */
    public final static int EVENT_EXPO_STOP               = 0x4001; /* hardware event: exposure stop */
    public final static int EVENT_TRIGGER_ALLOW           = 0x4002; /* hardware event: next trigger allow */
    public final static int EVENT_HEARTBEAT               = 0x4003; /* hardware event: heartbeat, can be used to monitor whether the camera is alive */
    public final static int EVENT_FACTORY                 = 0x8001; /* restore factory settings */
    
    public final static int OPTION_NOFRAME_TIMEOUT        = 0x01;       /* no frame timeout: 1 = enable; 0 = disable. default: disable */
    public final static int OPTION_THREAD_PRIORITY        = 0x02;       /* set the priority of the internal thread which grab data from the usb device.
                                                                             Win: iValue: 0 = THREAD_PRIORITY_NORMAL; 1 = THREAD_PRIORITY_ABOVE_NORMAL; 2 = THREAD_PRIORITY_HIGHEST; 3 = THREAD_PRIORITY_TIME_CRITICAL; default: 1; see: https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-setthreadpriority
                                                                             Linux & macOS: The high 16 bits for the scheduling policy, and the low 16 bits for the priority; see: https://linux.die.net/man/3/pthread_setschedparam
                                                                        */
    public final static int OPTION_RAW                    = 0x04;       /* raw data mode, read the sensor "raw" data. This can be set only BEFORE Starshootg_StartXXX(). 0 = rgb, 1 = raw, default value: 0 */
    public final static int OPTION_HISTOGRAM              = 0x05;       /* 0 = only one, 1 = continue mode */
    public final static int OPTION_BITDEPTH               = 0x06;       /* 0 = 8 bits mode, 1 = 16 bits mode */
    public final static int OPTION_FAN                    = 0x07;       /* 0 = turn off the cooling fan, [1, max] = fan speed */
    public final static int OPTION_TEC                    = 0x08;       /* 0 = turn off the thermoelectric cooler, 1 = turn on the thermoelectric cooler */
    public final static int OPTION_LINEAR                 = 0x09;       /* 0 = turn off the builtin linear tone mapping, 1 = turn on the builtin linear tone mapping, default value: 1 */
    public final static int OPTION_CURVE                  = 0x0a;       /* 0 = turn off the builtin curve tone mapping, 1 = turn on the builtin polynomial curve tone mapping, 2 = logarithmic curve tone mapping, default value: 2 */
    public final static int OPTION_TRIGGER                = 0x0b;       /* 0 = video mode, 1 = software or simulated trigger mode, 2 = external trigger mode, 3 = external + software trigger, default value = 0 */
    public final static int OPTION_RGB                    = 0x0c;       /* 0 => RGB24; 1 => enable RGB48 format when bitdepth > 8; 2 => RGB32; 3 => 8 Bits Gray (only for mono camera); 4 => 16 Bits Gray (only for mono camera when bitdepth > 8) */
    public final static int OPTION_COLORMATIX             = 0x0d;       /* enable or disable the builtin color matrix, default value: 1 */
    public final static int OPTION_WBGAIN                 = 0x0e;       /* enable or disable the builtin white balance gain, default value: 1 */
    public final static int OPTION_TECTARGET              = 0x0f;       /* get or set the target temperature of the thermoelectric cooler, in 0.1 degree Celsius. For example, 125 means 12.5 degree Celsius, -35 means -3.5 degree Celsius */
    public final static int OPTION_AUTOEXP_POLICY         = 0x10;       /* auto exposure policy:
                                                                              0: Exposure Only
                                                                              1: Exposure Preferred
                                                                              2: Gain Only
                                                                              3: Gain Preferred
                                                                           default value: 1
                                                                        */
    public final static int OPTION_FRAMERATE              = 0x11;       /* limit the frame rate, range=[0, 63], the default value 0 means no limit */
    public final static int OPTION_DEMOSAIC               = 0x12;       /* demosaic method for both video and still image: BILINEAR = 0, VNG(Variable Number of Gradients) = 1, PPG(Patterned Pixel Grouping) = 2, AHD(Adaptive Homogeneity Directed) = 3, EA(Edge Aware) = 4, see https://en.wikipedia.org/wiki/Demosaicing, default value: 0 */
    public final static int OPTION_DEMOSAIC_VIDEO         = 0x13;       /* demosaic method for video */
    public final static int OPTION_DEMOSAIC_STILL         = 0x14;       /* demosaic method for still image */
    public final static int OPTION_BLACKLEVEL             = 0x15;       /* black level */
    public final static int OPTION_MULTITHREAD            = 0x16;       /* multithread image processing */
    public final static int OPTION_BINNING                = 0x17;       /* binning, 0x01 (no binning), 0x02 (add, 2*2), 0x03 (add, 3*3), 0x04 (add, 4*4), 0x05 (add, 5*5), 0x06 (add, 6*6), 0x07 (add, 7*7), 0x08 (add, 8*8), 0x82 (average, 2*2), 0x83 (average, 3*3), 0x84 (average, 4*4), 0x85 (average, 5*5), 0x86 (average, 6*6), 0x87 (average, 7*7), 0x88 (average, 8*8). The final image size is rounded down to an even number, such as 640/3 to get 212 */
    public final static int OPTION_ROTATE                 = 0x18;       /* rotate clockwise: 0, 90, 180, 270 */
    public final static int OPTION_CG                     = 0x19;       /* Conversion Gain mode: 0 = LCG, 1 = HCG, 2 = HDR */
    public final static int OPTION_PIXEL_FORMAT           = 0x1a;       /* pixel format */
    public final static int OPTION_FFC                    = 0x1b;       /* flat field correction
                                                                          set:
                                                                              0: disable
                                                                              1: enable
                                                                              -1: reset
                                                                              (0xff000000 | n): set the average number to n, [1~255]
                                                                          get:
                                                                              (val & 0xff): 0 -> disable, 1 -> enable, 2 -> inited
                                                                              ((val & 0xff00) >> 8): sequence
                                                                              ((val & 0xff0000) >> 8): average number
                                                                        */
    public final static int OPTION_DDR_DEPTH              = 0x1c;       /* the number of the frames that DDR can cache
                                                                          1: DDR cache only one frame
                                                                          0: Auto:
                                                                              ->one for video mode when auto exposure is enabled
                                                                              ->full capacity for others
                                                                          1: DDR can cache frames to full capacity
                                                                        */
    public final static int OPTION_DFC                    = 0x1d;       /* dark field correction
                                                                          set:
                                                                              0: disable
                                                                              1: enable
                                                                              -1: reset
                                                                              (0xff000000 | n): set the average number to n, [1~255]
                                                                          get:
                                                                              (val & 0xff): 0 -> disable, 1 -> enable, 2 -> inited
                                                                              ((val & 0xff00) >> 8): sequence
                                                                              ((val & 0xff0000) >> 8): average number
                                                                        */
    public final static int OPTION_SHARPENING             = 0x1e;       /* Sharpening: (threshold << 24) | (radius << 16) | strength)
                                                                            strength: [0, 500], default: 0 (disable)
                                                                            radius: [1, 10]
                                                                            threshold: [0, 255]
                                                                        */
    public final static int OPTION_FACTORY                = 0x1f;       /* restore the factory settings */
    public final static int OPTION_TEC_VOLTAGE            = 0x20;       /* get the current TEC voltage in 0.1V, 59 mean 5.9V; readonly */
    public final static int OPTION_TEC_VOLTAGE_MAX        = 0x21;       /* get the TEC maximum voltage in 0.1V; readonly */
    public final static int OPTION_DEVICE_RESET           = 0x22;       /* reset usb device, simulate a replug */
    public final static int OPTION_UPSIDE_DOWN            = 0x23;       /* upsize down:
                                                                            1: yes
                                                                            0: no
                                                                            default: 1 (win), 0 (linux/macos)
                                                                        */
    public final static int OPTION_MOTORPOS               = 0x24;       /* focus motor positon */
    public final static int OPTION_AFMODE                 = 0x25;       /* auto focus mode (0:manul focus; 1:auto focus; 2:once focus; 3:conjugate calibration) */
    public final static int OPTION_AFZONE                 = 0x26;       /* auto focus zone */
    public final static int OPTION_AFFEEDBACK             = 0x27;       /* auto focus information feedback; 0:unknown; 1:focused; 2:focusing; 3:defocus; 4:up; 5:down */
    public final static int OPTION_TESTPATTERN            = 0x28;       /* test pattern:
                                                                            0: TestPattern Off
                                                                            3: monochrome diagonal stripes
                                                                            5: monochrome vertical stripes
                                                                            7: monochrome horizontal stripes
                                                                            9: chromatic diagonal stripes
                                                                        */
    public final static int OPTION_AUTOEXP_THRESHOLD      = 0x29;       /* threshold of auto exposure, default value: 5, range = [2, 15] */
    public final static int OPTION_BYTEORDER              = 0x2a;       /* Byte order, BGR or RGB: 0->RGB, 1->BGR, default value: 1(Win), 0(macOS, Linux, Android) */
    public final static int OPTION_NOPACKET_TIMEOUT       = 0x2b;       /* no packet timeout: 0 = disable, positive value = timeout milliseconds. default: disable */
    public final static int OPTION_MAX_PRECISE_FRAMERATE  = 0x2c;       /* get the precise frame rate maximum value in 0.1 fps, such as 115 means 11.5 fps. E_NOTIMPL means not supported */
    public final static int OPTION_PRECISE_FRAMERATE      = 0x2d;       /* precise frame rate current value in 0.1 fps, range:[1~maximum] */
    public final static int OPTION_BANDWIDTH              = 0x2e;       /* bandwidth, [1-100]% */
    public final static int OPTION_RELOAD                 = 0x2f;       /* reload the last frame in trigger mode */
    public final static int OPTION_CALLBACK_THREAD        = 0x30;       /* dedicated thread for callback */
    public final static int OPTION_FRONTEND_DEQUE_LENGTH  = 0x31;       /* frontend frame buffer deque length, range: [2, 1024], default: 4 */
    public final static int OPTION_FRAME_DEQUE_LENGTH     = 0x31;       /* alias of STARSHOOTG_OPTION_FRONTEND_DEQUE_LENGTH */
    public final static int OPTION_MIN_PRECISE_FRAMERATE  = 0x32;       /* get the precise frame rate minimum value in 0.1 fps, such as 15 means 1.5 fps */
    public final static int OPTION_SEQUENCER_ONOFF        = 0x33;       /* sequencer trigger: on/off */
    public final static int OPTION_SEQUENCER_NUMBER       = 0x34;       /* sequencer trigger: number, range = [1, 255] */
    public final static int OPTION_SEQUENCER_EXPOTIME     = 0x01000000; /* sequencer trigger: exposure time, iOption = OPTION_SEQUENCER_EXPOTIME | index, iValue = exposure time
                                                                             For example, to set the exposure time of the third group to 50ms, call:
                                                                                Starshootg_put_Option(STARSHOOTG_OPTION_SEQUENCER_EXPOTIME | 3, 50000)
                                                                        */
    public final static int OPTION_SEQUENCER_EXPOGAIN     = 0x02000000; /* sequencer trigger: exposure gain, iOption = OPTION_SEQUENCER_EXPOGAIN | index, iValue = gain */
    public final static int OPTION_DENOISE                = 0x35;       /* denoise, strength range: [0, 100], 0 means disable */
    public final static int OPTION_HEAT_MAX               = 0x36;       /* maximum level: heat to prevent fogging up */
    public final static int OPTION_HEAT                   = 0x37;       /* heat to prevent fogging up */
    public final static int OPTION_LOW_NOISE              = 0x38;       /* low noise mode (Higher signal noise ratio, lower frame rate): 1 => enable */
    public final static int OPTION_POWER                  = 0x39;       /* get power consumption, unit: milliwatt */
    public final static int OPTION_GLOBAL_RESET_MODE      = 0x3a;       /* global reset mode */
    public final static int OPTION_OPEN_USB_ERRORCODE     = 0x3b;       /* get the open usb error code */
    public final static int OPTION_LINUX_USB_ZEROCOPY     = 0x3c;       /* global option for linux platform:
                                                                            enable or disable usb zerocopy (helps to reduce memory copy and improve efficiency. Requires kernel version >= 4.6 and hardware platform support)
                                                                            if the image is wrong, this indicates that the hardware platform does not support this feature, please disable it when the program starts:
                                                                              Starshootg_put_Option((this is a global option, the camera handle parameter is not required, use nullptr), STARSHOOTG_OPTION_LINUX_USB_ZEROCOPY, 0)
                                                                            default value:
                                                                              disable(0): android or arm
                                                                              enable(1):  others
                                                                        */
    public final static int OPTION_FLUSH                  = 0x3d;       /* 1 = hard flush, discard frames cached by camera DDR (if any)
                                                                           2 = soft flush, discard frames cached by starshootg.dll (if any)
                                                                           3 = both flush
                                                                           Starshootg_Flush means 'both flush'
                                                                        */
    public final static int OPTION_NUMBER_DROP_FRAME      = 0x3e;       /* get the number of frames that have been grabbed from the USB but dropped by the software */
    public final static int OPTION_DUMP_CFG               = 0x3f;       /* explicitly dump configuration to ini, json, or EEPROM. when camera is closed, it will dump configuration automatically */
    public final static int OPTION_DEFECT_PIXEL           = 0x40;       /* Defect Pixel Correction: 0 => disable, 1 => enable; default: 1 */
    public final static int OPTION_BACKEND_DEQUE_LENGTH   = 0x41;       /* backend frame buffer deque length (Only available in pull mode), range: [2, 1024], default: 3 */
    public final static int OPTION_LIGHTSOURCE_MAX        = 0x42;       /* get the light source range, [0 ~ max] */
    public final static int OPTION_LIGHTSOURCE            = 0x43;       /* light source */   
    public final static int OPTION_HEARTBEAT              = 0x44;       /* Heartbeat interval in millisecond, range = [HEARTBEAT_MIN, HEARTBEAT_MAX], 0 = disable, default: disable */
    public final static int OPTION_FRONTEND_DEQUE_CURRENT = 0x45;       /* get the current number in frontend deque */
    public final static int OPTION_BACKEND_DEQUE_CURRENT  = 0x46;       /* get the current number in backend deque */
    public final static int OPTION_EVENT_HARDWARE         = 0x04000000; /* enable or disable hardware event: iOption = TOUCPAM_OPTION_EVENT_HARDWARE | (event type), iValue = 1 (enable), 0 (disable); default: disable */
    public final static int OPTION_PACKET_NUMBER          = 0x47;       /* get the received packet number */
	
    public final static int PIXELFORMAT_RAW8              = 0x00;
    public final static int PIXELFORMAT_RAW10             = 0x01;
    public final static int PIXELFORMAT_RAW12             = 0x02;
    public final static int PIXELFORMAT_RAW14             = 0x03;
    public final static int PIXELFORMAT_RAW16             = 0x04;
    public final static int PIXELFORMAT_YUV411            = 0x05;
    public final static int PIXELFORMAT_VUYY              = 0x06;
    public final static int PIXELFORMAT_YUV444            = 0x07;
    public final static int PIXELFORMAT_RGB888            = 0x08;
    public final static int PIXELFORMAT_GMCY8             = 0x09;
    public final static int PIXELFORMAT_GMCY12            = 0x0a;
    public final static int PIXELFORMAT_UYVY              = 0x0b;
    
    public final static int FRAMEINFO_FLAG_SEQ            = 0x01; /* sequence number */
    public final static int FRAMEINFO_FLAG_TIMESTAMP      = 0x02;
    
    public final static int IOCONTROLTYPE_GET_SUPPORTEDMODE         = 0x01; /* 0x01->Input, 0x02->Output, (0x01 | 0x02)->support both Input and Output */
    public final static int IOCONTROLTYPE_GET_GPIODIR               = 0x03; /* 0x00->Input, 0x01->Output */
    public final static int IOCONTROLTYPE_SET_GPIODIR               = 0x04;
    public final static int IOCONTROLTYPE_GET_FORMAT                = 0x05; /*
                                                                               0x00-> not connected
                                                                               0x01-> Tri-state: Tri-state mode (Not driven)
                                                                               0x02-> TTL: TTL level signals
                                                                               0x03-> LVDS: LVDS level signals
                                                                               0x04-> RS422: RS422 level signals
                                                                               0x05-> Opto-coupled
                                                                            */
    public final static int IOCONTROLTYPE_SET_FORMAT                = 0x06;
    public final static int IOCONTROLTYPE_GET_OUTPUTINVERTER        = 0x07; /* boolean, only support output signal */
    public final static int IOCONTROLTYPE_SET_OUTPUTINVERTER        = 0x08;
    public final static int IOCONTROLTYPE_GET_INPUTACTIVATION       = 0x09; /* 0x00->Positive, 0x01->Negative */
    public final static int IOCONTROLTYPE_SET_INPUTACTIVATION       = 0x0a;
    public final static int IOCONTROLTYPE_GET_DEBOUNCERTIME         = 0x0b; /* debouncer time in microseconds, [0, 20000] */
    public final static int IOCONTROLTYPE_SET_DEBOUNCERTIME         = 0x0c;
    public final static int IOCONTROLTYPE_GET_TRIGGERSOURCE         = 0x0d; /*
                                                                               0x00-> Opto-isolated input
                                                                               0x01-> GPIO0
                                                                               0x02-> GPIO1
                                                                               0x03-> Counter
                                                                               0x04-> PWM
                                                                               0x05-> Software
                                                                            */
    public final static int IOCONTROLTYPE_SET_TRIGGERSOURCE         = 0x0e;
    public final static int IOCONTROLTYPE_GET_TRIGGERDELAY          = 0x0f; /* Trigger delay time in microseconds, [0, 5000000] */
    public final static int IOCONTROLTYPE_SET_TRIGGERDELAY          = 0x10;
    public final static int IOCONTROLTYPE_GET_BURSTCOUNTER          = 0x11; /* Burst Counter, range: [1 ~ 65535] */
    public final static int IOCONTROLTYPE_SET_BURSTCOUNTER          = 0x12;
    public final static int IOCONTROLTYPE_GET_COUNTERSOURCE         = 0x13; /* 0x00-> Opto-isolated input, 0x01-> GPIO0, 0x02-> GPIO1 */
    public final static int IOCONTROLTYPE_SET_COUNTERSOURCE         = 0x14;
    public final static int IOCONTROLTYPE_GET_COUNTERVALUE          = 0x15; /* Counter Value, range: [1 ~ 65535] */
    public final static int IOCONTROLTYPE_SET_COUNTERVALUE          = 0x16;
    public final static int IOCONTROLTYPE_SET_RESETCOUNTER          = 0x18;
    public final static int IOCONTROLTYPE_GET_PWM_FREQ              = 0x19;
    public final static int IOCONTROLTYPE_SET_PWM_FREQ              = 0x1a;
    public final static int IOCONTROLTYPE_GET_PWM_DUTYRATIO         = 0x1b;
    public final static int IOCONTROLTYPE_SET_PWM_DUTYRATIO         = 0x1c;
    public final static int IOCONTROLTYPE_GET_PWMSOURCE             = 0x1d; /* 0x00-> Opto-isolated input, 0x01-> GPIO0, 0x02-> GPIO1 */
    public final static int IOCONTROLTYPE_SET_PWMSOURCE             = 0x1e;
    public final static int IOCONTROLTYPE_GET_OUTPUTMODE            = 0x1f; /*
                                                                               0x00-> Frame Trigger Wait
                                                                               0x01-> Exposure Active
                                                                               0x02-> Strobe
                                                                               0x03-> User output
                                                                            */
    public final static int IOCONTROLTYPE_SET_OUTPUTMODE            = 0x20;
    public final static int IOCONTROLTYPE_GET_STROBEDELAYMODE       = 0x21; /* boolean, 1 -> delay, 0 -> pre-delay; compared to exposure active signal */
    public final static int IOCONTROLTYPE_SET_STROBEDELAYMODE       = 0x22;
    public final static int IOCONTROLTYPE_GET_STROBEDELAYTIME       = 0x23; /* Strobe delay or pre-delay time in microseconds, [0, 5000000] */
    public final static int IOCONTROLTYPE_SET_STROBEDELAYTIME       = 0x24;
    public final static int IOCONTROLTYPE_GET_STROBEDURATION        = 0x25; /* Strobe duration time in microseconds, [0, 5000000] */
    public final static int IOCONTROLTYPE_SET_STROBEDURATION        = 0x26;
    public final static int IOCONTROLTYPE_GET_USERVALUE             = 0x27; /*
                                                                               bit0-> Opto-isolated output
                                                                               bit1-> GPIO0 output
                                                                               bit2-> GPIO1 output
                                                                            */
    public final static int IOCONTROLTYPE_SET_USERVALUE             = 0x28;
    public final static int IOCONTROLTYPE_GET_UART_ENABLE           = 0x29; /* enable: 1-> on; 0-> off */
    public final static int IOCONTROLTYPE_SET_UART_ENABLE           = 0x2a;
    public final static int IOCONTROLTYPE_GET_UART_BAUDRATE         = 0x2b; /* baud rate: 0-> 9600; 1-> 19200; 2-> 38400; 3-> 57600; 4-> 115200 */
    public final static int IOCONTROLTYPE_SET_UART_BAUDRATE         = 0x2c;
    public final static int IOCONTROLTYPE_GET_UART_LINEMODE         = 0x2d; /* line mode: 0-> TX(GPIO_0)/RX(GPIO_1); 1-> TX(GPIO_1)/RX(GPIO_0) */
    public final static int IOCONTROLTYPE_SET_UART_LINEMODE         = 0x2e;
    
    /* hardware level range mode */
    public final static int LEVELRANGE_MANUAL        = 0x0000;   /* manual */
    public final static int LEVELRANGE_ONCE          = 0x0001;   /* once */
    public final static int LEVELRANGE_CONTINUE      = 0x0002;   /* continue */
    public final static int LEVELRANGE_DISABLE       = 0x0003;   /* disable */
    public final static int LEVELRANGE_ROI           = 0xffff;   /* update roi rect only */
    
    public final static int TEMP_DEF                 = 6503;     /* temp, default */
    public final static int TEMP_MIN                 = 2000;     /* temp, minimum */
    public final static int TEMP_MAX                 = 15000;    /* temp, maximum */
    public final static int TINT_DEF                 = 1000;     /* tint */
    public final static int TINT_MIN                 = 200;      /* tint */
    public final static int TINT_MAX                 = 2500;     /* tint */
    public final static int HUE_DEF                  = 0;        /* hue */
    public final static int HUE_MIN                  = -180;     /* hue */
    public final static int HUE_MAX                  = 180;      /* hue */
    public final static int SATURATION_DEF           = 128;      /* saturation */
    public final static int SATURATION_MIN           = 0;        /* saturation */
    public final static int SATURATION_MAX           = 255;      /* saturation */
    public final static int BRIGHTNESS_DEF           = 0;        /* brightness */
    public final static int BRIGHTNESS_MIN           = -64;      /* brightness */
    public final static int BRIGHTNESS_MAX           = 64;       /* brightness */
    public final static int CONTRAST_DEF             = 0;        /* contrast */
    public final static int CONTRAST_MIN             = -100;     /* contrast */
    public final static int CONTRAST_MAX             = 100;      /* contrast */
    public final static int GAMMA_DEF                = 100;      /* gamma */
    public final static int GAMMA_MIN                = 20;       /* gamma */
    public final static int GAMMA_MAX                = 180;      /* gamma */
    public final static int AETARGET_DEF             = 120;      /* target of auto exposure */
    public final static int AETARGET_MIN             = 16;       /* target of auto exposure */
    public final static int AETARGET_MAX             = 220;      /* target of auto exposure */
    public final static int WBGAIN_DEF               = 0;        /* white balance gain */
    public final static int WBGAIN_MIN               = -127;     /* white balance gain */
    public final static int WBGAIN_MAX               = 127;      /* white balance gain */
    public final static int BLACKLEVEL_MIN           = 0;        /* minimum black level */
    public final static int BLACKLEVEL8_MAX          = 31;       /* maximum black level for bit depth = 8 */
    public final static int BLACKLEVEL10_MAX         = 31 * 4;   /* maximum black level for bit depth = 10 */
    public final static int BLACKLEVEL12_MAX         = 31 * 16;  /* maximum black level for bit depth = 12 */
    public final static int BLACKLEVEL14_MAX         = 31 * 64;  /* maximum black level for bit depth = 14 */
    public final static int BLACKLEVEL16_MAX         = 31 * 256; /* maximum black level for bit depth = 16 */
    public final static int SHARPENING_STRENGTH_DEF  = 0;        /* sharpening strength */
    public final static int SHARPENING_STRENGTH_MIN  = 0;        /* sharpening strength */
    public final static int SHARPENING_STRENGTH_MAX  = 500;      /* sharpening strength */
    public final static int SHARPENING_RADIUS_DEF    = 2;        /* sharpening radius */
    public final static int SHARPENING_RADIUS_MIN    = 1;        /* sharpening radius */
    public final static int SHARPENING_RADIUS_MAX    = 10;       /* sharpening radius */
    public final static int SHARPENING_THRESHOLD_DEF = 0;        /* sharpening threshold */
    public final static int SHARPENING_THRESHOLD_MIN = 0;        /* sharpening threshold */
    public final static int SHARPENING_THRESHOLD_MAX = 255;      /* sharpening threshold */
    public final static int AUTOEXPO_THRESHOLD_DEF   = 5;        /* auto exposure threshold */
    public final static int AUTOEXPO_THRESHOLD_MIN   = 2;        /* auto exposure threshold */
    public final static int AUTOEXPO_THRESHOLD_MAX   = 15;       /* auto exposure threshold */
    public final static int BANDWIDTH_DEF            = 90;       /* bandwidth */
    public final static int BANDWIDTH_MIN            = 1;        /* bandwidth */
    public final static int BANDWIDTH_MAX            = 100;      /* bandwidth */
    public final static int DENOISE_DEF              = 0;        /* denoise */
    public final static int DENOISE_MIN              = 0;        /* denoise */
    public final static int DENOISE_MAX              = 100;      /* denoise */
    public final static int TEC_TARGET_MIN           = -300;     /* TEC target: -30.0 degrees Celsius */
    public final static int TEC_TARGET_DEF           = 0;        /* TEC target: 0.0 degrees Celsius */
    public final static int TEC_TARGET_MAX           = 300;      /* TEC target: 30.0 degrees Celsius */
    public final static int HEARTBEAT_MIN            = 100;      /* millisecond */
    public final static int HEARTBEAT_MAX            = 10000;    /* millisecond */
    
    public static class HRESULTException extends Exception {
        public final static int S_OK            = 0x00000000;
        public final static int S_FALSE         = 0x00000001;
        public final static int E_UNEXPECTED    = 0x8000ffff;
        public final static int E_NOTIMPL       = 0x80004001;
        public final static int E_NOINTERFACE   = 0x80004002;
        public final static int E_ACCESSDENIED  = 0x80070005;
        public final static int E_OUTOFMEMORY   = 0x8007000e;
        public final static int E_INVALIDARG    = 0x80070057;
        public final static int E_POINTER       = 0x80004003;
        public final static int E_FAIL          = 0x80004005;
        public final static int E_WRONG_THREAD  = 0x8001010e;
        public final static int E_GEN_FAILURE   = 0x8007001f;
        public final static int E_PENDING       = 0x8000000a;
        
        private final int _hresult;
        
        public HRESULTException(int hresult) {
            _hresult = hresult;
        }
        
        public int getHRESULT() {
            return _hresult;
        }
        
        @Override
        public String toString() {
            return toString(_hresult);
        }
        
        public static String toString(int hresult) {
            switch (hresult) {
                case E_INVALIDARG:
                    return "One or more arguments are not valid";
                case E_NOTIMPL:
                    return "Not supported or not implemented";
                case E_POINTER:
                    return "Pointer that is not valid";
                case E_UNEXPECTED:
                    return "Unexpected failure";
                case E_ACCESSDENIED:
                    return "General access denied error";
                case E_OUTOFMEMORY:
                    return "Out of memory";
                case E_WRONG_THREAD:
                    return "call function in the wrong thread";
                case E_GEN_FAILURE:
                    return "device not functioning";
                case E_PENDING:
                    return "The data necessary to complete this operation is not yet available";
                default:
                    return "Unspecified failure";
            }
        }
    }
    
    private static int errCheck(int hresult) throws HRESULTException {
        if (hresult < 0)
            throw new HRESULTException(hresult);
        return hresult;
    }
    
    public static class Resolution {
        public int width;
        public int height;
    }
    
    public static class ModelV2 {
        public String name;         /* model name */
        public long flag;           /* STARSHOOTG_FLAG_xxx, 64 bits */
        public int maxspeed;        /* number of speed level, same as Starshootg_get_MaxSpeed(), the speed range = [0, maxspeed], closed interval */
        public int preview;         /* number of preview resolution, same as get_ResolutionNumber() */
        public int still;           /* number of still resolution, same as get_StillResolutionNumber() */
        public int maxfanspeed;     /* maximum fan speed */
        public int ioctrol;         /* number of input/output control */
        public float xpixsz;        /* physical pixel size */
        public float ypixsz;        /* physical pixel size */
        public Resolution[] res;
    }
    
    public static class DeviceV2 {
        public String displayname;  /* display name */
        public String id;           /* unique and opaque id of a connected camera */
        public ModelV2 model;
    }
    
    @FieldOrder({ "width", "height", "flag", "seq", "timestamp" })
    public static class FrameInfoV2 extends Structure {
        public int  width;
        public int  height;
        public int  flag;           /* FRAMEINFO_FLAG_xxxx */
        public int  seq;            /* sequence number */
        public long timestamp;      /* microsecond */
    }
    
    @FieldOrder({ "imax", "imin", "idef", "imaxabs", "iminabs", "zoneh", "zonev" })
    public static class AfParam extends Structure {
        public int imax;            /* maximum auto focus sensor board positon */
        public int imin;            /* minimum auto focus sensor board positon */
        public int idef;            /* conjugate calibration positon */
        public int imaxabs;         /* maximum absolute auto focus sensor board positon, micrometer */
        public int iminabs;         /* maximum absolute auto focus sensor board positon, micrometer */
        public int zoneh;           /* zone horizontal */
        public int zonev;           /* zone vertical */
    }
    
    @FieldOrder({ "left", "top", "right", "bottom" })
    private static class RECT extends Structure {
        public int left, top, right, bottom;
    }
    
    private interface CLib {
        Pointer Starshootg_Version();
        int Starshootg_EnumV2(Pointer ti);
        Pointer Starshootg_OpenByIndex(int index);
        void Starshootg_Close(Pointer h);
        int Starshootg_PullImageV2(Pointer h, Pointer pImageData, int bits, FrameInfoV2 pInfo);
        int Starshootg_PullStillImageV2(Pointer h, Pointer pImageData, int bits, FrameInfoV2 pInfo);
        int Starshootg_PullImageWithRowPitchV2(Pointer h, Pointer pImageData, int bits, int rowPitch, FrameInfoV2 pInfo);
        int Starshootg_PullStillImageWithRowPitchV2(Pointer h, Pointer pImageData, int bits, int rowPitch, FrameInfoV2 pInfo);
        int Starshootg_Stop(Pointer h);
        int Starshootg_Pause(Pointer h, int bPause);
        int Starshootg_Snap(Pointer h, int nResolutionIndex);
        int Starshootg_SnapN(Pointer h, int nResolutionIndex, int nNumber);
        int Starshootg_Trigger(Pointer h, short nNumber);
        int Starshootg_put_Size(Pointer h, int nWidth, int nHeight);
        int Starshootg_get_Size(Pointer h, IntByReference nWidth, IntByReference nHeight);
        int Starshootg_put_eSize(Pointer h, int nResolutionIndex);
        int Starshootg_get_eSize(Pointer h, IntByReference nResolutionIndex);
        int Starshootg_get_FinalSize(Pointer h, IntByReference nWidth, IntByReference nHeight);
        int Starshootg_get_ResolutionNumber(Pointer h);
        int Starshootg_get_Resolution(Pointer h, int nResolutionIndex, IntByReference pWidth, IntByReference pHeight);
        int Starshootg_get_ResolutionRatio(Pointer h, int nResolutionIndex, IntByReference pNumerator, IntByReference pDenominator);
        int Starshootg_get_Field(Pointer h);
        int Starshootg_get_RawFormat(Pointer h, IntByReference nFourCC, IntByReference bitdepth);
        int Starshootg_put_RealTime(Pointer h, int val);
        int Starshootg_get_RealTime(Pointer h, IntByReference val);
        int Starshootg_Flush(Pointer h);
        int Starshootg_get_Temperature(Pointer h, ShortByReference pTemperature);
        int Starshootg_put_Temperature(Pointer h, short nTemperature);
        int Starshootg_get_Roi(Pointer h, IntByReference pxOffset, IntByReference pyOffset, IntByReference pxWidth, IntByReference pyHeight);
        int Starshootg_put_Roi(Pointer h, int xOffset, int yOffset, int xWidth, int yHeight);
        int Starshootg_get_AutoExpoEnable(Pointer h, IntByReference bAutoExposure);
        int Starshootg_put_AutoExpoEnable(Pointer h, int bAutoExposure);
        int Starshootg_get_AutoExpoTarget(Pointer h, ShortByReference Target);
        int Starshootg_put_AutoExpoTarget(Pointer h, short Target);
        int Starshootg_put_MaxAutoExpoTimeAGain(Pointer h, int maxTime, short maxAGain);
        int Starshootg_get_MaxAutoExpoTimeAGain(Pointer h, IntByReference maxTime, ShortByReference maxAGain);
        int Starshootg_put_MinAutoExpoTimeAGain(Pointer h, int minTime, short minAGain);
        int Starshootg_get_MinAutoExpoTimeAGain(Pointer h, IntByReference minTime, ShortByReference minAGain);
        int Starshootg_get_ExpoTime(Pointer h, IntByReference Time)/* in microseconds */;
        int Starshootg_put_ExpoTime(Pointer h, int Time)/* inmicroseconds */;
        int Starshootg_get_ExpTimeRange(Pointer h, IntByReference nMin, IntByReference nMax, IntByReference nDef);
        int Starshootg_get_ExpoAGain(Pointer h, ShortByReference AGain);/* percent, such as 300 */
        int Starshootg_put_ExpoAGain(Pointer h, short AGain);/* percent */
        int Starshootg_get_ExpoAGainRange(Pointer h, ShortByReference nMin, ShortByReference nMax, ShortByReference nDef);
        int Starshootg_put_LevelRange(Pointer h, short[] aLow, short[] aHigh);
        int Starshootg_get_LevelRange(Pointer h, short[] aLow, short[] aHigh);
        int Starshootg_put_LevelRangeV2(Pointer h, short mode, RECT pRoiRect, short[] aLow, short[] aHigh);
        int Starshootg_get_LevelRangeV2(Pointer h, ShortByReference pMode, RECT pRoiRect, short[] aLow, short[] aHigh);
        int Starshootg_put_Hue(Pointer h, int Hue);
        int Starshootg_get_Hue(Pointer h, IntByReference Hue);
        int Starshootg_put_Saturation(Pointer h, int Saturation);
        int Starshootg_get_Saturation(Pointer h, IntByReference Saturation);
        int Starshootg_put_Brightness(Pointer h, int Brightness);
        int Starshootg_get_Brightness(Pointer h, IntByReference Brightness);
        int Starshootg_get_Contrast(Pointer h, IntByReference Contrast);
        int Starshootg_put_Contrast(Pointer h, int Contrast);
        int Starshootg_get_Gamma(Pointer h, IntByReference Gamma);
        int Starshootg_put_Gamma(Pointer h, int Gamma);
        int Starshootg_get_Chrome(Pointer h, IntByReference bChrome);    /* monochromatic mode */
        int Starshootg_put_Chrome(Pointer h, int bChrome);
        int Starshootg_get_VFlip(Pointer h, IntByReference bVFlip);  /* vertical flip */
        int Starshootg_put_VFlip(Pointer h, int bVFlip);
        int Starshootg_get_HFlip(Pointer h, IntByReference bHFlip);
        int Starshootg_put_HFlip(Pointer h, int bHFlip);  /* horizontal flip */
        int Starshootg_get_Negative(Pointer h, IntByReference bNegative);
        int Starshootg_put_Negative(Pointer h, int bNegative);
        int Starshootg_put_Speed(Pointer h, short nSpeed);
        int Starshootg_get_Speed(Pointer h, ShortByReference pSpeed);
        int Starshootg_get_MaxSpeed(Pointer h);/* get the maximum speed, "Frame Speed Level", speed range = [0, max] */
        int Starshootg_get_MaxBitDepth(Pointer h);/* get the max bit depth of this camera, such as 8, 10, 12, 14, 16 */
        int Starshootg_get_FanMaxSpeed(Pointer h);/* get the maximum fan speed, the fan speed range = [0, max], closed interval */
        int Starshootg_put_HZ(Pointer h, int nHZ);
        int Starshootg_get_HZ(Pointer h, IntByReference nHZ);
        int Starshootg_put_Mode(Pointer h, int bSkip); /* skip or bin */
        int Starshootg_get_Mode(Pointer h, IntByReference bSkip);
        int Starshootg_put_TempTint(Pointer h, int nTemp, int nTint);
        int Starshootg_get_TempTint(Pointer h, IntByReference nTemp, IntByReference nTint);
        int Starshootg_put_WhiteBalanceGain(Pointer h, int[] aGain);
        int Starshootg_get_WhiteBalanceGain(Pointer h, int[] aGain);
        int Starshootg_put_BlackBalance(Pointer h, short[] aSub);
        int Starshootg_get_BlackBalance(Pointer h, short[] aSub);
        int Starshootg_put_AWBAuxRect(Pointer h, RECT pAuxRect);
        int Starshootg_get_AWBAuxRect(Pointer h, RECT pAuxRect);
        int Starshootg_put_AEAuxRect(Pointer h, RECT pAuxRect);
        int Starshootg_get_AEAuxRect(Pointer h, RECT pAuxRect);
        int Starshootg_put_ABBAuxRect(Pointer h, RECT pAuxRect);
        int Starshootg_get_ABBAuxRect(Pointer h, RECT pAuxRect);
        int Starshootg_get_MonoMode(Pointer h);
        int Starshootg_get_StillResolutionNumber(Pointer h);
        int Starshootg_get_StillResolution(Pointer h, int nResolutionIndex, IntByReference pWidth, IntByReference pHeight);
        int Starshootg_get_Revision(Pointer h, ShortByReference pRevision);
        int Starshootg_get_SerialNumber(Pointer h, Pointer sn);
        int Starshootg_get_FwVersion(Pointer h, Pointer fwver);
        int Starshootg_get_HwVersion(Pointer h, Pointer hwver);
        int Starshootg_get_FpgaVersion(Pointer h, Pointer fpgaver);
        int Starshootg_get_ProductionDate(Pointer h, Pointer pdate);
        int Starshootg_get_PixelSize(Pointer h, int nResolutionIndex, FloatByReference x, FloatByReference y);
        int Starshootg_AwbOnce(Pointer h, Pointer fnTTProc, Pointer pTTCtx);
        int Starshootg_AwbInit(Pointer h, Pointer fnWBProc, Pointer pWBCtx);
        int Starshootg_LevelRangeAuto(Pointer h);
        int Starshootg_AbbOnce(Pointer h, Pointer fnBBProc, Pointer pBBCtx);
        int Starshootg_put_LEDState(Pointer h, short iLed, short iState, short iPeriod);
        int Starshootg_write_EEPROM(Pointer h, int addr, Pointer pBuffer, int nBufferLen);
        int Starshootg_read_EEPROM(Pointer h, int addr, Pointer pBuffer, int nBufferLen);
        int Starshootg_write_Pipe(Pointer h, int pipeNum, Pointer pBuffer, int nBufferLen);
        int Starshootg_read_Pipe(Pointer h, int pipeNum, Pointer pBuffer, int nBufferLen);
        int Starshootg_feed_Pipe(Pointer h, int pipeNum);
        int Starshootg_write_UART(Pointer h, Pointer pBuffer, int nBufferLen);
        int Starshootg_read_UART(Pointer h, Pointer pBuffer, int nBufferLen);
        int Starshootg_put_Option(Pointer h, int iOption, int iValue);
        int Starshootg_get_Option(Pointer h, int iOption, IntByReference iValue);
        int Starshootg_put_Linear(Pointer h, byte[] v8, short[] v16);
        int Starshootg_put_Curve(Pointer h, byte[] v8, short[] v16);
        int Starshootg_put_ColorMatrix(Pointer h, double[] v);
        int Starshootg_put_InitWBGain(Pointer h, short[] v);
        int Starshootg_get_FrameRate(Pointer h, IntByReference nFrame, IntByReference nTime, IntByReference nTotalFrame);
        int Starshootg_FfcOnce(Pointer h);
        int Starshootg_DfcOnce(Pointer h);
        int Starshootg_IoControl(Pointer h, int ioLineNumber, int eType, int outVal, IntByReference inVal);
        int Starshootg_get_AfParam(Pointer h, AfParam pAfParam);
        
        int Starshootg_PullImageV2Array(Pointer h, byte[] pImageData, int bits, FrameInfoV2 pInfo);
        int Starshootg_PullStillImageV2Array(Pointer h, byte[] pImageData, int bits, FrameInfoV2 pInfo);
        int Starshootg_PullImageWithRowPitchV2Array(Pointer h, byte[] pImageData, int bits, int rowPitch, FrameInfoV2 pInfo);
        int Starshootg_PullStillImageWithRowPitchV2Array(Pointer h, byte[] pImageData, int bits, int rowPitch, FrameInfoV2 pInfo);
        int Starshootg_write_EEPROMArray(Pointer h, int addr, byte[] pBuffer, int nBufferLen);
        int Starshootg_read_EEPROMArray(Pointer h, int addr, byte[] pBuffer, int nBufferLen);
        int Starshootg_write_PipeArray(Pointer h, int pipeNum, byte[] pBuffer, int nBufferLen);
        int Starshootg_read_PipeArray(Pointer h, int pipeNum, byte[] pBuffer, int nBufferLen);
        int Starshootg_write_UARTArray(Pointer h, byte[] pBuffer, int nBufferLen);
        int Starshootg_read_UARTArray(Pointer h, byte[] pBuffer, int nBufferLen);
    }

    private interface WinLibrary extends CLib, StdCallLibrary {
        WinLibrary INSTANCE = (WinLibrary)Native.load("starshootg", WinLibrary.class, options_);
        
        interface EVENT_CALLBACK extends StdCallCallback {
            void invoke(int nEvent, long pCallbackCtx);
        }
        
        Pointer Starshootg_Open(WString id);
        int Starshootg_Replug(WString id);
        int Starshootg_StartPullModeWithCallback(Pointer h, EVENT_CALLBACK pEventCallback, long pCallbackCtx);
        int Starshootg_StartPullModeWithWndMsg(Pointer h, Pointer hWnd, int nMsg);
        int Starshootg_FfcImport(Pointer h, WString filepath);
        int Starshootg_FfcExport(Pointer h, WString filepath);
        int Starshootg_DfcImport(Pointer h, WString filepath);
        int Starshootg_DfcExport(Pointer h, WString filepath);

        interface PROGRESS_CALLBACK extends StdCallCallback {
            void invoke(int percent, long pCallbackCtx);
        }
        int Starshootg_Update(WString camId, WString filePath, PROGRESS_CALLBACK pProgressCallback, long pCallbackCtx);
     }
    
    private interface CLibrary extends CLib, Library {
        CLibrary INSTANCE = (CLibrary)Native.load("starshootg", CLibrary.class, options_);
        
        interface EVENT_CALLBACK extends Callback {
            void invoke(int nEvent, long pCallbackCtx);
        }
        
        Pointer Starshootg_Open(String id);
        int Starshootg_Replug(String id);
        int Starshootg_StartPullModeWithCallback(Pointer h, EVENT_CALLBACK pEventCallback, long pCallbackCtx);
        int Starshootg_FfcImport(Pointer h, String filepath);
        int Starshootg_FfcExport(Pointer h, String filepath);
        int Starshootg_DfcImport(Pointer h, String filepath);
        int Starshootg_DfcExport(Pointer h, String filepath);
        
        interface HOTPLUG_CALLBACK extends Callback {
            void invoke(long pCallbackCtx);
        }
        void Starshootg_HotPlug(HOTPLUG_CALLBACK pHotPlugCallback, long pCallbackCtx);

        interface PROGRESS_CALLBACK extends Callback {
            void invoke(int percent, long pCallbackCtx);
        }
        int Starshootg_Update(String camId, String filePath, PROGRESS_CALLBACK pProgressCallback, long pCallbackCtx);
    }
    
    public interface IEventCallback {
        void onEvent(int nEvent);
    }
    
    public interface IHotplugCallback {
        void onHotplug();
    }

    public interface IProgressCallback {
        void onProgress(int percent);
    }
    
    private final static Map options_ = new HashMap() {
        {
            put(Library.OPTION_FUNCTION_MAPPER, new FunctionMapper() {
                HashMap<String, String> funcmap_ = new HashMap() {
                    {
                        put("Starshootg_PullImageV2Array", "Starshootg_PullImageV2");
                        put("Starshootg_PullStillImageV2Array", "Starshootg_PullStillImageV2");
                        put("Starshootg_PullImageWithRowPitchV2Array", "Starshootg_PullImageWithRowPitchV2");
                        put("Starshootg_PullStillImageWithRowPitchV2Array", "Starshootg_PullStillImageWithRowPitchV2");
                        put("Starshootg_write_EEPROMArray", "Starshootg_write_EEPROM");
                        put("Starshootg_read_EEPROMArray", "Starshootg_read_EEPROM");
                        put("Starshootg_write_PipeArray", "Starshootg_write_Pipe");
                        put("Starshootg_read_PipeArray", "Starshootg_read_Pipe");
                        put("Starshootg_write_UARTArray", "Starshootg_write_UART");
                        put("Starshootg_read_UARTArray", "Starshootg_read_UART");
                    }
                };
                
                @Override
                public String getFunctionName(NativeLibrary library, Method method) {
                    String name = method.getName();
                    String str = funcmap_.get(name);
                    if (str != null)
                        return str;
                    else
                        return name;
                }
            });
        }
    };
    
    private final static CLib _lib = Platform.isWindows() ?  WinLibrary.INSTANCE : CLibrary.INSTANCE;
    private final static Hashtable _hash = new Hashtable();
    private static int _clsid = 0;
    private static IHotplugCallback _hotplug = null;
    private static CLibrary.HOTPLUG_CALLBACK _hotplugcallback = null;
    private int _objid = 0;
    private Pointer _handle = null;
    private Callback _callback = null;
    private IEventCallback _cb = null;
    
    static public int MAKEFOURCC(int a, int b, int c, int d) {
        return ((int)(byte)(a) | ((int)(byte)(b) << 8) | ((int)(byte)(c) << 16) | ((int)(byte)(d) << 24));
    }
    
    /*
        the object of starshootg must be obtained by static mothod Open or OpenByIndex, it cannot be obtained by obj = new starshootg (The constructor is private on purpose)
    */
    private starshootg(Pointer h) {
        _handle = h;
        synchronized (_hash) {
            _objid = _clsid++;
        }
    }
    
    @Override
    public void close() {
        if (_handle != null) {
            _lib.Starshootg_Close(_handle);
            _handle = null;
        }
        
        _callback = null;
        _cb = null;
        _hash.remove(_objid);
    }
    
    /* get the version of this dll/so/dylib, which is: 50.19915.20211127 */
    public static String Version() {
        if (Platform.isWindows())
            return _lib.Starshootg_Version().getWideString(0);
        else
            return _lib.Starshootg_Version().getString(0);
    }

    public static void put_GlobalOption(int iOption, int iValue) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Option(null, iOption, iValue));
    }

    public static int get_GlobalOption(int iOption) throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_Option(null, iOption, p));
        return p.getValue();
    }
    
    public static void HotPlug(IHotplugCallback cb) throws HRESULTException {
        if (Platform.isWindows())       /* only available on macOS and Linux, it's unnecessary on Windows */
            errCheck(HRESULTException.E_NOTIMPL);
        else
        {
            _hotplug = cb;
            if (_hotplug == null)
                _hotplugcallback = null;
            else
            {
                _hotplugcallback = new CLibrary.HOTPLUG_CALLBACK() {
                    @Override
                    public void invoke(long pCallbackCtx) {
                        if (_hotplug != null)
                            _hotplug.onHotplug();
                    }
                };
            }
            ((CLibrary)_lib).Starshootg_HotPlug(_hotplugcallback, 0);
        }
    }
    
    /* enumerate cameras that are currently connected to the computer */
    public static DeviceV2[] EnumV2() {
        Memory ptr = new Memory(512 * 128);
        int cnt = _lib.Starshootg_EnumV2(ptr);
        DeviceV2[] arr = new DeviceV2[(cnt > 0) ? cnt : 0];
        if (cnt > 0) {
            long poffset = 0, qoffset = 0;
            for (int i = 0; i < cnt; ++i) {
                arr[i] = new DeviceV2();
                if (Platform.isWindows())
                {
                    arr[i].displayname = ptr.getWideString(poffset);
                    poffset += 128;
                    arr[i].id = ptr.getWideString(poffset);
                    poffset += 128;
                }
                else
                {
                    arr[i].displayname = ptr.getString(poffset);
                    poffset += 64;
                    arr[i].id = ptr.getString(poffset);
                    poffset += 64;
                }
                
                Pointer qtr = ptr.getPointer(poffset);
                poffset += Native.POINTER_SIZE;
                qoffset = 0;
                
                {
                    arr[i].model = new ModelV2();
                    arr[i].model.name = qtr.getPointer(qoffset).getWideString(0);
                    qoffset += Native.POINTER_SIZE;
                    if (Platform.isWindows() && (4 == Native.POINTER_SIZE))   /* 32bits windows */
                        qoffset += 4; //skip 4 bytes, different from the linux version
                    arr[i].model.flag = qtr.getLong(qoffset);
                    qoffset += 8;
                    arr[i].model.maxspeed = qtr.getInt(qoffset);
                    qoffset += 4;
                    arr[i].model.preview = qtr.getInt(qoffset);
                    qoffset += 4;
                    arr[i].model.still = qtr.getInt(qoffset);
                    qoffset += 4;
                    arr[i].model.maxfanspeed = qtr.getInt(qoffset);
                    qoffset += 4;
                    arr[i].model.ioctrol = qtr.getInt(qoffset);
                    qoffset += 4;
                    arr[i].model.xpixsz = qtr.getFloat(qoffset);
                    qoffset += 4;
                    arr[i].model.ypixsz = qtr.getFloat(qoffset);
                    qoffset += 4;
                    arr[i].model.res = new Resolution[arr[i].model.preview];
                    for (int j = 0; j < arr[i].model.preview; ++j) {
                        arr[i].model.res[j] = new Resolution();
                        arr[i].model.res[j].width = qtr.getInt(qoffset);
                        qoffset += 4;
                        arr[i].model.res[j].height = qtr.getInt(qoffset);
                        qoffset += 4;
                    }
                }
            }
        }
        
        return arr;
    }
    
    /*
        the object of starshootg must be obtained by static mothod Open or OpenByIndex, it cannot be obtained by obj = new starshootg (The constructor is private on purpose)
    */
    // id: enumerated by EnumV2, null means the first camera
    public static starshootg Open(String id) {
        Pointer tmphandle = null;
        if (Platform.isWindows()) {
            if (id == null)
                tmphandle = ((WinLibrary)_lib).Starshootg_Open(null);
            else
                tmphandle = ((WinLibrary)_lib).Starshootg_Open(new WString(id));
        }
        else {
            tmphandle = ((CLibrary)_lib).Starshootg_Open(id);
        }
        if (tmphandle == null)
            return null;
        return new starshootg(tmphandle);
    }
    
    /*
        the object of starshootg must be obtained by static mothod Open or OpenByIndex, it cannot be obtained by obj = new starshootg (The constructor is private on purpose)
    */
    /*
        the same with Open, but use the index as the parameter. such as:
        index == 0, open the first camera,
        index == 1, open the second camera,
        etc
    */
    public static starshootg OpenByIndex(int index) {
        Pointer tmphandle = _lib.Starshootg_OpenByIndex(index);
        if (tmphandle == null)
            return null;
        return new starshootg(tmphandle);
    }
    
    public int getResolutionNumber() throws HRESULTException {
        return errCheck(_lib.Starshootg_get_ResolutionNumber(_handle));
    }
    
    public int getStillResolutionNumber() throws HRESULTException {
        return errCheck(_lib.Starshootg_get_StillResolutionNumber(_handle));
    }
    
    /*
        false:    color mode
        true:     mono mode, such as EXCCD00300KMA and UHCCD01400KMA
    */
    public boolean getMonoMode() throws HRESULTException {
        return (0 == errCheck(_lib.Starshootg_get_MonoMode(_handle)));
    }
    
    /* get the maximum speed, "Frame Speed Level" */
    public int getMaxSpeed() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_MaxSpeed(_handle));
        return p.getValue();
    }
    
    /* get the max bit depth of this camera, such as 8, 10, 12, 14, 16 */
    public int getMaxBitDepth() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_MaxBitDepth(_handle));
        return p.getValue();
    }
    
    /* get the maximum fan speed, the fan speed range = [0, max], closed interval */
    public int getFanMaxSpeed() throws HRESULTException {
        return errCheck(_lib.Starshootg_get_FanMaxSpeed(_handle));
    }
    
    /* get the revision */
    public short getRevision() throws HRESULTException {
        ShortByReference p = new ShortByReference();
        errCheck(_lib.Starshootg_get_Revision(_handle, p));
        return p.getValue();
    }
    
    /* get the serial number which is always 32 chars which is zero-terminated such as "TP110826145730ABCD1234FEDC56787" */
    public String getSerialNumber() throws HRESULTException {
        Memory p = new Memory(32);
        errCheck(_lib.Starshootg_get_SerialNumber(_handle, p));
        return p.getString(0);
    }
    
    /* get the camera firmware version, such as: 3.2.1.20140922 */
    public String getFwVersion() throws HRESULTException {
        Memory p = new Memory(16);
        errCheck(_lib.Starshootg_get_FwVersion(_handle, p));
        return p.getString(0);
    }
    
    /* get the camera hardware version, such as: 3.2.1.20140922 */
    public String getHwVersion() throws HRESULTException {
        Memory p = new Memory(16);
        errCheck(_lib.Starshootg_get_HwVersion(_handle, p));
        return p.getString(0);
    }
    
    /* such as: 20150327 */
    public String getProductionDate() throws HRESULTException {
        Memory p = new Memory(16);
        errCheck(_lib.Starshootg_get_ProductionDate(_handle, p));
        return p.getString(0);
    }
    
    /* such as: 1.3 */
    public String getFpgaVersion() throws HRESULTException {
        Memory p = new Memory(16);
        errCheck(_lib.Starshootg_get_FpgaVersion(_handle, p));
        return p.getString(0);
    }
    
    public int getField() throws HRESULTException {
        return errCheck(_lib.Starshootg_get_Field(_handle));
    }
    
    private static void OnEventCallback(int nEvent, long pCallbackCtx) {
        Object o = _hash.get((int)pCallbackCtx);
        if (o instanceof starshootg)
        {
            starshootg t = (starshootg)o;
            if (t._cb != null)
                t._cb.onEvent(nEvent);
        }
    }
    
    public void StartPullModeWithCallback(IEventCallback cb) throws HRESULTException {
        _cb = cb;
        _hash.put(_objid, this);
        if (Platform.isWindows()) {
            _callback = new WinLibrary.EVENT_CALLBACK() {
                @Override
                public void invoke(int nEvent, long pCallbackCtx) {
                    OnEventCallback(nEvent, pCallbackCtx);
                }
            };
            Native.setCallbackThreadInitializer(_callback, new CallbackThreadInitializer(false, false, "eventCallback"));
            errCheck(((WinLibrary)_lib).Starshootg_StartPullModeWithCallback(_handle, (WinLibrary.EVENT_CALLBACK)_callback, _objid));
        }
        else {
            _callback = new CLibrary.EVENT_CALLBACK() {
                @Override
                public void invoke(int nEvent, long pCallbackCtx) {
                    OnEventCallback(nEvent, pCallbackCtx);
                }
            };
            Native.setCallbackThreadInitializer(_callback, new CallbackThreadInitializer(false, false, "eventCallback"));
            errCheck(((CLibrary)_lib).Starshootg_StartPullModeWithCallback(_handle, (CLibrary.EVENT_CALLBACK)_callback, _objid));
        }
    }

    public void StartPullModeWithWndMsg(Pointer hWnd, int nMsg) throws HRESULTException {
        if (Platform.isWindows())
            errCheck(((WinLibrary)_lib).Starshootg_StartPullModeWithWndMsg(_handle, hWnd, nMsg));
        else
            errCheck(HRESULTException.E_NOTIMPL);
    }

    public void PullImageV2(ByteBuffer pImageData, int bits, FrameInfoV2 pInfo) throws HRESULTException {
        if (pImageData.isDirect())
            errCheck(_lib.Starshootg_PullImageV2(_handle, Native.getDirectBufferPointer(pImageData), bits, pInfo));
        else if (pImageData.hasArray())
            PullImageV2(pImageData.array(), bits, pInfo);
        else
            errCheck(HRESULTException.E_INVALIDARG);
    }
    
    public void PullImageV2(byte[] pImageData, int bits, FrameInfoV2 pInfo) throws HRESULTException {
        errCheck(_lib.Starshootg_PullImageV2Array(_handle, pImageData, bits, pInfo));
    }
    
    public void PullStillImageV2(ByteBuffer pImageData, int bits, FrameInfoV2 pInfo) throws HRESULTException {
        if (pImageData.isDirect())
            errCheck(_lib.Starshootg_PullStillImageV2(_handle, Native.getDirectBufferPointer(pImageData), bits, pInfo));
        else if (pImageData.hasArray())
            PullStillImageV2(pImageData.array(), bits, pInfo);
        else
            errCheck(HRESULTException.E_INVALIDARG);
    }
    
    public void PullStillImageV2(byte[] pImageData, int bits, FrameInfoV2 pInfo) throws HRESULTException {
        errCheck(_lib.Starshootg_PullStillImageV2Array(_handle, pImageData, bits, pInfo));
    }
    
    /*  bits: 24 (RGB24), 32 (RGB32), 48 (RGB48), 8 (Gray) or 16 (Gray). In RAW mode, this parameter is ignored.
        rowPitch: The distance from one row to the next row. rowPitch = 0 means using the default row pitch. rowPitch = -1 means zero padding
        
        ----------------------------------------------------------------------------------------------
        | format                             | 0 means default row pitch     | -1 means zero padding |
        |------------------------------------|-------------------------------|-----------------------|
        | RGB       | RGB24                  | TDIBWIDTHBYTES(24 * Width)    | Width * 3             |
        |           | RGB32                  | Width * 4                     | Width * 4             |
        |           | RGB48                  | TDIBWIDTHBYTES(48 * Width)    | Width * 6             |
        |           | GREY8                  | TDIBWIDTHBYTES(8 * Width)     | Width                 |
        |           | GREY16                 | TDIBWIDTHBYTES(16 * Width)    | Width * 2             |
        |-----------|------------------------|-------------------------------|-----------------------|
        | RAW       | 8bits Mode             | Width                         | Width                 |
        |           | 10/12/14/16bits Mode   | Width * 2                     | Width * 2             |
        |-----------|------------------------|-------------------------------|-----------------------|      
    */
    public void PullImageWithRowPitchV2(ByteBuffer pImageData, int bits, int rowPitch, FrameInfoV2 pInfo) throws HRESULTException {
        if (pImageData.isDirect())
            errCheck(_lib.Starshootg_PullImageWithRowPitchV2(_handle, Native.getDirectBufferPointer(pImageData), bits, rowPitch, pInfo));
        else if (pImageData.hasArray())
            PullImageWithRowPitchV2(pImageData.array(), bits, rowPitch, pInfo);
        else
            errCheck(HRESULTException.E_INVALIDARG);
    }
    
    public void PullImageWithRowPitchV2(byte[] pImageData, int bits, int rowPitch, FrameInfoV2 pInfo) throws HRESULTException {
        errCheck(_lib.Starshootg_PullImageWithRowPitchV2Array(_handle, pImageData, bits, rowPitch, pInfo));
    }
    
    public void PullStillImageWithRowPitchV2(ByteBuffer pImageData, int bits, int rowPitch, FrameInfoV2 pInfo) throws HRESULTException {
        if (pImageData.isDirect())
            errCheck(_lib.Starshootg_PullStillImageWithRowPitchV2(_handle, Native.getDirectBufferPointer(pImageData), bits, rowPitch, pInfo));
        else if (pImageData.hasArray())
            PullStillImageWithRowPitchV2(pImageData.array(), bits, rowPitch, pInfo);
        else
            errCheck(HRESULTException.E_INVALIDARG);
    }
    
    public void PullStillImageWithRowPitchV2(byte[] pImageData, int bits, int rowPitch, FrameInfoV2 pInfo) throws HRESULTException {
        errCheck(_lib.Starshootg_PullStillImageWithRowPitchV2Array(_handle, pImageData, bits, rowPitch, pInfo));
    }
    
    public void Stop() throws HRESULTException {
        errCheck(_lib.Starshootg_Stop(_handle));
        
        _callback = null;
        _cb = null;
        _hash.remove(_objid);
    }
    
    public void Pause(boolean bPause) throws HRESULTException {
        errCheck(_lib.Starshootg_Pause(_handle, bPause ? 1 : 0));
    }
    
    public void Snap(int nResolutionIndex) throws HRESULTException {
        errCheck(_lib.Starshootg_Snap(_handle, nResolutionIndex));
    }
     
    /* multiple still image snap */
    public void SnapN(int nResolutionIndex, int nNumber) throws HRESULTException {
        errCheck(_lib.Starshootg_SnapN(_handle, nResolutionIndex, nNumber));
    }
    
    /*
        soft trigger:
        nNumber:    0xffff:     trigger continuously
                    0:          cancel trigger
                    others:     number of images to be triggered
    */
    public void Trigger(short nNumber) throws HRESULTException {
        errCheck(_lib.Starshootg_Trigger(_handle, nNumber));
    }
    
    public void put_Size(int nWidth, int nHeight) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Size(_handle, nWidth, nHeight));
    }
    
    /* width, height */
    public int[] get_Size() throws HRESULTException {
        IntByReference p = new IntByReference();
        IntByReference q = new IntByReference();
        errCheck(_lib.Starshootg_get_Size(_handle, p, q));
        return new int[] { p.getValue(), q.getValue() };
    }
    
    /*
        put_Size, put_eSize, can be used to set the video output resolution BEFORE Start.
        put_Size use width and height parameters, put_eSize use the index parameter.
        for example, UCMOS03100KPA support the following resolutions:
            index 0:    2048,   1536
            index 1:    1024,   768
            index 2:    680,    510
        so, we can use put_Size(h, 1024, 768) or put_eSize(h, 1). Both have the same effect.
    */
    public void put_eSize(int nResolutionIndex) throws HRESULTException {
        errCheck(_lib.Starshootg_put_eSize(_handle, nResolutionIndex));
    }
    
    public int get_eSize() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_eSize(_handle, p));
        return p.getValue();
    }
    
    /*
        final size after ROI, rotate, binning
    */
    public int[] get_FinalSize() throws HRESULTException {
        IntByReference p = new IntByReference();
        IntByReference q = new IntByReference();
        errCheck(_lib.Starshootg_get_FinalSize(_handle, p, q));
        return new int[] { p.getValue(), q.getValue() };
    }
    
    /* width, height */
    public int[] get_Resolution(int nResolutionIndex) throws HRESULTException {
        IntByReference p = new IntByReference();
        IntByReference q = new IntByReference();
        errCheck(_lib.Starshootg_get_Resolution(_handle, nResolutionIndex, p, q));
        return new int[] { p.getValue(), q.getValue() };
    }
    
    /*
        get the sensor pixel size, such as: 2.4um
    */
    public float[] get_PixelSize(int nResolutionIndex) throws HRESULTException {
        FloatByReference p = new FloatByReference();
        FloatByReference q = new FloatByReference();
        errCheck(_lib.Starshootg_get_PixelSize(_handle, nResolutionIndex, p, q));
        return new float[] { p.getValue(), q.getValue() };
    }
    
    /*
        numerator/denominator, such as: 1/1, 1/2, 1/3
    */
    public int[] get_ResolutionRatio(int nResolutionIndex) throws HRESULTException {
        IntByReference p = new IntByReference();
        IntByReference q = new IntByReference();
        errCheck(_lib.Starshootg_get_ResolutionRatio(_handle, nResolutionIndex, p, q));
        return new int[] { p.getValue(), q.getValue() };
    }
    
    /*
        see: http://www.fourcc.org
        FourCC:
            MAKEFOURCC('G', 'B', 'R', 'G'), see http://www.siliconimaging.com/RGB%20Bayer.htm
            MAKEFOURCC('R', 'G', 'G', 'B')
            MAKEFOURCC('B', 'G', 'G', 'R')
            MAKEFOURCC('G', 'R', 'B', 'G')
            MAKEFOURCC('Y', 'Y', 'Y', 'Y'), monochromatic sensor
            MAKEFOURCC('Y', '4', '1', '1'), yuv411
            MAKEFOURCC('V', 'U', 'Y', 'Y'), yuv422
            MAKEFOURCC('U', 'Y', 'V', 'Y'), yuv422
            MAKEFOURCC('Y', '4', '4', '4'), yuv444
            MAKEFOURCC('R', 'G', 'B', '8'), RGB888
    */
    public int[] get_RawFormat() throws HRESULTException {
        IntByReference p = new IntByReference();
        IntByReference q = new IntByReference();
        errCheck(_lib.Starshootg_get_RawFormat(_handle, p, q));
        return new int[] { p.getValue(), q.getValue() };
    }
    
    /*  0: stop grab frame when frame buffer deque is full, until the frames in the queue are pulled away and the queue is not full
        1: realtime
              use minimum frame buffer. When new frame arrive, drop all the pending frame regardless of whether the frame buffer is full.
              If DDR present, also limit the DDR frame buffer to only one frame.
        2: soft realtime
              Drop the oldest frame when the queue is full and then enqueue the new frame
        default: 0
    */
    public void put_RealTime(int val) throws HRESULTException {
        errCheck(_lib.Starshootg_put_RealTime(_handle, val));
    }
    
    public int get_RealTime() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_RealTime(_handle, p));
        return p.getValue();
    }
    
    /* Flush is obsolete, it's a synonyms for put_Option(OPTION_FLUSH, 3) */
    public void Flush() throws HRESULTException {
        errCheck(_lib.Starshootg_Flush(_handle));
    }
    
    /*
        ------------------------------------------------------------------|
        | Parameter               |   Range       |   Default             |
        |-----------------------------------------------------------------|
        | Auto Exposure Target    |   16~235      |   120                 |
        | Temp                    |   2000~15000  |   6503                |
        | Tint                    |   200~2500    |   1000                |
        | LevelRange              |   0~255       |   Low = 0, High = 255 |
        | Contrast                |   -100~100    |   0                   |
        | Hue                     |   -180~180    |   0                   |
        | Saturation              |   0~255       |   128                 |
        | Brightness              |   -64~64      |   0                   |
        | Gamma                   |   20~180      |   100                 |
        | WBGain                  |   -127~127    |   0                   |
        ------------------------------------------------------------------|
    */
    
    public boolean get_AutoExpoEnable() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_AutoExpoEnable(_handle, p));
        return (p.getValue() != 0);
    }
    
    public void put_AutoExpoEnable(boolean bAutoExposure) throws HRESULTException {
        errCheck(_lib.Starshootg_put_AutoExpoEnable(_handle, bAutoExposure ? 1 : 0));
    }
    
    public short get_AutoExpoTarget() throws HRESULTException {
        ShortByReference p = new ShortByReference();
        errCheck(_lib.Starshootg_get_AutoExpoTarget(_handle, p));
        return p.getValue();
    }
    
    public void put_AutoExpoTarget(short Target) throws HRESULTException {
        errCheck(_lib.Starshootg_put_AutoExpoTarget(_handle, Target));
    }
    
    public void put_MaxAutoExpoTimeAGain(int maxTime, short maxAGain) throws HRESULTException {
        errCheck(_lib.Starshootg_put_MaxAutoExpoTimeAGain(_handle, maxTime, maxAGain));
    }
    
    public int[] get_MaxAutoExpoTimeAGain() throws HRESULTException {
        IntByReference p = new IntByReference();
        ShortByReference q = new ShortByReference();
        errCheck(_lib.Starshootg_get_MaxAutoExpoTimeAGain(_handle, p, q));
        return new int[] { p.getValue(), q.getValue() };
    }
    
    public void put_MinAutoExpoTimeAGain(int minTime, short minAGain) throws HRESULTException {
        errCheck(_lib.Starshootg_put_MinAutoExpoTimeAGain(_handle, minTime, minAGain));
    }
    
    public int[] get_MinAutoExpoTimeAGain() throws HRESULTException {
        IntByReference p = new IntByReference();
        ShortByReference q = new ShortByReference();
        errCheck(_lib.Starshootg_get_MinAutoExpoTimeAGain(_handle, p, q));
        return new int[] { p.getValue(), q.getValue() };
    }
    
    /* in microseconds */
    public int get_ExpoTime() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_ExpoTime(_handle, p));
        return p.getValue();
    }
    
    /* in microseconds */
    public void put_ExpoTime(int Time) throws HRESULTException {
        errCheck(_lib.Starshootg_put_ExpoTime(_handle, Time));
    }
    
    /* min, max, default */
    public int[] get_ExpTimeRange() throws HRESULTException {
        IntByReference p = new IntByReference();
        IntByReference q = new IntByReference();
        IntByReference r = new IntByReference();
        errCheck(_lib.Starshootg_get_ExpTimeRange(_handle, p, q, r));
        return new int[] { p.getValue(), q.getValue(), r.getValue() };
    }
    
    /* percent, such as 300 */
    public short get_ExpoAGain() throws HRESULTException{
        ShortByReference p = new ShortByReference();
        errCheck(_lib.Starshootg_get_ExpoAGain(_handle, p));
        return p.getValue();
    }
    
    /* percent */
    public void put_ExpoAGain(short AGain) throws HRESULTException {
        errCheck(_lib.Starshootg_put_ExpoAGain(_handle, AGain));
    }
    
    /* min, max, default */
    public short[] get_ExpoAGainRange() throws HRESULTException {
        ShortByReference p = new ShortByReference();
        ShortByReference q = new ShortByReference();
        ShortByReference r = new ShortByReference();
        errCheck(_lib.Starshootg_get_ExpoAGainRange(_handle, p, q, r));
        return new short[] { p.getValue(), q.getValue(), r.getValue() };
    }
    
    public void put_LevelRange(short[] aLow, short[] aHigh) throws HRESULTException {
        if (aLow.length != 4 || aHigh.length != 4)
            errCheck(HRESULTException.E_INVALIDARG);
        errCheck(_lib.Starshootg_put_LevelRange(_handle, aLow, aHigh));
    }
    
    public void get_LevelRange(short[] aLow, short[] aHigh) throws HRESULTException {
        if (aLow.length != 4 || aHigh.length != 4)
            errCheck(HRESULTException.E_INVALIDARG);
        errCheck(_lib.Starshootg_get_LevelRange(_handle, aLow, aHigh));
    }

    public void put_LevelRangeV2(short mode, int roiX, int roiY, int roiWidth, int roiHeight, short[] aLow, short[] aHigh) throws HRESULTException {
        if (aLow.length != 4 || aHigh.length != 4)
            errCheck(HRESULTException.E_INVALIDARG);
        RECT rc = new RECT();
        rc.left = roiX;
        rc.right = roiX + roiWidth;
        rc.top = roiY;
        rc.bottom = roiY + roiHeight;
        errCheck(_lib.Starshootg_put_LevelRangeV2(_handle, mode, rc, aLow, aHigh));
    }

    public int[] get_LevelRangeV2(short[] aLow, short[] aHigh) throws HRESULTException {
        if (aLow.length != 4 || aHigh.length != 4)
            errCheck(HRESULTException.E_INVALIDARG);
        ShortByReference pMode = new ShortByReference();
        RECT rc = new RECT();
        errCheck(_lib.Starshootg_get_LevelRangeV2(_handle, pMode, rc, aLow, aHigh));
        return new int[] { pMode.getValue(), rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top };
    }

    public void put_Hue(int Hue) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Hue(_handle, Hue));
    }
    
    public int get_Hue() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_Hue(_handle, p));
        return p.getValue();
    }
    
    public void put_Saturation(int Saturation) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Saturation(_handle, Saturation));
    }
    
    public int get_Saturation() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_Saturation(_handle, p));
        return p.getValue();
    }
    
    public void put_Brightness(int Brightness) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Brightness(_handle, Brightness));
    }
    
    public int get_Brightness() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_Brightness(_handle, p));
        return p.getValue();
    }
    
    public int get_Contrast() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_Contrast(_handle, p));
        return p.getValue();
    }
    
    public void put_Contrast(int Contrast) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Contrast(_handle, Contrast));
    }
    
    public int get_Gamma() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_Gamma(_handle, p));
        return p.getValue();
    }
    
    public void put_Gamma(int Gamma) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Gamma(_handle, Gamma));
    }
    
    /* monochromatic mode */
    public boolean get_Chrome() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_Chrome(_handle, p));
        return (p.getValue() != 0);
    }
    
    public void put_Chrome(boolean bChrome) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Chrome(_handle, bChrome ? 1 : 0));
    }
    
    /* vertical flip */
    public boolean get_VFlip() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_VFlip(_handle, p));
        return (p.getValue() != 0);
    }
    
    public void put_VFlip(boolean bVFlip) throws HRESULTException {
        errCheck(_lib.Starshootg_put_VFlip(_handle, bVFlip ? 1 : 0));
    }
    
    public boolean get_HFlip() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_HFlip(_handle, p));
        return (p.getValue() != 0);
    }
    
    /* horizontal flip */
    public void put_HFlip(boolean bHFlip) throws HRESULTException {
        errCheck(_lib.Starshootg_put_HFlip(_handle, bHFlip ? 1 : 0));
    }
    
    /* negative film */
    public boolean get_Negative() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_Negative(_handle, p));
        return (p.getValue() != 0);
    }
    
    /* negative film */
    public void put_Negative(boolean bNegative) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Negative(_handle, bNegative ? 1 : 0));
    }
    
    public void put_Speed(short nSpeed) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Speed(_handle, nSpeed));
    }
    
    public short get_Speed() throws HRESULTException {
        ShortByReference p = new ShortByReference();
        errCheck(_lib.Starshootg_get_Speed(_handle, p));
        return p.getValue();
    }
    
    /* power supply: 
            0 -> 60HZ AC
            1 -> 50Hz AC
            2 -> DC
    */
    public void put_HZ(int nHZ) throws HRESULTException {
        errCheck(_lib.Starshootg_put_HZ(_handle, nHZ));
    }
    
    public int get_HZ() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_HZ(_handle, p));
        return p.getValue();
    }
    
    /* skip or bin */
    public void put_Mode(boolean bSkip) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Mode(_handle, bSkip ? 1 : 0));
    }
    
    public boolean get_Mode() throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_Mode(_handle, p));
        return (p.getValue() != 0);
    }
    
    /* White Balance, Temp/Tint mode */
    public void put_TempTint(int nTemp, int nTint) throws HRESULTException {
        errCheck(_lib.Starshootg_put_TempTint(_handle, nTemp, nTint));
    }
    
    /* White Balance, Temp/Tint mode */
    public int[] get_TempTint() throws HRESULTException {
        IntByReference p = new IntByReference();
        IntByReference q = new IntByReference();
        errCheck(_lib.Starshootg_get_TempTint(_handle, p, q));
        return new int[] { p.getValue(), q.getValue() };
    }
    
    /* White Balance, RGB Gain Mode */
    public void put_WhiteBalanceGain(int[] aGain) throws HRESULTException {
        if (aGain.length != 3)
            errCheck(HRESULTException.E_INVALIDARG);
        errCheck(_lib.Starshootg_put_WhiteBalanceGain(_handle, aGain));
    }
    
    /* White Balance, RGB Gain Mode */
    public void get_WhiteBalanceGain(int[] aGain) throws HRESULTException {
        if (aGain.length != 3)
            errCheck(HRESULTException.E_INVALIDARG);
        errCheck(_lib.Starshootg_get_WhiteBalanceGain(_handle, aGain));
    }
    
    public void put_AWBAuxRect(int X, int Y, int Width, int Height) throws HRESULTException {
        RECT rc = new RECT();
        rc.left = X;
        rc.right = X + Width;
        rc.top = Y;
        rc.bottom = Y + Height;
        errCheck(_lib.Starshootg_put_AWBAuxRect(_handle, rc));
    }
    
    /* left, top, width, height */
    public int[] get_AWBAuxRect() throws HRESULTException {
        RECT rc = new RECT();
        errCheck(_lib.Starshootg_get_AWBAuxRect(_handle, rc));
        return new int[] { rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top };
    }
    
    public void put_AEAuxRect(int X, int Y, int Width, int Height) throws HRESULTException {
        RECT rc = new RECT();
        rc.left = X;
        rc.right = X + Width;
        rc.top = Y;
        rc.bottom = Y + Height;
        errCheck(_lib.Starshootg_put_AEAuxRect(_handle, rc));
    }
    
    /* left, top, width, height */
    public int[] get_AEAuxRect() throws HRESULTException {
        RECT rc = new RECT();
        errCheck(_lib.Starshootg_get_AEAuxRect(_handle, rc));
        return new int[] { rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top };
    }
    
    public void put_BlackBalance(short[] aSub) throws HRESULTException {
        if (aSub.length != 3)
            errCheck(HRESULTException.E_INVALIDARG);
        errCheck(_lib.Starshootg_put_BlackBalance(_handle, aSub));
    }
    
    public short[] get_BlackBalance() throws HRESULTException {
        short[] p = new short[3];
        errCheck(_lib.Starshootg_get_BlackBalance(_handle, p));
        return p;
    }
    
    public void put_ABBAuxRect(int X, int Y, int Width, int Height) throws HRESULTException {
        RECT rc = new RECT();
        rc.left = X;
        rc.right = X + Width;
        rc.top = Y;
        rc.bottom = Y + Height;
        errCheck(_lib.Starshootg_put_ABBAuxRect(_handle, rc));
    }
    
    /* left, top, width, height */
    public int[] get_ABBAuxRect() throws HRESULTException {
        RECT rc = new RECT();
        errCheck(_lib.Starshootg_get_ABBAuxRect(_handle, rc));
        return new int[] { rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top };
    }
    
    /* width, height */
    public int[] get_StillResolution(int nResolutionIndex) throws HRESULTException {
        IntByReference p = new IntByReference();
        IntByReference q = new IntByReference();
        errCheck(_lib.Starshootg_get_StillResolution(_handle, nResolutionIndex, p, q));
        return new int[] { p.getValue(), q.getValue() };
    }
    
    /* led state:
        iLed: Led index, (0, 1, 2, ...)
        iState: 1 -> Ever bright; 2 -> Flashing; other -> Off
        iPeriod: Flashing Period (>= 500ms)
    */
    public void put_LEDState(short iLed, short iState, short iPeriod) throws HRESULTException {
        errCheck(_lib.Starshootg_put_LEDState(_handle, iLed, iState, iPeriod));
    }
    
    public void write_EEPROM(int addr, ByteBuffer pBuffer) throws HRESULTException {
        if (pBuffer.isDirect())
            errCheck(_lib.Starshootg_write_EEPROM(_handle, addr, Native.getDirectBufferPointer(pBuffer), pBuffer.remaining()));
        else if (pBuffer.hasArray())
            write_EEPROM(addr, pBuffer.array());
        else
            errCheck(HRESULTException.E_INVALIDARG);
    }
    
    public void write_EEPROM(int addr, byte[] pBuffer) throws HRESULTException {
        errCheck(_lib.Starshootg_write_EEPROMArray(_handle, addr, pBuffer, pBuffer.length));
    }
    
    public int read_EEPROM(int addr, ByteBuffer pBuffer) throws HRESULTException {
        if (pBuffer.isDirect())
            return errCheck(_lib.Starshootg_read_EEPROM(_handle, addr, Native.getDirectBufferPointer(pBuffer), pBuffer.remaining()));
        else if (pBuffer.hasArray())
            return read_EEPROM(addr, pBuffer.array());
        else
            return errCheck(HRESULTException.E_INVALIDARG);
    }
    
    public int read_EEPROM(int addr, byte[] pBuffer) throws HRESULTException {
        return errCheck(_lib.Starshootg_read_EEPROMArray(_handle, addr, pBuffer, pBuffer.length));
    }
    
    public void write_Pipe(int pipeNum, ByteBuffer pBuffer) throws HRESULTException {
        if (pBuffer.isDirect())
            errCheck(_lib.Starshootg_write_Pipe(_handle, pipeNum, Native.getDirectBufferPointer(pBuffer), pBuffer.remaining()));
        else if (pBuffer.hasArray())
            write_Pipe(pipeNum, pBuffer.array());
        else
            errCheck(HRESULTException.E_INVALIDARG);
    }
    
    public void write_Pipe(int pipeNum, byte[] pBuffer) throws HRESULTException {
        errCheck(_lib.Starshootg_write_PipeArray(_handle, pipeNum, pBuffer, pBuffer.length));
    }
    
    public int read_Pipe(int pipeNum, ByteBuffer pBuffer) throws HRESULTException {
        if (pBuffer.isDirect())
            return errCheck(_lib.Starshootg_read_Pipe(_handle, pipeNum, Native.getDirectBufferPointer(pBuffer), pBuffer.remaining()));
        else if (pBuffer.hasArray())
            return read_Pipe(pipeNum, pBuffer.array());
        else
            return errCheck(HRESULTException.E_INVALIDARG);
    }
    
    public int read_Pipe(int pipeNum, byte[] pBuffer) throws HRESULTException {
        return errCheck(_lib.Starshootg_read_PipeArray(_handle, pipeNum, pBuffer, pBuffer.length));
    }
    
    public void feed_Pipe(int pipeNum) throws HRESULTException {
        errCheck(_lib.Starshootg_feed_Pipe(_handle, pipeNum));
    }
    
    public void write_UART(ByteBuffer pBuffer) throws HRESULTException {
        if (pBuffer.isDirect())
            errCheck(_lib.Starshootg_write_UART(_handle, Native.getDirectBufferPointer(pBuffer), pBuffer.remaining()));
        else if (pBuffer.hasArray())
            write_UART(pBuffer.array());
        else
            errCheck(HRESULTException.E_INVALIDARG);
    }
    
    public void write_UART(byte[] pBuffer) throws HRESULTException {
        errCheck(_lib.Starshootg_write_UARTArray(_handle, pBuffer, pBuffer.length));
    }
    
    public int read_UART(ByteBuffer pBuffer) throws HRESULTException {
        if (pBuffer.isDirect())
            return errCheck(_lib.Starshootg_read_UART(_handle, Native.getDirectBufferPointer(pBuffer), pBuffer.remaining()));
        else if (pBuffer.hasArray())
            return read_UART(pBuffer.array());
        else
            return errCheck(HRESULTException.E_INVALIDARG);
    }
    
    public int read_UART(byte[] pBuffer) throws HRESULTException {
        return errCheck(_lib.Starshootg_read_UARTArray(_handle, pBuffer, pBuffer.length));
    }
    
    public void put_Option(int iOption, int iValue) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Option(_handle, iOption, iValue));
    }
    
    public int get_Option(int iOption) throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_get_Option(_handle, iOption, p));
        return p.getValue();
    }
    
    public void put_Linear(byte[] v8, short[] v16) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Linear(_handle, v8, v16));
    }
    
    public void put_Curve(byte[] v8, short[] v16) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Curve(_handle, v8, v16));
    }
    
    public void put_ColorMatrix(double[] v) throws HRESULTException {
        if (v.length != 9)
            errCheck(HRESULTException.E_INVALIDARG);
        errCheck(_lib.Starshootg_put_ColorMatrix(_handle, v));
    }
    
    public void put_InitWBGain(short[] v) throws HRESULTException {
        if (v.length != 3)
            errCheck(HRESULTException.E_INVALIDARG);
        errCheck(_lib.Starshootg_put_InitWBGain(_handle, v));
    }
    
    /* get the temperature of the sensor, in 0.1 degrees Celsius (32 means 3.2 degrees Celsius, -35 means -3.5 degree Celsius) */
    public short get_Temperature() throws HRESULTException {
        ShortByReference p = new ShortByReference();
        errCheck(_lib.Starshootg_get_Temperature(_handle, p));
        return p.getValue();
    }
    
    /* set the target temperature of the sensor or TEC, in 0.1 degrees Celsius (32 means 3.2 degrees Celsius, -35 means -3.5 degree Celsius) */
    public void put_Temperature(short nTemperature) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Temperature(_handle, nTemperature));
    }
    
    /* xOffset, yOffset, xWidth, yHeight: must be even numbers */
    public void put_Roi(int xOffset, int yOffset, int xWidth, int yHeight) throws HRESULTException {
        errCheck(_lib.Starshootg_put_Roi(_handle, xOffset, yOffset, xWidth, yHeight));
    }
    
    /* xOffset, yOffset, xWidth, yHeight */
    public int[] get_Roi() throws HRESULTException {
        IntByReference p = new IntByReference();
        IntByReference q = new IntByReference();
        IntByReference r = new IntByReference();
        IntByReference s = new IntByReference();
        errCheck(_lib.Starshootg_get_Roi(_handle, p, q, r, s));
        return new int[] { p.getValue(), q.getValue(), r.getValue(), s.getValue() };
    }
    
    /*
        get the frame rate: framerate (fps) = Frame * 1000.0 / nTime
        Frame, Time, TotalFrame
    */
    public int[] get_FrameRate() throws HRESULTException {
        IntByReference p = new IntByReference();
        IntByReference q = new IntByReference();
        IntByReference r = new IntByReference();
        errCheck(_lib.Starshootg_get_FrameRate(_handle, p, q, r));
        return new int[] { p.getValue(), q.getValue(), r.getValue() };
    }
    
    public void LevelRangeAuto() throws HRESULTException {
        errCheck(_lib.Starshootg_LevelRangeAuto(_handle));
    }
    
    /* Auto White Balance "Once", Temp/Tint Mode */
    public void AwbOnce() throws HRESULTException {
        errCheck(_lib.Starshootg_AwbOnce(_handle, null, null));
    }
    
    public void AwbOnePush() throws HRESULTException {
        AwbOnce();
    }
    
    /* Auto White Balance, RGB Gain Mode */
    public void AwbInit() throws HRESULTException {
        errCheck(_lib.Starshootg_AwbInit(_handle, null, null));
    }
    
    public void AbbOnce() throws HRESULTException {
        errCheck(_lib.Starshootg_AbbOnce(_handle, null, null));
    }
    
    public void AbbOnePush() throws HRESULTException {
        AbbOnce();
    }
    
    public void FfcOnce() throws HRESULTException {
        errCheck(_lib.Starshootg_FfcOnce(_handle));
    }
    
    public void FfcOnePush() throws HRESULTException {
        FfcOnce();
    }
    
    public void DfcOnce() throws HRESULTException {
        errCheck(_lib.Starshootg_DfcOnce(_handle));
    }
    
    public void DfcOnePush() throws HRESULTException {
        DfcOnce();
    }
    
    public void FfcExport(String filepath) throws HRESULTException {
        if (Platform.isWindows())
            errCheck(((WinLibrary)_lib).Starshootg_FfcExport(_handle, new WString(filepath)));
        else
            errCheck(((CLibrary)_lib).Starshootg_FfcExport(_handle, filepath));
    }
    
    public void FfcImport(String filepath) throws HRESULTException {
        if (Platform.isWindows())
            errCheck(((WinLibrary)_lib).Starshootg_FfcImport(_handle, new WString(filepath)));
        else
            errCheck(((CLibrary)_lib).Starshootg_FfcImport(_handle, filepath));
    }
    
    public void DfcExport(String filepath) throws HRESULTException {
        if (Platform.isWindows())
            errCheck(((WinLibrary)_lib).Starshootg_DfcExport(_handle, new WString(filepath)));
        else
            errCheck(((CLibrary)_lib).Starshootg_DfcExport(_handle, filepath));
    }
    
    public void DfcImport(String filepath) throws HRESULTException {
        if (Platform.isWindows())
            errCheck(((WinLibrary)_lib).Starshootg_DfcImport(_handle, new WString(filepath)));
        else
            errCheck(((CLibrary)_lib).Starshootg_DfcImport(_handle, filepath));
    }
    
    public int IoControl(int ioLineNumber, int eType, int outVal) throws HRESULTException {
        IntByReference p = new IntByReference();
        errCheck(_lib.Starshootg_IoControl(_handle, ioLineNumber, eType, outVal, p));
        return p.getValue();
    }
    
    public void get_AfParam(AfParam pAfParam) throws HRESULTException {
        errCheck(_lib.Starshootg_get_AfParam(_handle, pAfParam));
    }
    
    /*
        simulate replug:
        return > 0, the number of device has been replug
        return = 0, no device found
        return E_ACCESSDENIED if without UAC Administrator privileges
        for each device found, it will take about 3 seconds
    */
    public static void Replug(String id) throws HRESULTException {
        if (Platform.isWindows())
            errCheck(((WinLibrary)_lib).Starshootg_Replug(new WString(id)));
        else
            errCheck(((CLibrary)_lib).Starshootg_Replug(id));
    }

    private static void OnProgressCallback(int percent, long pCallbackCtx) {
        Object o = _hash.get((int)pCallbackCtx);
        if (o instanceof IProgressCallback) {
            IProgressCallback t = (IProgressCallback)o;
            if (t != null)
                t.onProgress(percent);
        }
    }

    /*
        firmware update:
            camId: camera ID
            filePath: ufw file full path
            pFun, pCtx: progress percent callback
        Please do not unplug the camera or lost power during the upgrade process, this is very very important.
        Once an unplugging or power outage occurs during the upgrade process, the camera will no longer be available and can only be returned to the factory for repair.
    */
    public static void Update(String camId, String filePath, IProgressCallback cb) throws HRESULTException {
        int id = _clsid++;
        _hash.put(id, cb);
        if (Platform.isWindows()) {
            WinLibrary.PROGRESS_CALLBACK p = new WinLibrary.PROGRESS_CALLBACK() {
                @Override
                public void invoke(int percent, long pCallbackCtx) {
                    OnProgressCallback(percent, pCallbackCtx);
                }
            };
            Native.setCallbackThreadInitializer(p, new CallbackThreadInitializer(false, false, "progressCallback"));
            errCheck(((WinLibrary)_lib).Starshootg_Update(new WString(camId), new WString(filePath), p, id));
        }
        else {
            CLibrary.PROGRESS_CALLBACK p = new CLibrary.PROGRESS_CALLBACK() {
                @Override
                public void invoke(int percent, long pCallbackCtx) {
                    OnProgressCallback(percent, pCallbackCtx);
                }
            };
            Native.setCallbackThreadInitializer(p, new CallbackThreadInitializer(false, false, "progressCallback"));
            errCheck(((CLibrary)_lib).Starshootg_Update(camId, filePath, p, id));
        }
    }
}
