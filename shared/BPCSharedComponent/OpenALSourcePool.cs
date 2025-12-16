/* This source is provided under the GNU AGPLv3  license. You are free to modify and distribute this source and any containing work (such as sound files) provided that:
* - You make available complete source code of modifications, even if the modifications are part of a larger project, and make the modified work available under the same license (GNU AGPLv3).
* - You include all copyright and license notices on the modified source.
* - You state which parts of this source were changed in your work
* Note that containing works (such as SharpDX) may be available under a different license.
* Copyright (C) Munawar Bijani
*/
using System.Collections.Generic;
using System.Linq;
using OpenALSoft.Net.Core;

namespace BPCSharedComponent.ExtendedAudio
{
	internal static class OpenALSourcePool
	{
		// Cap to avoid exhausting OpenAL and crashing the host process while still allowing many simultaneous sounds.
		private const int MaxSources = 256;

		private static readonly object Sync = new object();
		private static readonly Queue<AudioSource> Available = new Queue<AudioSource>();
		private static readonly Dictionary<AudioSource, ExtendedAudioBuffer> InUse = new Dictionary<AudioSource, ExtendedAudioBuffer>();

		/// <summary>
		/// Acquire an audio source for the given buffer. Returns null when no source is available.
		/// </summary>
		internal static AudioSource Rent(ExtendedAudioBuffer owner)
		{
			if (owner == null)
				return null;

			lock (Sync)
			{
				ReclaimFinishedUnsafe();

				if (Available.Count > 0)
				{
					var src = Available.Dequeue();
					InUse[src] = owner;
					return src;
				}

				if (InUse.Count < MaxSources)
				{
					try
					{
						var src = new AudioSource();
						InUse[src] = owner;
						return src;
					}
					catch
					{
						return null;
					}
				}

				// As a last resort, steal a stopped source.
				AudioSource stoppedSource = null;
				ExtendedAudioBuffer stoppedOwner = null;
				foreach (var kvp in InUse)
				{
					if (kvp.Key.State != AudioSourceState.Playing)
					{
						stoppedSource = kvp.Key;
						stoppedOwner = kvp.Value;
						break;
					}
				}

				if (stoppedSource != null)
				{
					FreeOwnerReference(stoppedSource, stoppedOwner);
					InUse.Remove(stoppedSource);
					SafeStopAndDetach(stoppedSource);
					InUse[stoppedSource] = owner;
					return stoppedSource;
				}

				return null;
			}
		}

		internal static void Return(ExtendedAudioBuffer owner)
		{
			if (owner == null)
				return;

			lock (Sync)
			{
				var source = owner.AlSource;
				if (source == null)
					return;

				owner.AlSource = null;
				SafeStopAndDetach(source);
				if (source != null && InUse.ContainsKey(source))
					InUse.Remove(source);
				Available.Enqueue(source);
			}
		}

		internal static ExtendedAudioBuffer.State GetState(ExtendedAudioBuffer owner)
		{
			lock (Sync)
			{
				if (owner?.AlSource == null)
					return ExtendedAudioBuffer.State.stopped;

				var source = owner.AlSource;
				if (source.State == AudioSourceState.Playing)
					return ExtendedAudioBuffer.State.playing;

				// If the source has stopped, return it to the pool so new sounds can play.
				Return(owner);
				return ExtendedAudioBuffer.State.stopped;
			}
		}

		internal static void ReclaimFinished()
		{
			lock (Sync)
			{
				ReclaimFinishedUnsafe();
			}
		}

		internal static void DisposeAll()
		{
			lock (Sync)
			{
				foreach (var kvp in InUse.ToArray())
				{
					SafeDisposeSource(kvp.Key, kvp.Value);
				}

				foreach (var src in Available)
				{
					SafeDisposeSource(src, null);
				}

				InUse.Clear();
				Available.Clear();
			}
		}

		private static void ReclaimFinishedUnsafe()
		{
			var finished = InUse.Where(kvp => kvp.Key.State != AudioSourceState.Playing).ToList();
			foreach (var kvp in finished)
			{
				FreeOwnerReference(kvp.Key, kvp.Value);
				InUse.Remove(kvp.Key);
				SafeStopAndDetach(kvp.Key);
				Available.Enqueue(kvp.Key);
			}
		}

		private static void FreeOwnerReference(AudioSource source, ExtendedAudioBuffer owner)
		{
			if (owner != null && ReferenceEquals(owner.AlSource, source))
			{
				owner.AlSource = null;
			}
		}

		private static void SafeStopAndDetach(AudioSource source)
		{
			if (source == null)
				return;

			try
			{
				source.Stop();
				source.Buffer = null;
			}
			catch
			{
				// Best effort cleanup; pool will continue even if AL rejects commands for a stopped context.
			}
		}

		private static void SafeDisposeSource(AudioSource source, ExtendedAudioBuffer owner)
		{
			try
			{
				SafeStopAndDetach(source);
				source?.Dispose();
			}
			catch
			{
				// Ignore disposal failures; sources are being torn down during shutdown.
			}
		}
	}
}
