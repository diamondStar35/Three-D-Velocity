using System;
using OpenALSoft.Net.Native;

namespace OpenALSoft.Net.Core
{
    /// <summary>
    /// Represents an OpenAL audio source.
    /// </summary>
    public class AudioSource : IDisposable
    {
        private uint _source;
        private bool _isDisposed;

        /// <summary>
        /// Gets the native source ID.
        /// </summary>
        public uint Id => _source;

        /// <summary>
        /// Creates a new audio source.
        /// </summary>
        public AudioSource()
        {
            uint[] sources = new uint[1];
            AL.GenSources(1, sources);
            _source = sources[0];

            if (_source == 0)
            {
                throw new Exception("Failed to generate audio source.");
            }
        }

        /// <summary>
        /// Gets or sets the buffer attached to this source.
        /// </summary>
        public AudioBuffer Buffer
        {
            set
            {
                AL.Sourcei(_source, AL.BUFFER, (int)(value?.Id ?? 0));
                CheckError();
            }
        }

        /// <summary>
        /// Gets or sets the gain (volume) of the source.
        /// </summary>
        public float Gain
        {
            get
            {
                AL.GetSourcef(_source, AL.GAIN, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.GAIN, value);
            }
        }

        /// <summary>
        /// Gets or sets the pitch of the source.
        /// </summary>
        public float Pitch
        {
            get
            {
                AL.GetSourcef(_source, AL.PITCH, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.PITCH, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the source should loop.
        /// </summary>
        public bool Looping
        {
            get
            {
                AL.GetSourcei(_source, AL.LOOPING, out int value);
                return value == AL.TRUE;
            }
            set
            {
                AL.Sourcei(_source, AL.LOOPING, value ? AL.TRUE : AL.FALSE);
            }
        }

        /// <summary>
        /// Gets or sets the position of the source in 3D space.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                AL.GetSource3f(_source, AL.POSITION, out float x, out float y, out float z);
                return new Vector3(x, y, z);
            }
            set
            {
                AL.Source3f(_source, AL.POSITION, value.X, value.Y, value.Z);
            }
        }

        /// <summary>
        /// Gets or sets the velocity of the source in 3D space.
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                AL.GetSource3f(_source, AL.VELOCITY, out float x, out float y, out float z);
                return new Vector3(x, y, z);
            }
            set
            {
                AL.Source3f(_source, AL.VELOCITY, value.X, value.Y, value.Z);
            }
        }

