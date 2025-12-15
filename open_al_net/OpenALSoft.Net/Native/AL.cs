using System;
using System.Runtime.InteropServices;

namespace OpenALSoft.Net.Native
{
    /// <summary>
    /// Native bindings for OpenAL Soft AL (Core) API.
    /// </summary>
    public static class AL
    {
        private const string Lib = "soft_oal.dll";
        private const CallingConvention CallConvention = CallingConvention.Cdecl;

        // AL Types
        // ALboolean is char (1 byte)
        // ALchar is char
        // ALbyte is signed char
        // ALubyte is unsigned char
        // ALshort is short
        // ALushort is unsigned short
        // ALint is int
        // ALuint is unsigned int
        // ALsizei is int
        // ALenum is int
        // ALfloat is float
        // ALdouble is double

        public const int NONE = 0;
        public const int FALSE = 0;
        public const int TRUE = 1;

        // Source Relative
        public const int SOURCE_RELATIVE = 0x202;

        // Cone
        public const int CONE_INNER_ANGLE = 0x1001;
        public const int CONE_OUTER_ANGLE = 0x1002;

        // Pitch
        public const int PITCH = 0x1003;

        // Position, Direction, Velocity
        public const int POSITION = 0x1004;
        public const int DIRECTION = 0x1005;
        public const int VELOCITY = 0x1006;

        // Looping
        public const int LOOPING = 0x1007;

        // Buffer
        public const int BUFFER = 0x1009;

        // Gain
        public const int GAIN = 0x100A;
        public const int MIN_GAIN = 0x100D;
        public const int MAX_GAIN = 0x100E;

        // Orientation
        public const int ORIENTATION = 0x100F;

        // Source State
        public const int SOURCE_STATE = 0x1010;
        public const int INITIAL = 0x1011;
        public const int PLAYING = 0x1012;
        public const int PAUSED = 0x1013;
        public const int STOPPED = 0x1014;

        // Buffers Queued
        public const int BUFFERS_QUEUED = 0x1015;
        public const int BUFFERS_PROCESSED = 0x1016;

        // Distance Model
        public const int DISTANCE_MODEL = 0xD000;
        public const int INVERSE_DISTANCE = 0xD001;
        public const int INVERSE_DISTANCE_CLAMPED = 0xD002;
        public const int LINEAR_DISTANCE = 0xD003;
        public const int LINEAR_DISTANCE_CLAMPED = 0xD004;
        public const int EXPONENT_DISTANCE = 0xD005;
        public const int EXPONENT_DISTANCE_CLAMPED = 0xD006;

        public const int REFERENCE_DISTANCE = 0x1020;
        public const int ROLLOFF_FACTOR = 0x1021;
        public const int CONE_OUTER_GAIN = 0x1022;
        public const int MAX_DISTANCE = 0x1023;

        public const int DOPPLER_FACTOR = 0xC000;
        public const int SPEED_OF_SOUND = 0xC003;

        // Offsets
        public const int SEC_OFFSET = 0x1024;
        public const int SAMPLE_OFFSET = 0x1025;
        public const int BYTE_OFFSET = 0x1026;

        // Source Type
        public const int SOURCE_TYPE = 0x1027;
        public const int STATIC = 0x1028;
        public const int STREAMING = 0x1029;
        public const int UNDETERMINED = 0x1030;

        // Formats
        public const int FORMAT_MONO8 = 0x1100;
        public const int FORMAT_MONO16 = 0x1101;
        public const int FORMAT_STEREO8 = 0x1102;
        public const int FORMAT_STEREO16 = 0x1103;

        // Buffer Attributes
        public const int FREQUENCY = 0x2001;
        public const int BITS = 0x2002;
        public const int CHANNELS = 0x2003;
        public const int SIZE = 0x2004;

        // Errors
        public const int NO_ERROR = 0;
        public const int INVALID_NAME = 0xA001;
        public const int INVALID_ENUM = 0xA002;
        public const int INVALID_VALUE = 0xA003;
        public const int INVALID_OPERATION = 0xA004;
        public const int OUT_OF_MEMORY = 0xA005;

