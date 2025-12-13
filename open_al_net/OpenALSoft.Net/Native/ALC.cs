using System;
using System.Runtime.InteropServices;

namespace OpenALSoft.Net.Native
{
    /// <summary>
    /// Native bindings for OpenAL Soft ALC (Context) API.
    /// </summary>
    public static class ALC
    {
        private const string Lib = "soft_oal.dll";
        private const CallingConvention CallConvention = CallingConvention.Cdecl;

        // ALC types
        // ALCdevice and ALCcontext are opaque pointers, so we use IntPtr.
        // ALCboolean is char (1 byte).
        // ALCchar is char.
        // ALCint is int.
        // ALCenum is int.

        public const int FALSE = 0;
        public const int TRUE = 1;

        // Context attributes
        public const int FREQUENCY = 0x1007;
        public const int REFRESH = 0x1008;
        public const int SYNC = 0x1009;
        public const int MONO_SOURCES = 0x1010;
        public const int STEREO_SOURCES = 0x1011;

        // Errors
        public const int NO_ERROR = 0;
        public const int INVALID_DEVICE = 0xA001;
        public const int INVALID_CONTEXT = 0xA002;
        public const int INVALID_ENUM = 0xA003;
        public const int INVALID_VALUE = 0xA004;
        public const int OUT_OF_MEMORY = 0xA005;

        // Version
        public const int MAJOR_VERSION = 0x1000;
        public const int MINOR_VERSION = 0x1001;

        // Attributes
        public const int ATTRIBUTES_SIZE = 0x1002;
        public const int ALL_ATTRIBUTES = 0x1003;

        // String queries
        public const int DEFAULT_DEVICE_SPECIFIER = 0x1004;
        public const int DEVICE_SPECIFIER = 0x1005;
        public const int EXTENSIONS = 0x1006;

        // Capture extension
        public const int EXT_CAPTURE = 1;
        public const int CAPTURE_DEVICE_SPECIFIER = 0x310;
        public const int CAPTURE_DEFAULT_DEVICE_SPECIFIER = 0x311;
        public const int CAPTURE_SAMPLES = 0x312;

        // Enumerate All extension
        public const int ENUMERATE_ALL_EXT = 1;
        public const int DEFAULT_ALL_DEVICES_SPECIFIER = 0x1012;
        public const int ALL_DEVICES_SPECIFIER = 0x1013;

        // ALC_SOFT_HRTF extension
        public const int HRTF_SOFT = 0x1992;
        public const int DONT_CARE_SOFT = 0x0002;
        public const int HRTF_STATUS_SOFT = 0x1993;
        public const int HRTF_DISABLED_SOFT = 0x0000;
        public const int HRTF_ENABLED_SOFT = 0x0001;
        public const int HRTF_DENIED_SOFT = 0x0002;
        public const int HRTF_REQUIRED_SOFT = 0x0003;
        public const int HRTF_HEADPHONES_DETECTED_SOFT = 0x0004;
        public const int HRTF_UNSUPPORTED_FORMAT_SOFT = 0x0005;
        public const int NUM_HRTF_SPECIFIERS_SOFT = 0x1994;
        public const int HRTF_SPECIFIER_SOFT = 0x1995;
        public const int HRTF_ID_SOFT = 0x1996;

        // Context Management
        [DllImport(Lib, EntryPoint = "alcCreateContext", CallingConvention = CallConvention)]
        public static extern IntPtr CreateContext(IntPtr device, [In] int[] attrlist);

        [DllImport(Lib, EntryPoint = "alcMakeContextCurrent", CallingConvention = CallConvention)]
        public static extern bool MakeContextCurrent(IntPtr context);

        [DllImport(Lib, EntryPoint = "alcProcessContext", CallingConvention = CallConvention)]
        public static extern void ProcessContext(IntPtr context);

        [DllImport(Lib, EntryPoint = "alcSuspendContext", CallingConvention = CallConvention)]
        public static extern void SuspendContext(IntPtr context);

        [DllImport(Lib, EntryPoint = "alcDestroyContext", CallingConvention = CallConvention)]
        public static extern void DestroyContext(IntPtr context);

        [DllImport(Lib, EntryPoint = "alcGetCurrentContext", CallingConvention = CallConvention)]
        public static extern IntPtr GetCurrentContext();

        [DllImport(Lib, EntryPoint = "alcGetContextsDevice", CallingConvention = CallConvention)]
        public static extern IntPtr GetContextsDevice(IntPtr context);

        // Device Management
        [DllImport(Lib, EntryPoint = "alcOpenDevice", CallingConvention = CallConvention, CharSet = CharSet.Ansi)]
        public static extern IntPtr OpenDevice(string devicename);

