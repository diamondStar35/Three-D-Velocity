using System;

namespace OpenALSoft.Net.Core
{
    /// <summary>
    /// Represents the state of an audio source.
    /// </summary>
    public enum AudioSourceState
    {
        Initial = 0x1011,
        Playing = 0x1012,
        Paused = 0x1013,
        Stopped = 0x1014
    }

    /// <summary>
    /// Represents the type of an audio source.
    /// </summary>
    public enum AudioSourceType
    {
        Static = 0x1028,
        Streaming = 0x1029,
        Undetermined = 0x1030
    }

    /// <summary>
    /// Represents the distance model used for attenuation.
    /// </summary>
    public enum AudioDistanceModel
    {
        None = 0,
        InverseDistance = 0xD001,
        InverseDistanceClamped = 0xD002,
        LinearDistance = 0xD003,
        LinearDistanceClamped = 0xD004,
        ExponentDistance = 0xD005,
        ExponentDistanceClamped = 0xD006
    }

    /// <summary>
    /// Represents the audio format.
    /// </summary>
    public enum AudioFormat
    {
        Mono8 = 0x1100,
        Mono16 = 0x1101,
        Stereo8 = 0x1102,
        Stereo16 = 0x1103
    }

    /// <summary>
    /// Represents the spatialization mode.
    /// </summary>
    public enum SpatializationMode
    {
        Off = 0,
        On = 1,
        Auto = 2
    }

    /// <summary>
    /// Represents the resampler type.
    /// </summary>
    public enum ResamplerType
    {
        Default = 0,
        Point = 1,
        Linear = 2,
        Cubic = 3,
        BSinc12 = 4,
        BSinc24 = 5
    }
}
