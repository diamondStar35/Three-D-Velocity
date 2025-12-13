using System;
using System.Runtime.InteropServices;
using OpenALSoft.Net.Native;

namespace OpenALSoft.Net.Core
{
    /// <summary>
    /// Represents an OpenAL audio buffer.
    /// </summary>
    public class AudioBuffer : IDisposable
    {
        private uint _buffer;
        private bool _isDisposed;

        /// <summary>
        /// Gets the native buffer ID.
        /// </summary>
        public uint Id => _buffer;

        /// <summary>
        /// Creates a new audio buffer.
        /// </summary>
        public AudioBuffer()
        {
            uint[] buffers = new uint[1];
            AL.GenBuffers(1, buffers);
            _buffer = buffers[0];

            if (_buffer == 0)
            {
                throw new Exception("Failed to generate audio buffer.");
            }
        }

        /// <summary>
        /// Fills the buffer with audio data.
        /// </summary>
        /// <param name="format">The format of the audio data (e.g. AL.FORMAT_MONO16).</param>
        /// <param name="data">Pointer to the audio data.</param>
        /// <param name="size">Size of the data in bytes.</param>
        /// <param name="sampleRate">Sample rate of the audio data.</param>
        public void BufferData(int format, IntPtr data, int size, int sampleRate)
        {
            AL.BufferData(_buffer, format, data, size, sampleRate);
            CheckError();
        }

        /// <summary>
        /// Fills the buffer with audio data.
        /// </summary>
        public void BufferData(int format, byte[] data, int sampleRate)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                BufferData(format, handle.AddrOfPinnedObject(), data.Length, sampleRate);
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Fills the buffer with audio data.
        /// </summary>
        public void BufferData(int format, short[] data, int sampleRate)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                BufferData(format, handle.AddrOfPinnedObject(), data.Length * 2, sampleRate);
            }
            finally
            {
                handle.Free();
            }
        }

        /// <summary>
        /// Fills the buffer with audio data.
        /// </summary>
        public void BufferData(int format, float[] data, int sampleRate)
        {
             GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                BufferData(format, handle.AddrOfPinnedObject(), data.Length * 4, sampleRate);
            }
            finally
            {
                handle.Free();
            }
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
                if (_buffer != 0)
                {
                    if (AL.IsBuffer(_buffer))
                    {
                        AL.DeleteBuffers(1, new uint[] { _buffer });
                    }
                    _buffer = 0;
                }
                _isDisposed = true;
            }
        }

        ~AudioBuffer()
        {
            Dispose(false);
        }
    }
}