        [DllImport(Lib, EntryPoint = "alcCloseDevice", CallingConvention = CallConvention)]
        public static extern bool CloseDevice(IntPtr device);

        // Error Support
        [DllImport(Lib, EntryPoint = "alcGetError", CallingConvention = CallConvention)]
        public static extern int GetError(IntPtr device);

        // Extension Support
        [DllImport(Lib, EntryPoint = "alcIsExtensionPresent", CallingConvention = CallConvention, CharSet = CharSet.Ansi)]
        public static extern bool IsExtensionPresent(IntPtr device, string extname);

        [DllImport(Lib, EntryPoint = "alcGetProcAddress", CallingConvention = CallConvention, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr device, string funcname);

        [DllImport(Lib, EntryPoint = "alcGetEnumValue", CallingConvention = CallConvention, CharSet = CharSet.Ansi)]
        public static extern int GetEnumValue(IntPtr device, string enumname);

        // Query Functions
        [DllImport(Lib, EntryPoint = "alcGetString", CallingConvention = CallConvention, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetString(IntPtr device, int param);

        [DllImport(Lib, EntryPoint = "alcGetIntegerv", CallingConvention = CallConvention)]
        public static extern void GetIntegerv(IntPtr device, int param, int size, [Out] int[] values);

        // Capture Functions
        [DllImport(Lib, EntryPoint = "alcCaptureOpenDevice", CallingConvention = CallConvention, CharSet = CharSet.Ansi)]
        public static extern IntPtr CaptureOpenDevice(string devicename, uint frequency, int format, int buffersize);

        [DllImport(Lib, EntryPoint = "alcCaptureCloseDevice", CallingConvention = CallConvention)]
        public static extern bool CaptureCloseDevice(IntPtr device);

        [DllImport(Lib, EntryPoint = "alcCaptureStart", CallingConvention = CallConvention)]
        public static extern void CaptureStart(IntPtr device);

        [DllImport(Lib, EntryPoint = "alcCaptureStop", CallingConvention = CallConvention)]
        public static extern void CaptureStop(IntPtr device);

        [DllImport(Lib, EntryPoint = "alcCaptureSamples", CallingConvention = CallConvention)]
        public static extern void CaptureSamples(IntPtr device, IntPtr buffer, int samples);

        // ALC_SOFT_pause_device
        [UnmanagedFunctionPointer(CallConvention)]
        public delegate void AlcDevicePauseSoftDelegate(IntPtr device);
        [UnmanagedFunctionPointer(CallConvention)]
        public delegate void AlcDeviceResumeSoftDelegate(IntPtr device);

        // ALC_SOFT_reopen_device
        [UnmanagedFunctionPointer(CallConvention)]
        public delegate bool AlcReopenDeviceSoftDelegate(IntPtr device, string deviceName, [In] int[] attribs);

        // ALC_SOFT_device_clock
        public const int DEVICE_CLOCK_SOFT = 0x1600;
        public const int DEVICE_LATENCY_SOFT = 0x1601;
        public const int DEVICE_CLOCK_LATENCY_SOFT = 0x1602;
        [UnmanagedFunctionPointer(CallConvention)]
        public delegate void AlcGetInteger64vSoftDelegate(IntPtr device, int pname, int size, [Out] long[] values);

        // ALC_SOFT_output_limiter
        public const int OUTPUT_LIMITER_SOFT = 0x199A;

        // ALC_SOFT_output_mode
        public const int OUTPUT_MODE_SOFT = 0x19AC;
        public const int ANY_SOFT = 0x19AD;
        public const int STEREO_BASIC_SOFT = 0x19AE;
        public const int STEREO_UHJ_SOFT = 0x19AF;
        public const int STEREO_HRTF_SOFT = 0x19B2;
        public const int SURROUND_5_1_SOFT = 0x1504;
        public const int SURROUND_6_1_SOFT = 0x1505;
        public const int SURROUND_7_1_SOFT = 0x1506;

        // ALC_SOFT_HRTF Extension Functions
        // These are usually retrieved via alcGetProcAddress, but we can try DllImport if they are exported.
        // If not, we need a delegate and GetProcAddress.
        // OpenAL Soft usually exports them if built with them, but standard practice is GetProcAddress for extensions.
        // However, for simplicity in a wrapper, we can try DllImport first or use a helper to load them.
        // Let's define delegates for them just in case.

        [UnmanagedFunctionPointer(CallConvention)]
        public delegate IntPtr AlcGetStringiSoftDelegate(IntPtr device, int paramName, int index);

        [UnmanagedFunctionPointer(CallConvention)]
        public delegate bool AlcResetDeviceSoftDelegate(IntPtr device, [In] int[] attribs);
    }
}
