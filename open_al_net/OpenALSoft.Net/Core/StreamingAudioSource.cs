using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenALSoft.Net.Native;

namespace OpenALSoft.Net.Core
{
    /// <summary>
    /// Represents a streaming OpenAL source that refills its queue via a managed callback.
    /// Designed for feeding Steam Audio's processed (float) PCM output directly into OpenAL.
    /// </summary>
    public sealed class StreamingAudioSource : IDisposable
    {
        public delegate StreamChunk StreamFillCallback(int requestedFrames);

        private static readonly bool Float32Supported = AL.IsExtensionPresent("AL_EXT_float32");

        private readonly AudioSource _source;
        private readonly AudioBuffer[] _buffers;
        private readonly Dictionary<uint, AudioBuffer> _bufferLookup;
        private readonly StreamFillCallback _fillCallback;
        private readonly AudioFormat _format;
        private readonly int _sampleRate;
        private readonly int _channelCount;
        private readonly int _frameSize;

        private float[] _scratch;
        private bool _endOfStream;
        private bool _isDisposed;

        public StreamingAudioSource(AudioFormat format, int sampleRate, int frameSize, StreamFillCallback fillCallback, int bufferCount = 4)
        {
            if (fillCallback == null)
                throw new ArgumentNullException(nameof(fillCallback));
            if (frameSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(frameSize), "Frame size must be positive.");
            if (bufferCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferCount), "Buffer count must be positive.");

            if (!IsFloatFormat(format))
                throw new NotSupportedException("StreamingAudioSource currently supports 32-bit float formats to match Steam Audio output.");
            if (!Float32Supported)
                throw new NotSupportedException("The AL_EXT_float32 extension is required for streaming float audio.");

            _format = format;
            _sampleRate = sampleRate;
            _frameSize = frameSize;
            _fillCallback = fillCallback;
            _channelCount = GetChannelCount(format);
            _scratch = new float[_frameSize * _channelCount];

            _source = new AudioSource();
            _buffers = new AudioBuffer[bufferCount];
            _bufferLookup = new Dictionary<uint, AudioBuffer>(bufferCount);
            for (int i = 0; i < bufferCount; i++)
            {
                var buffer = new AudioBuffer();
                _buffers[i] = buffer;
                _bufferLookup[buffer.Id] = buffer;
            }

            PrimeBuffers();
        }

        /// <summary>
        /// The underlying AudioSource so callers can set spatial parameters (position, gain, etc).
        /// </summary>
        public AudioSource Source => _source;

        /// <summary>
        /// True once the callback signaled end of stream and all queued buffers have been drained.
        /// </summary>
        public bool HasEnded => _endOfStream && QueuedBufferCount == 0;

        public void Play()
        {
            EnsureNotDisposed();
            if (_source.State != AudioSourceState.Playing && QueuedBufferCount > 0)
            {
                _source.Play();
            }
        }

        public void Stop()
        {
            if (_isDisposed)
                return;

            _endOfStream = true;
            _source.Stop();
        }

        /// <summary>
        /// Pumps processed buffers back through the callback to keep the queue full.
        /// Should be called regularly from the game loop.
        /// </summary>
        /// <returns>False when the stream has ended and no buffers remain queued.</returns>
        public bool Update()
        {
            EnsureNotDisposed();

            AL.GetSourcei(_source.Id, AL.BUFFERS_PROCESSED, out int processed);
            while (processed-- > 0)
            {
                var buffer = UnqueueBuffer();
                if (buffer != null && TryRefillBuffer(buffer))
                {
                    QueueBuffer(buffer);
                }
            }

            if (QueuedBufferCount == 0 && !_endOfStream)
            {
                PrimeBuffers();
            }

            if (_source.State != AudioSourceState.Playing && QueuedBufferCount > 0 && !_endOfStream)
            {
                _source.Play();
            }

            if (HasEnded && _source.State == AudioSourceState.Playing)
            {
                _source.Stop();
            }

            return !HasEnded;
        }

        private void PrimeBuffers()
        {
            if (_endOfStream)
                return;

            foreach (var buffer in _buffers)
            {
                if (!TryRefillBuffer(buffer))
                    break;

                QueueBuffer(buffer);
            }
        }