        // String Queries
        public const int VENDOR = 0xB001;
        public const int VERSION = 0xB002;
        public const int RENDERER = 0xB003;
        public const int EXTENSIONS = 0xB004;

        // AL_SOFT_source_spatialize
        public const int SOURCE_SPATIALIZE_SOFT = 0x1214;
        public const int AUTO_SOFT = 0x0002;

        // AL_SOFT_direct_channels
        public const int DIRECT_CHANNELS_SOFT = 0x1033;

        // AL_EXT_STEREO_ANGLES
        public const int STEREO_ANGLES = 0x1030;

        // AL_EXT_SOURCE_RADIUS
        public const int SOURCE_RADIUS = 0x1031;

        // AL_SOFT_source_latency
        public const int SAMPLE_OFFSET_LATENCY_SOFT = 0x1200;

        // AL_SOFT_gain_clamp_ex
        public const int GAIN_LIMIT_SOFT = 0x200E;

        // AL_SOFT_source_resampler
        public const int SOURCE_RESAMPLER_SOFT = 0x1212;
        public const int RESAMPLER_DEFAULT_SOFT = 0x0000;
        public const int RESAMPLER_POINT_SOFT = 0x0001;
        public const int RESAMPLER_LINEAR_SOFT = 0x0002;
        public const int RESAMPLER_CUBIC_SOFT = 0x0003;
        public const int RESAMPLER_BSINC12_SOFT = 0x0004;
        public const int RESAMPLER_BSINC24_SOFT = 0x0005;

        // AL_SOFT_source_start_delay
        // (No constants, just functions)

        // AL_SOFT_direct_channels_remix
        public const int DROP_UNMATCHED_SOFT = 0x0001;
        public const int REMIX_UNMATCHED_SOFT = 0x0002;

        // EFX Source Properties
        public const int DIRECT_FILTER = 0x20005;
        public const int AUXILIARY_SEND_FILTER = 0x20006;
        public const int AIR_ABSORPTION_FACTOR = 0x20007;
        public const int ROOM_ROLLOFF_FACTOR = 0x20008;
        public const int CONE_OUTER_GAINHF = 0x20009;
        public const int DIRECT_FILTER_GAINHF_AUTO = 0x2000A;
        public const int AUXILIARY_SEND_FILTER_GAIN_AUTO = 0x2000B;
        public const int AUXILIARY_SEND_FILTER_GAINHF_AUTO = 0x2000C;

        // Renderer State
        [DllImport(Lib, EntryPoint = "alEnable", CallingConvention = CallConvention)]
        public static extern void Enable(int capability);

        [DllImport(Lib, EntryPoint = "alDisable", CallingConvention = CallConvention)]
        public static extern void Disable(int capability);

        [DllImport(Lib, EntryPoint = "alIsEnabled", CallingConvention = CallConvention)]
        public static extern bool IsEnabled(int capability);

        // Context State
        [DllImport(Lib, EntryPoint = "alDistanceModel", CallingConvention = CallConvention)]
        public static extern void DistanceModel(int distanceModel);

        [DllImport(Lib, EntryPoint = "alSpeedOfSound", CallingConvention = CallConvention)]
        public static extern void SpeedOfSound(float value);

        [DllImport(Lib, EntryPoint = "alDopplerFactor", CallingConvention = CallConvention)]
        public static extern void DopplerFactor(float value);

        [DllImport(Lib, EntryPoint = "alDopplerVelocity", CallingConvention = CallConvention)]
        public static extern void DopplerVelocity(float value);

        [DllImport(Lib, EntryPoint = "alGetBooleanv", CallingConvention = CallConvention)]
        public static extern void GetBooleanv(int param, [Out] bool[] values);

        // Extension Delegates
        [UnmanagedFunctionPointer(CallConvention)]
        public delegate void AlDeferUpdatesSoftDelegate();

