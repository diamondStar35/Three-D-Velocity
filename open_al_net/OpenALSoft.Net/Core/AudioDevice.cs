using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenALSoft.Net.Native;

namespace OpenALSoft.Net.Core
{
    /// <summary>
    /// Represents an OpenAL audio device.
    /// </summary>
    public class AudioDevice : IDisposable
    {
        private IntPtr _device;
        private bool _isDisposed;

        // Extension delegates
        private ALC.AlcGetStringiSoftDelegate _getStringiSoft;
        private ALC.AlcResetDeviceSoftDelegate _resetDeviceSoft;
        private ALC.AlcDevicePauseSoftDelegate _devicePauseSoft;
        private ALC.AlcDeviceResumeSoftDelegate _deviceResumeSoft;
        private ALC.AlcReopenDeviceSoftDelegate _reopenDeviceSoft;
        private ALC.AlcGetInteger64vSoftDelegate _getInteger64vSoft;

        /// <summary>
        /// Gets the native device handle.
        /// </summary>
        public IntPtr Handle => _device;

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the ALC_SOFT_HRTF extension is supported.
        /// </summary>
        public bool IsHrtfSupported { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the ALC_SOFT_pause_device extension is supported.
        /// </summary>
        public bool IsPauseDeviceSupported { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the ALC_SOFT_reopen_device extension is supported.
        /// </summary>
        public bool IsReopenDeviceSupported { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the ALC_SOFT_device_clock extension is supported.
        /// </summary>
        public bool IsDeviceClockSupported { get; private set; }

        /// <summary>
        /// Opens the default audio device.
        /// </summary>
        public AudioDevice() : this(null)
        {
        }

        /// <summary>
        /// Opens the specified audio device.
        /// </summary>
        /// <param name="deviceName">The name of the device to open, or null for the default device.</param>
        public AudioDevice(string deviceName)
        {
            _device = ALC.OpenDevice(deviceName);
            if (_device == IntPtr.Zero)
            {
                throw new Exception("Failed to open audio device.");
            }

            Name = deviceName ?? GetDefaultDeviceName();
            CheckExtensions();
        }

        private void CheckExtensions()
        {
            IsHrtfSupported = ALC.IsExtensionPresent(_device, "ALC_SOFT_HRTF");
            if (IsHrtfSupported)
            {
                LoadDelegate(ref _getStringiSoft, "alcGetStringiSOFT");
                LoadDelegate(ref _resetDeviceSoft, "alcResetDeviceSOFT");
            }

            IsPauseDeviceSupported = ALC.IsExtensionPresent(_device, "ALC_SOFT_pause_device");
            if (IsPauseDeviceSupported)
            {
                LoadDelegate(ref _devicePauseSoft, "alcDevicePauseSOFT");
                LoadDelegate(ref _deviceResumeSoft, "alcDeviceResumeSOFT");
            }

            IsReopenDeviceSupported = ALC.IsExtensionPresent(_device, "ALC_SOFT_reopen_device");
            if (IsReopenDeviceSupported)
            {
                LoadDelegate(ref _reopenDeviceSoft, "alcReopenDeviceSOFT");
            }

            IsDeviceClockSupported = ALC.IsExtensionPresent(_device, "ALC_SOFT_device_clock");
            if (IsDeviceClockSupported)
            {
                LoadDelegate(ref _getInteger64vSoft, "alcGetInteger64vSOFT");
            }
        }

        private void LoadDelegate<T>(ref T del, string name) where T : Delegate
        {
            IntPtr proc = ALC.GetProcAddress(_device, name);
            if (proc != IntPtr.Zero)
            {
                del = Marshal.GetDelegateForFunctionPointer<T>(proc);
            }
        }

        /// <summary>
        /// Gets the list of available HRTF specifiers.
        /// </summary>
        public string[] GetHrtfSpecifiers()
        {
            if (!IsHrtfSupported || _getStringiSoft == null)
            {
                return new string[0];
            }

            int numSpecifiers = 0;
            ALC.GetIntegerv(_device, ALC.NUM_HRTF_SPECIFIERS_SOFT, 1, new int[] { numSpecifiers });

            int[] val = new int[1];
            ALC.GetIntegerv(_device, ALC.NUM_HRTF_SPECIFIERS_SOFT, 1, val);
            numSpecifiers = val[0];

            var specifiers = new List<string>();
            for (int i = 0; i < numSpecifiers; i++)
            {
                IntPtr ptr = _getStringiSoft(_device, ALC.HRTF_SPECIFIER_SOFT, i);
                if (ptr != IntPtr.Zero)
                {
                    specifiers.Add(Marshal.PtrToStringAnsi(ptr));
                }
            }

            return specifiers.ToArray();
        }

        /// <summary>
        /// Resets the device with the specified attributes.
        /// </summary>
        public bool Reset(int[] attributes)
        {
            if (_resetDeviceSoft != null)
            {
                return _resetDeviceSoft(_device, attributes);
            }
            return false;
        }

        /// <summary>
        /// Pauses the device playback.
        /// </summary>
        public void Pause()
        {
            if (_devicePauseSoft != null)
            {
                _devicePauseSoft(_device);
            }
        }

        /// <summary>
        /// Resumes the device playback.
        /// </summary>
        public void Resume()
        {
            if (_deviceResumeSoft != null)
            {
                _deviceResumeSoft(_device);
            }
        }

        /// <summary>
        /// Reopens the device with new attributes.
        /// </summary>
        public bool Reopen(string deviceName, int[] attributes)
        {
            if (_reopenDeviceSoft != null)
            {
                return _reopenDeviceSoft(_device, deviceName, attributes);
            }
            return false;
        }

        /// <summary>
        /// Gets the device clock time in nanoseconds.
        /// </summary>
        public long GetClock()
        {
            if (_getInteger64vSoft != null)
            {
                long[] val = new long[1];
                _getInteger64vSoft(_device, ALC.DEVICE_CLOCK_SOFT, 1, val);
                return val[0];
            }
            return 0;
        }

        /// <summary>
        /// Gets the device latency in nanoseconds.
        /// </summary>
        public long GetLatency()
        {
            if (_getInteger64vSoft != null)
            {
                long[] val = new long[1];
                _getInteger64vSoft(_device, ALC.DEVICE_LATENCY_SOFT, 1, val);
                return val[0];
            }
            return 0;
        }

        public static string GetDefaultDeviceName()
        {
            IntPtr ptr = ALC.GetString(IntPtr.Zero, ALC.DEFAULT_DEVICE_SPECIFIER);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public static string[] EnumerateDevices()
        {
            if (ALC.IsExtensionPresent(IntPtr.Zero, "ALC_ENUMERATE_ALL_EXT"))
            {
                IntPtr ptr = ALC.GetString(IntPtr.Zero, ALC.ALL_DEVICES_SPECIFIER);
                return ParseStringList(ptr);
            }
            else
            {
                IntPtr ptr = ALC.GetString(IntPtr.Zero, ALC.DEVICE_SPECIFIER);
                return ParseStringList(ptr);
            }
        }

        private static string[] ParseStringList(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return new string[0];

            var strings = new List<string>();
            int offset = 0;
            while (true)
            {
                string s = Marshal.PtrToStringAnsi(new IntPtr(ptr.ToInt64() + offset));
                if (string.IsNullOrEmpty(s)) break;
                strings.Add(s);
                offset += s.Length + 1;
            }
            return strings.ToArray();
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
                if (_device != IntPtr.Zero)
                {
                    ALC.CloseDevice(_device);
                    _device = IntPtr.Zero;
                }
                _isDisposed = true;
            }
        }

        ~AudioDevice()
        {
            Dispose(false);
        }
    }
}
