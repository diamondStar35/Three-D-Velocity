using System;
using System.Runtime.InteropServices;
using OpenALSoft.Net.Native;

namespace OpenALSoft.Net.Core
{
    internal static class AlExtensions
    {
        private static bool _initialized;

        public static AL.AlDeferUpdatesSoftDelegate DeferUpdatesSoft;
        public static AL.AlProcessUpdatesSoftDelegate ProcessUpdatesSoft;
        public static AL.AlGetSourcei64vSoftDelegate GetSourcei64vSoft;
        public static AL.AlGetStringiSoftDelegate GetStringiSoft;
        public static AL.AlSourcePlayAtTimeSoftDelegate SourcePlayAtTimeSoft;
        public static AL.AlSourcePlayAtTimevSoftDelegate SourcePlayAtTimevSoft;

        public static void Initialize()
        {
            if (_initialized) return;

            LoadDelegate(ref DeferUpdatesSoft, "alDeferUpdatesSOFT");
            LoadDelegate(ref ProcessUpdatesSoft, "alProcessUpdatesSOFT");
            LoadDelegate(ref GetSourcei64vSoft, "alGetSourcei64vSOFT");
            LoadDelegate(ref GetStringiSoft, "alGetStringiSOFT");
            LoadDelegate(ref SourcePlayAtTimeSoft, "alSourcePlayAtTimeSOFT");
            LoadDelegate(ref SourcePlayAtTimevSoft, "alSourcePlayAtTimevSOFT");

            _initialized = true;
        }

        private static void LoadDelegate<T>(ref T del, string name) where T : Delegate
        {
            if (AL.IsExtensionPresent("AL_SOFT_deferred_updates") || 
                AL.IsExtensionPresent("AL_SOFT_source_latency") ||
                AL.IsExtensionPresent("AL_SOFT_source_resampler") ||
                AL.IsExtensionPresent("AL_SOFT_source_start_delay"))
            {
                // We just try to load everything. If an extension is not present, GetProcAddress returns null.
                IntPtr proc = AL.GetProcAddress(name);
                if (proc != IntPtr.Zero)
                {
                    del = Marshal.GetDelegateForFunctionPointer<T>(proc);
                }
            }
        }
    }
}