        [UnmanagedFunctionPointer(CallConvention)]
        public delegate void AlProcessUpdatesSoftDelegate();

        [UnmanagedFunctionPointer(CallConvention)]
        public delegate void AlGetSourcei64vSoftDelegate(uint source, int param, [Out] long[] values);

        [UnmanagedFunctionPointer(CallConvention)]
        public delegate IntPtr AlGetStringiSoftDelegate(uint source, int param, int index);

        [UnmanagedFunctionPointer(CallConvention)]
        public delegate void AlSourcePlayAtTimeSoftDelegate(uint source, long start);

        [UnmanagedFunctionPointer(CallConvention)]
        public delegate void AlSourcePlayAtTimevSoftDelegate(int n, [In] uint[] sources, long start);

        [DllImport(Lib, EntryPoint = "alGetIntegerv", CallingConvention = CallConvention)]
        public static extern void GetIntegerv(int param, [Out] int[] values);

        [DllImport(Lib, EntryPoint = "alGetFloatv", CallingConvention = CallConvention)]
        public static extern void GetFloatv(int param, [Out] float[] values);

        [DllImport(Lib, EntryPoint = "alGetDoublev", CallingConvention = CallConvention)]
        public static extern void GetDoublev(int param, [Out] double[] values);

        [DllImport(Lib, EntryPoint = "alGetBoolean", CallingConvention = CallConvention)]
        public static extern bool GetBoolean(int param);

        [DllImport(Lib, EntryPoint = "alGetInteger", CallingConvention = CallConvention)]
        public static extern int GetInteger(int param);

        [DllImport(Lib, EntryPoint = "alGetFloat", CallingConvention = CallConvention)]
        public static extern float GetFloat(int param);

        [DllImport(Lib, EntryPoint = "alGetDouble", CallingConvention = CallConvention)]
        public static extern double GetDouble(int param);

        // Error
        [DllImport(Lib, EntryPoint = "alGetError", CallingConvention = CallConvention)]
        public static extern int GetError();

        // Extension Support
        [DllImport(Lib, EntryPoint = "alIsExtensionPresent", CallingConvention = CallConvention, CharSet = CharSet.Ansi)]
        public static extern bool IsExtensionPresent(string extname);

