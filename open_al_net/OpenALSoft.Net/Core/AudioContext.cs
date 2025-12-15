using System;
using System.Collections.Generic;
using OpenALSoft.Net.Native;

namespace OpenALSoft.Net.Core
{
    /// <summary>
    /// Represents an OpenAL context.
    /// </summary>
    public class AudioContext : IDisposable
    {
        private IntPtr _context;
        private bool _isDisposed;
        private AudioDevice _device;

        /// <summary>
        /// Gets the native context handle.
        /// </summary>
        public IntPtr Handle => _context;

        /// <summary>
        /// Gets the device associated with this context.
        /// </summary>
        public AudioDevice Device => _device;

        static AudioContext()
        {
            AlExtensions.Initialize();
        }

        /// <summary>
        /// Creates a new audio context on the specified device.
        /// </summary>
        /// <param name="device">The device to create the context on.</param>
        /// <param name="attributes">Optional context attributes.</param>
        public AudioContext(AudioDevice device, int[] attributes = null)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            
            // Ensure the attribute list is terminated with 0 if not null
            if (attributes != null && attributes.Length > 0 && attributes[attributes.Length - 1] != 0)
            {
                var newAttribs = new int[attributes.Length + 1];
                Array.Copy(attributes, newAttribs, attributes.Length);
                newAttribs[attributes.Length] = 0;
                attributes = newAttribs;
            }

            _context = ALC.CreateContext(device.Handle, attributes);
            if (_context == IntPtr.Zero)
            {
                throw new Exception("Failed to create audio context.");
            }
        }

        /// <summary>
        /// Makes this context the current context for the current thread.
        /// </summary>
        public bool MakeCurrent()
        {
            return ALC.MakeContextCurrent(_context);
        }

        /// <summary>
        /// Makes no context current.
        /// </summary>
        public static void MakeNoneCurrent()
        {
            ALC.MakeContextCurrent(IntPtr.Zero);
        }

        /// <summary>
        /// Processes the context (updates).
        /// </summary>
        public void Process()
        {
            ALC.ProcessContext(_context);
        }

        /// <summary>
        /// Suspends the context (updates).
        /// </summary>
        public void Suspend()
        {
            ALC.SuspendContext(_context);
        }

        /// <summary>
        /// Defers updates for the current context.
        /// </summary>
        public static void DeferUpdates()
        {
            if (AlExtensions.DeferUpdatesSoft != null)
            {
                AlExtensions.DeferUpdatesSoft();
            }
        }

        /// <summary>
        /// Processes deferred updates for the current context.
        /// </summary>
        public static void ProcessUpdates()
        {
            if (AlExtensions.ProcessUpdatesSoft != null)
            {
                AlExtensions.ProcessUpdatesSoft();
            }
        }

        /// <summary>
        /// Gets the current context.
        /// </summary>
        public static IntPtr GetCurrentContext()
        {
            return ALC.GetCurrentContext();
        }

        /// <summary>
        /// Gets or sets the distance model.
        /// </summary>
        public static AudioDistanceModel DistanceModel
        {
            get
            {
                return (AudioDistanceModel)AL.GetInteger(AL.DISTANCE_MODEL);
            }
            set
            {
                AL.DistanceModel((int)value);
            }
        }

        /// <summary>
        /// Gets or sets the doppler factor.
        /// </summary>
        public static float DopplerFactor
        {
            get
            {
                return AL.GetFloat(AL.DOPPLER_FACTOR);
            }
            set
            {
                AL.DopplerFactor(value);
            }
        }

        /// <summary>
        /// Gets or sets the speed of sound.
        /// </summary>
        public static float SpeedOfSound
        {
            get
            {
                return AL.GetFloat(AL.SPEED_OF_SOUND);
            }
            set
            {
                AL.SpeedOfSound(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether HRTF is enabled for this context's device.
        /// This should be checked after context creation.
        /// </summary>
        public bool IsHrtfEnabled
        {
            get
            {
                if (_device == null || !_device.IsHrtfSupported)
                    return false;

                int[] status = new int[1];
                ALC.GetIntegerv(_device.Handle, ALC.HRTF_STATUS_SOFT, 1, status);
                return status[0] == ALC.HRTF_ENABLED_SOFT;
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
                if (_context != IntPtr.Zero)
                {
                    // If this context is current, make it not current before destroying
                    if (ALC.GetCurrentContext() == _context)
                    {
                        ALC.MakeContextCurrent(IntPtr.Zero);
                    }

                    ALC.DestroyContext(_context);
                    _context = IntPtr.Zero;
                }
                _isDisposed = true;
            }
        }

        ~AudioContext()
        {
            Dispose(false);
        }
    }
}