        /// <summary>
        /// Gets or sets the direction of the source in 3D space.
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                AL.GetSource3f(_source, AL.DIRECTION, out float x, out float y, out float z);
                return new Vector3(x, y, z);
            }
            set
            {
                AL.Source3f(_source, AL.DIRECTION, value.X, value.Y, value.Z);
            }
        }

        /// <summary>
        /// Gets or sets the reference distance for attenuation.
        /// </summary>
        public float ReferenceDistance
        {
            get
            {
                AL.GetSourcef(_source, AL.REFERENCE_DISTANCE, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.REFERENCE_DISTANCE, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum distance for attenuation.
        /// </summary>
        public float MaxDistance
        {
            get
            {
                AL.GetSourcef(_source, AL.MAX_DISTANCE, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.MAX_DISTANCE, value);
            }
        }

        /// <summary>
        /// Gets or sets the rolloff factor for attenuation.
        /// </summary>
        public float RolloffFactor
        {
            get
            {
                AL.GetSourcef(_source, AL.ROLLOFF_FACTOR, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.ROLLOFF_FACTOR, value);
            }
        }

        /// <summary>
        /// Gets or sets the minimum gain for the source.
        /// </summary>
        public float MinGain
        {
            get
            {
                AL.GetSourcef(_source, AL.MIN_GAIN, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.MIN_GAIN, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximum gain for the source.
        /// </summary>
        public float MaxGain
        {
            get
            {
                AL.GetSourcef(_source, AL.MAX_GAIN, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.MAX_GAIN, value);
            }
        }

        /// <summary>
        /// Gets or sets the gain when the listener is outside the cone.
        /// </summary>
        public float ConeOuterGain
        {
            get
            {
                AL.GetSourcef(_source, AL.CONE_OUTER_GAIN, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.CONE_OUTER_GAIN, value);
            }
        }

        /// <summary>
        /// Gets or sets the inner angle of the sound cone, in degrees.
        /// </summary>
        public float ConeInnerAngle
        {
            get
            {
                AL.GetSourcef(_source, AL.CONE_INNER_ANGLE, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.CONE_INNER_ANGLE, value);
            }
        }

        /// <summary>
        /// Gets or sets the outer angle of the sound cone, in degrees.
        /// </summary>
        public float ConeOuterAngle
        {
            get
            {
                AL.GetSourcef(_source, AL.CONE_OUTER_ANGLE, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.CONE_OUTER_ANGLE, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the source is relative to the listener.
        /// </summary>
        public bool Relative
        {
            get
            {
                AL.GetSourcei(_source, AL.SOURCE_RELATIVE, out int value);
                return value == AL.TRUE;
            }
            set
            {
                AL.Sourcei(_source, AL.SOURCE_RELATIVE, value ? AL.TRUE : AL.FALSE);
            }
        }

        /// <summary>
        /// Gets the source type (Static, Streaming, Undetermined).
        /// </summary>
        public AudioSourceType Type
        {
            get
            {
                AL.GetSourcei(_source, AL.SOURCE_TYPE, out int value);
                return (AudioSourceType)value;
            }
        }

        /// <summary>
        /// Gets or sets the playback position in seconds.
        /// </summary>
        public float SecOffset
        {
            get
            {
                AL.GetSourcef(_source, AL.SEC_OFFSET, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.SEC_OFFSET, value);
            }
        }

        /// <summary>
        /// Gets or sets the playback position in samples.
        /// </summary>
        public int SampleOffset
        {
            get
            {
                AL.GetSourcei(_source, AL.SAMPLE_OFFSET, out int value);
                return value;
            }
            set
            {
                AL.Sourcei(_source, AL.SAMPLE_OFFSET, value);
            }
        }

        /// <summary>
        /// Gets or sets the playback position in bytes.
        /// </summary>
        public int ByteOffset
        {
            get
            {
                AL.GetSourcei(_source, AL.BYTE_OFFSET, out int value);
                return value;
            }
            set
            {
                AL.Sourcei(_source, AL.BYTE_OFFSET, value);
            }
        }

        /// <summary>
        /// Gets or sets the spatialization mode (Auto, On, Off).
        /// </summary>
        public SpatializationMode Spatialize
        {
            get
            {
                AL.GetSourcei(_source, AL.SOURCE_SPATIALIZE_SOFT, out int value);
                return (SpatializationMode)value;
            }
            set
            {
                AL.Sourcei(_source, AL.SOURCE_SPATIALIZE_SOFT, (int)value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether direct channels are enabled.
        /// </summary>
        public bool DirectChannels
        {
            get
            {
                AL.GetSourcei(_source, AL.DIRECT_CHANNELS_SOFT, out int value);
                return value == AL.TRUE;
            }
            set
            {
                AL.Sourcei(_source, AL.DIRECT_CHANNELS_SOFT, value ? AL.TRUE : AL.FALSE);
            }
        }

        /// <summary>
        /// Gets or sets the stereo angles (in radians).
        /// </summary>
        public Vector2 StereoAngles
        {
            get
            {
                float[] values = new float[2];
                AL.GetSourcefv(_source, AL.STEREO_ANGLES, values);
                return new Vector2(values[0], values[1]);
            }
            set
            {
                AL.Sourcefv(_source, AL.STEREO_ANGLES, new float[] { value.X, value.Y });
            }
        }

        /// <summary>
        /// Gets or sets the source radius.
        /// </summary>
        public float Radius
        {
            get
            {
                AL.GetSourcef(_source, AL.SOURCE_RADIUS, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.SOURCE_RADIUS, value);
            }
        }

        /// <summary>
        /// Gets the source latency in nanoseconds.
        /// </summary>
        public long Latency
        {
            get
            {
                if (AlExtensions.GetSourcei64vSoft != null)
                {
                    long[] values = new long[1];
                    AlExtensions.GetSourcei64vSoft(_source, AL.SAMPLE_OFFSET_LATENCY_SOFT, values);
                    // The first value is the sample offset (64-bit), the second is the latency?
                    // Spec says: AL_SAMPLE_OFFSET_LATENCY_SOFT: value1=offset (128.64 fixed point?), value2=latency (nanoseconds)
                    // Wait, alGetSourcei64vSOFT retrieves 64-bit integer values.
                    // If we use AL_SAMPLE_OFFSET_LATENCY_SOFT, it returns 2 values.
                    // Let's check the spec or header.
                    // "AL_SAMPLE_OFFSET_LATENCY_SOFT ... returns two values: the current sample offset ... and the latency ..."
                    
                    long[] val2 = new long[2];
                    AlExtensions.GetSourcei64vSoft(_source, AL.SAMPLE_OFFSET_LATENCY_SOFT, val2);
                    return val2[1];
                }
                return 0;
            }
        }

        /// <summary>
        /// Gets or sets the gain limit (clamp).
        /// </summary>
        public float GainLimit
        {
            get
            {
                AL.GetSourcef(_source, AL.GAIN_LIMIT_SOFT, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.GAIN_LIMIT_SOFT, value);
            }
        }

        /// <summary>
        /// Gets or sets the resampler index.
        /// </summary>
        public ResamplerType Resampler
        {
            get
            {
                AL.GetSourcei(_source, AL.SOURCE_RESAMPLER_SOFT, out int value);
                return (ResamplerType)value;
            }
            set
            {
                AL.Sourcei(_source, AL.SOURCE_RESAMPLER_SOFT, (int)value);
            }
        }

        /// <summary>
        /// Plays the source at a specific device time.
        /// </summary>
        public void PlayAtTime(long deviceTime)
        {
            if (AlExtensions.SourcePlayAtTimeSoft != null)
            {
                AlExtensions.SourcePlayAtTimeSoft(_source, deviceTime);
                CheckError();
            }
            else
            {
                throw new NotSupportedException("AL_SOFT_source_start_delay extension not supported.");
            }
        }

        /// <summary>
        /// Sets the auxiliary send filter.
        /// </summary>
        /// <param name="auxSlot">The auxiliary effect slot ID.</param>
        /// <param name="filter">The filter ID.</param>
        public void SetAuxiliarySendFilter(int auxSlot, int filter)
        {
            AL.Source3i(_source, AL.AUXILIARY_SEND_FILTER, auxSlot, filter, 0);
            CheckError();
        }

        /// <summary>
        /// Gets or sets the direct filter.
        /// </summary>
        public int DirectFilter
        {
            get
            {
                AL.GetSourcei(_source, AL.DIRECT_FILTER, out int value);
                return value;
            }
            set
            {
                AL.Sourcei(_source, AL.DIRECT_FILTER, value);
            }
        }

        /// <summary>
        /// Gets or sets the air absorption factor.
        /// </summary>
        public float AirAbsorptionFactor
        {
            get
            {
                AL.GetSourcef(_source, AL.AIR_ABSORPTION_FACTOR, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.AIR_ABSORPTION_FACTOR, value);
            }
        }

        /// <summary>
        /// Gets or sets the room rolloff factor.
        /// </summary>
        public float RoomRolloffFactor
        {
            get
            {
                AL.GetSourcef(_source, AL.ROOM_ROLLOFF_FACTOR, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.ROOM_ROLLOFF_FACTOR, value);
            }
        }

        /// <summary>
        /// Gets or sets the cone outer gain HF.
        /// </summary>
        public float ConeOuterGainHF
        {
            get
            {
                AL.GetSourcef(_source, AL.CONE_OUTER_GAINHF, out float value);
                return value;
            }
            set
            {
                AL.Sourcef(_source, AL.CONE_OUTER_GAINHF, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether direct filter gain HF is auto.
        /// </summary>
        public bool DirectFilterGainHFAuto
        {
            get
            {
                AL.GetSourcei(_source, AL.DIRECT_FILTER_GAINHF_AUTO, out int value);
                return value == AL.TRUE;
            }
            set
            {
                AL.Sourcei(_source, AL.DIRECT_FILTER_GAINHF_AUTO, value ? AL.TRUE : AL.FALSE);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auxiliary send filter gain is auto.
        /// </summary>
        public bool AuxiliarySendFilterGainAuto
        {
            get
            {
                AL.GetSourcei(_source, AL.AUXILIARY_SEND_FILTER_GAIN_AUTO, out int value);
                return value == AL.TRUE;
            }
            set
            {
                AL.Sourcei(_source, AL.AUXILIARY_SEND_FILTER_GAIN_AUTO, value ? AL.TRUE : AL.FALSE);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auxiliary send filter gain HF is auto.
        /// </summary>
        public bool AuxiliarySendFilterGainHFAuto
        {
            get
            {
                AL.GetSourcei(_source, AL.AUXILIARY_SEND_FILTER_GAINHF_AUTO, out int value);
                return value == AL.TRUE;
            }
            set
            {
                AL.Sourcei(_source, AL.AUXILIARY_SEND_FILTER_GAINHF_AUTO, value ? AL.TRUE : AL.FALSE);
            }
        }

        /// <summary>
        /// Gets the current state of the source.
        /// </summary>
        public AudioSourceState State
        {
            get
            {
                AL.GetSourcei(_source, AL.SOURCE_STATE, out int value);
                return (AudioSourceState)value;
            }
        }

        /// <summary>
        /// Plays the source.
        /// </summary>
        public void Play()
        {
            AL.SourcePlay(_source);
            CheckError();
        }

        /// <summary>
        /// Pauses the source.
        /// </summary>
        public void Pause()
        {
            AL.SourcePause(_source);
            CheckError();
        }

        /// <summary>
        /// Stops the source.
        /// </summary>
        public void Stop()
        {
            AL.SourceStop(_source);
            CheckError();
        }

        /// <summary>
        /// Rewinds the source.
        /// </summary>
        public void Rewind()
        {
            AL.SourceRewind(_source);
            CheckError();
        }

        private void CheckError()
        {
            int error = AL.GetError();
            if (error != AL.NO_ERROR)
            {
                throw new Exception($"OpenAL Error: {error:X}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (_source != 0)
                {
                    if (AL.IsSource(_source))
                    {
                        AL.DeleteSources(1, new uint[] { _source });
                    }
                    _source = 0;
                }
                _isDisposed = true;
            }
        }

        ~AudioSource()
        {
            Dispose(false);
        }
    }
}