        [DllImport(Lib, EntryPoint = "alGetProcAddress", CallingConvention = CallConvention, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(string fname);

        [DllImport(Lib, EntryPoint = "alGetEnumValue", CallingConvention = CallConvention, CharSet = CharSet.Ansi)]
        public static extern int GetEnumValue(string ename);

        // Listener
        [DllImport(Lib, EntryPoint = "alListenerf", CallingConvention = CallConvention)]
        public static extern void Listenerf(int param, float value);

        [DllImport(Lib, EntryPoint = "alListener3f", CallingConvention = CallConvention)]
        public static extern void Listener3f(int param, float value1, float value2, float value3);

        [DllImport(Lib, EntryPoint = "alListenerfv", CallingConvention = CallConvention)]
        public static extern void Listenerfv(int param, [In] float[] values);

        [DllImport(Lib, EntryPoint = "alListeneri", CallingConvention = CallConvention)]
        public static extern void Listeneri(int param, int value);

        [DllImport(Lib, EntryPoint = "alListener3i", CallingConvention = CallConvention)]
        public static extern void Listener3i(int param, int value1, int value2, int value3);

        [DllImport(Lib, EntryPoint = "alListeneriv", CallingConvention = CallConvention)]
        public static extern void Listeneriv(int param, [In] int[] values);

        [DllImport(Lib, EntryPoint = "alGetListenerf", CallingConvention = CallConvention)]
        public static extern void GetListenerf(int param, out float value);

        [DllImport(Lib, EntryPoint = "alGetListener3f", CallingConvention = CallConvention)]
        public static extern void GetListener3f(int param, out float value1, out float value2, out float value3);

        [DllImport(Lib, EntryPoint = "alGetListenerfv", CallingConvention = CallConvention)]
        public static extern void GetListenerfv(int param, [Out] float[] values);

        [DllImport(Lib, EntryPoint = "alGetListeneri", CallingConvention = CallConvention)]
        public static extern void GetListeneri(int param, out int value);

        [DllImport(Lib, EntryPoint = "alGetListener3i", CallingConvention = CallConvention)]
        public static extern void GetListener3i(int param, out int value1, out int value2, out int value3);

        [DllImport(Lib, EntryPoint = "alGetListeneriv", CallingConvention = CallConvention)]
        public static extern void GetListeneriv(int param, [Out] int[] values);

        // Sources
        [DllImport(Lib, EntryPoint = "alGenSources", CallingConvention = CallConvention)]
        public static extern void GenSources(int n, [Out] uint[] sources);

        [DllImport(Lib, EntryPoint = "alDeleteSources", CallingConvention = CallConvention)]
        public static extern void DeleteSources(int n, [In] uint[] sources);

        [DllImport(Lib, EntryPoint = "alIsSource", CallingConvention = CallConvention)]
        public static extern bool IsSource(uint source);

        [DllImport(Lib, EntryPoint = "alSourcef", CallingConvention = CallConvention)]
        public static extern void Sourcef(uint source, int param, float value);

        [DllImport(Lib, EntryPoint = "alSource3f", CallingConvention = CallConvention)]
        public static extern void Source3f(uint source, int param, float value1, float value2, float value3);

        [DllImport(Lib, EntryPoint = "alSourcefv", CallingConvention = CallConvention)]
        public static extern void Sourcefv(uint source, int param, [In] float[] values);

        [DllImport(Lib, EntryPoint = "alSourcei", CallingConvention = CallConvention)]
        public static extern void Sourcei(uint source, int param, int value);

        [DllImport(Lib, EntryPoint = "alSource3i", CallingConvention = CallConvention)]
        public static extern void Source3i(uint source, int param, int value1, int value2, int value3);

        [DllImport(Lib, EntryPoint = "alSourceiv", CallingConvention = CallConvention)]
        public static extern void Sourceiv(uint source, int param, [In] int[] values);

        [DllImport(Lib, EntryPoint = "alGetSourcef", CallingConvention = CallConvention)]
        public static extern void GetSourcef(uint source, int param, out float value);

        [DllImport(Lib, EntryPoint = "alGetSource3f", CallingConvention = CallConvention)]
        public static extern void GetSource3f(uint source, int param, out float value1, out float value2, out float value3);

        [DllImport(Lib, EntryPoint = "alGetSourcefv", CallingConvention = CallConvention)]
        public static extern void GetSourcefv(uint source, int param, [Out] float[] values);

        [DllImport(Lib, EntryPoint = "alGetSourcei", CallingConvention = CallConvention)]
        public static extern void GetSourcei(uint source, int param, out int value);

        [DllImport(Lib, EntryPoint = "alGetSource3i", CallingConvention = CallConvention)]
        public static extern void GetSource3i(uint source, int param, out int value1, out int value2, out int value3);

        [DllImport(Lib, EntryPoint = "alGetSourceiv", CallingConvention = CallConvention)]
        public static extern void GetSourceiv(uint source, int param, [Out] int[] values);

        [DllImport(Lib, EntryPoint = "alSourcePlay", CallingConvention = CallConvention)]
        public static extern void SourcePlay(uint source);

        [DllImport(Lib, EntryPoint = "alSourceStop", CallingConvention = CallConvention)]
        public static extern void SourceStop(uint source);

        [DllImport(Lib, EntryPoint = "alSourceRewind", CallingConvention = CallConvention)]
        public static extern void SourceRewind(uint source);

        [DllImport(Lib, EntryPoint = "alSourcePause", CallingConvention = CallConvention)]
        public static extern void SourcePause(uint source);

        [DllImport(Lib, EntryPoint = "alSourcePlayv", CallingConvention = CallConvention)]
        public static extern void SourcePlayv(int n, [In] uint[] sources);

        [DllImport(Lib, EntryPoint = "alSourceStopv", CallingConvention = CallConvention)]
        public static extern void SourceStopv(int n, [In] uint[] sources);

        [DllImport(Lib, EntryPoint = "alSourceRewindv", CallingConvention = CallConvention)]
        public static extern void SourceRewindv(int n, [In] uint[] sources);

        [DllImport(Lib, EntryPoint = "alSourcePausev", CallingConvention = CallConvention)]
        public static extern void SourcePausev(int n, [In] uint[] sources);

        [DllImport(Lib, EntryPoint = "alSourceQueueBuffers", CallingConvention = CallConvention)]
        public static extern void SourceQueueBuffers(uint source, int nb, [In] uint[] buffers);

        [DllImport(Lib, EntryPoint = "alSourceUnqueueBuffers", CallingConvention = CallConvention)]
        public static extern void SourceUnqueueBuffers(uint source, int nb, [Out] uint[] buffers);

        // Buffers
        [DllImport(Lib, EntryPoint = "alGenBuffers", CallingConvention = CallConvention)]
        public static extern void GenBuffers(int n, [Out] uint[] buffers);

        [DllImport(Lib, EntryPoint = "alDeleteBuffers", CallingConvention = CallConvention)]
        public static extern void DeleteBuffers(int n, [In] uint[] buffers);

        [DllImport(Lib, EntryPoint = "alIsBuffer", CallingConvention = CallConvention)]
        public static extern bool IsBuffer(uint buffer);

        [DllImport(Lib, EntryPoint = "alBufferData", CallingConvention = CallConvention)]
        public static extern void BufferData(uint buffer, int format, IntPtr data, int size, int samplerate);

        [DllImport(Lib, EntryPoint = "alBufferf", CallingConvention = CallConvention)]
        public static extern void Bufferf(uint buffer, int param, float value);

        [DllImport(Lib, EntryPoint = "alBuffer3f", CallingConvention = CallConvention)]
        public static extern void Buffer3f(uint buffer, int param, float value1, float value2, float value3);

        [DllImport(Lib, EntryPoint = "alBufferfv", CallingConvention = CallConvention)]
        public static extern void Bufferfv(uint buffer, int param, [In] float[] values);

        [DllImport(Lib, EntryPoint = "alBufferi", CallingConvention = CallConvention)]
        public static extern void Bufferi(uint buffer, int param, int value);

        [DllImport(Lib, EntryPoint = "alBuffer3i", CallingConvention = CallConvention)]
        public static extern void Buffer3i(uint buffer, int param, int value1, int value2, int value3);

        [DllImport(Lib, EntryPoint = "alBufferiv", CallingConvention = CallConvention)]
        public static extern void Bufferiv(uint buffer, int param, [In] int[] values);

        [DllImport(Lib, EntryPoint = "alGetBufferf", CallingConvention = CallConvention)]
        public static extern void GetBufferf(uint buffer, int param, out float value);

        [DllImport(Lib, EntryPoint = "alGetBuffer3f", CallingConvention = CallConvention)]
        public static extern void GetBuffer3f(uint buffer, int param, out float value1, out float value2, out float value3);

        [DllImport(Lib, EntryPoint = "alGetBufferfv", CallingConvention = CallConvention)]
        public static extern void GetBufferfv(uint buffer, int param, [Out] float[] values);

        [DllImport(Lib, EntryPoint = "alGetBufferi", CallingConvention = CallConvention)]
        public static extern void GetBufferi(uint buffer, int param, out int value);

        [DllImport(Lib, EntryPoint = "alGetBuffer3i", CallingConvention = CallConvention)]
        public static extern void GetBuffer3i(uint buffer, int param, out int value1, out int value2, out int value3);

        [DllImport(Lib, EntryPoint = "alGetBufferiv", CallingConvention = CallConvention)]
        public static extern void GetBufferiv(uint buffer, int param, [Out] int[] values);
    }
}
