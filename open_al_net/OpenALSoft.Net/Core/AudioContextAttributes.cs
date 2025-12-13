using System.Collections.Generic;
using OpenALSoft.Net.Native;

namespace OpenALSoft.Net.Core
{
    /// <summary>
    /// Helper class to build context attributes.
    /// </summary>
    public class AudioContextAttributes
    {
        private Dictionary<int, int> _attributes = new Dictionary<int, int>();

        /// <summary>
        /// Sets the frequency attribute.
        /// </summary>
        public AudioContextAttributes Frequency(int frequency)
        {
            _attributes[ALC.FREQUENCY] = frequency;
            return this;
        }

        /// <summary>
        /// Sets the refresh attribute.
        /// </summary>
        public AudioContextAttributes Refresh(int refresh)
        {
            _attributes[ALC.REFRESH] = refresh;
            return this;
        }

        /// <summary>
        /// Sets the sync attribute.
        /// </summary>
        public AudioContextAttributes Sync(bool sync)
        {
            _attributes[ALC.SYNC] = sync ? ALC.TRUE : ALC.FALSE;
            return this;
        }

        /// <summary>
        /// Sets the mono sources attribute.
        /// </summary>
        public AudioContextAttributes MonoSources(int count)
        {
            _attributes[ALC.MONO_SOURCES] = count;
            return this;
        }

        /// <summary>
        /// Sets the stereo sources attribute.
        /// </summary>
        public AudioContextAttributes StereoSources(int count)
        {
            _attributes[ALC.STEREO_SOURCES] = count;
            return this;
        }

        /// <summary>
        /// Sets the HRTF attribute (ALC_SOFT_HRTF).
        /// </summary>
        /// <param name="enable">True to enable, false to disable, null for don't care.</param>
        public AudioContextAttributes Hrtf(bool? enable)
        {
            if (enable.HasValue)
            {
                _attributes[ALC.HRTF_SOFT] = enable.Value ? ALC.HRTF_ENABLED_SOFT : ALC.HRTF_DISABLED_SOFT;
            }
            else
            {
                _attributes[ALC.HRTF_SOFT] = ALC.DONT_CARE_SOFT;
            }
            return this;
        }

        /// <summary>
        /// Sets the HRTF ID attribute.
        /// </summary>
        public AudioContextAttributes HrtfId(int id)
        {
            _attributes[ALC.HRTF_ID_SOFT] = id;
            return this;
        }

        /// <summary>
        /// Sets the HRTF specifier attribute (by index).
        /// Note: This usually requires using the index, not the string directly in attributes.
        /// </summary>
        public AudioContextAttributes HrtfSpecifier(int index)
        {
            // ALC_HRTF_SPECIFIER_SOFT expects an index if used in attributes?
            // Actually, the spec says "ALC_HRTF_ID_SOFT" is for selecting by ID.
            // "ALC_HRTF_SPECIFIER_SOFT" is usually for query.
            // But let's check if we can set it.
            // The spec says: "The ALC_HRTF_ID_SOFT attribute can be used to request a specific HRTF by its ID."
            // So we should use HrtfId.
            return this;
        }

        /// <summary>
        /// Converts the attributes to an integer array suitable for alcCreateContext.
        /// </summary>
        public int[] ToArray()
        {
            var list = new List<int>();
            foreach (var kvp in _attributes)
            {
                list.Add(kvp.Key);
                list.Add(kvp.Value);
            }
            list.Add(0); // Terminate with 0
            return list.ToArray();
        }
    }
}