        private AudioBuffer UnqueueBuffer()
        {
            uint[] bufferIds = new uint[1];
            AL.SourceUnqueueBuffers(_source.Id, 1, bufferIds);
            CheckError();

            var id = bufferIds[0];
            if (id == 0)
                return null;

            return _bufferLookup.TryGetValue(id, out var buffer) ? buffer : null;
        }

        private bool TryRefillBuffer(AudioBuffer buffer)
        {
            if (_endOfStream)
                return false;

            var chunk = _fillCallback(_frameSize);
            var channels = chunk.Channels ?? Array.Empty<float[]>();

            if (chunk.EndOfStream)
            {
                _endOfStream = true;
            }

            if (channels.Length != _channelCount || chunk.Frames <= 0)
            {
                return false;
            }

            int frames = Math.Min(chunk.Frames, GetMinAvailableFrames(channels));
            if (frames <= 0)
                return false;

            int samples = frames * _channelCount;
            EnsureScratch(samples);
            Interleave(channels, frames, _scratch);

            var handle = GCHandle.Alloc(_scratch, GCHandleType.Pinned);
            try
            {
                buffer.BufferData((int)_format, handle.AddrOfPinnedObject(), samples * sizeof(float), _sampleRate);
                CheckError();
            }
            finally
            {
                handle.Free();
            }

            return true;
        }

        private static int GetMinAvailableFrames(float[][] channels)
        {
            int frames = int.MaxValue;
            foreach (var channel in channels)
            {
                if (channel == null)
                    return 0;
                frames = Math.Min(frames, channel.Length);
            }

            return frames == int.MaxValue ? 0 : frames;
        }

        private static void Interleave(float[][] channels, int frames, float[] target)
        {
            int channelCount = channels.Length;
            for (int frame = 0; frame < frames; frame++)
            {
                int baseIndex = frame * channelCount;
                for (int channel = 0; channel < channelCount; channel++)
                {
                    target[baseIndex + channel] = channels[channel][frame];
                }
            }
        }

        private void QueueBuffer(AudioBuffer buffer)
        {
            AL.SourceQueueBuffers(_source.Id, 1, new[] { buffer.Id });
            CheckError();
        }

        private int QueuedBufferCount
        {
            get
            {
                AL.GetSourcei(_source.Id, AL.BUFFERS_QUEUED, out int queued);
                return queued;
            }
        }

        private void EnsureScratch(int requiredSamples)
        {
            if (_scratch.Length < requiredSamples)
            {
                Array.Resize(ref _scratch, requiredSamples);
            }
        }

        private static int GetChannelCount(AudioFormat format)
        {
            switch (format)
            {
                case AudioFormat.MonoFloat32:
                case AudioFormat.Mono16:
                case AudioFormat.Mono8:
                    return 1;
                case AudioFormat.StereoFloat32:
                case AudioFormat.Stereo16:
                case AudioFormat.Stereo8:
                    return 2;
                default:
                    throw new NotSupportedException($"Format {format} is not supported for streaming.");
            }
        }

        private static bool IsFloatFormat(AudioFormat format)
        {
            return format == AudioFormat.MonoFloat32 || format == AudioFormat.StereoFloat32;
        }

        private static void CheckError()
        {
            int error = AL.GetError();
            if (error != AL.NO_ERROR)
            {
                throw new Exception($"OpenAL Error: {error:X}");
            }
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(StreamingAudioSource));
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            try
            {
                _source.Stop();

                int queued = QueuedBufferCount;
                if (queued > 0)
                {
                    var bufferIds = new uint[queued];
                    AL.SourceUnqueueBuffers(_source.Id, queued, bufferIds);
                }
            }
            catch
            {
                // Ignore teardown errors.
            }

            foreach (var buffer in _buffers)
            {
                buffer?.Dispose();
            }

            _source.Dispose();
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Represents a chunk of deinterleaved 32-bit float PCM samples.
    /// </summary>
    public readonly struct StreamChunk
    {
        public StreamChunk(float[][] channels, int frames, bool endOfStream = false)
        {
            Channels = channels ?? Array.Empty<float[]>();
            Frames = frames;
            EndOfStream = endOfStream;
        }

        public float[][] Channels { get; }

        public int Frames { get; }

        public bool EndOfStream { get; }
    }
}
