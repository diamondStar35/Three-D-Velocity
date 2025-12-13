/* This source is provided under the GNU AGPLv3  license. You are free to modify and distribute this source and any containing work (such as sound files) provided that:
* - You make available complete source code of modifications, even if the modifications are part of a larger project, and make the modified work available under the same license (GNU AGPLv3).
* - You include all copyright and license notices on the modified source.
* - You state which parts of this source were changed in your work
* Note that containing works (such as SharpDX) may be available under a different license.
* Copyright (C) Munawar Bijani
*/
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using OpenALSoft.Net.Core;
using AlVector3 = OpenALSoft.Net.Core.Vector3;
using DxVector3 = SharpDX.Vector3;
using System.Linq;

namespace BPCSharedComponent.ExtendedAudio
{
	public class DSound
	{
		/// <summary>
		/// Various speaker configurations. The configuration of the system can be gotten from MasteringVoice.ChannelMask.
		/// This enum was built with the help of https://devel.nuclex.org/external/svn/directx/trunk/include/audiodefs.h and the SharpDX MasteringVoice source.
		/// </summary>
		private enum SpeakerConfiguration
		{
			mono = Speakers.FrontCenter,
			stereo = Speakers.FrontLeft|Speakers.FrontRight,
			twoPointOne = Speakers.FrontLeft|Speakers.FrontRight|Speakers.LowFrequency,
			surround = Speakers.FrontLeft|Speakers.FrontRight|Speakers.FrontCenter|Speakers.BackCenter,
			quad = Speakers.FrontLeft|Speakers.FrontRight|Speakers.BackLeft|Speakers.BackRight,
			fourPointOne = Speakers.FrontLeft|Speakers.FrontRight|Speakers.LowFrequency|Speakers.BackLeft|Speakers.BackRight,
			fivePointOne = Speakers.FrontLeft|Speakers.FrontRight|Speakers.FrontCenter|Speakers.LowFrequency|Speakers.BackLeft|Speakers.BackRight,
			// SharpDX doesn't define constants for leftOfCenter and rightOfCenter. These constants were obtained from https://devel.nuclex.org/external/svn/directx/trunk/include/audiodefs.h
			sevenPointOne = Speakers.FrontLeft|Speakers.FrontRight|Speakers.FrontCenter|Speakers.LowFrequency|Speakers.BackLeft|Speakers.BackRight | 0x00000040 | 0x00000080,
			fivePointOneSurround = Speakers.FrontLeft | Speakers.FrontRight | Speakers.FrontCenter | Speakers.LowFrequency | Speakers.SideLeft | Speakers.SideRight,
			sevenPointOneSurround = Speakers.FrontLeft | Speakers.FrontRight | Speakers.FrontCenter | Speakers.LowFrequency | Speakers.BackLeft|Speakers.BackRight| Speakers.SideLeft | Speakers.SideRight
		}
		private static String rootDir;
		public static string SFileName;
		private static XAudio2 mainSoundDevice, musicDevice, alwaysLoudDevice, cutScenesDevice;
		private static MasteringVoice mainMasteringVoice, musicMasteringVoice, alwaysLoudMasteringVoice, cutScenesMasteringVoice;
		private static AudioDevice alDevice;
		private static AudioContext alContext;
		public static bool IsHrtfActive => alContext?.IsHrtfEnabled ?? false;
		//used to hold sounds path
		public static string SoundPath;
		//used to hold narratives
		public static string NSoundPath;
		//used to hold numbers
		public static string NumPath;



		/// <summary>
		/// Initializes the sound library for playback.
		/// </summary>
		/// <param name="root">The root directory of the sounds.</param>
		public static void initialize(String root)
		{
			setRootDirectory(root);
			SoundPath = "s";
			NSoundPath = SoundPath + "\\n";
			NumPath = NSoundPath + "\\ns";
			mainSoundDevice = new XAudio2();
			mainMasteringVoice = new MasteringVoice(mainSoundDevice);
			musicDevice = new XAudio2();
			musicMasteringVoice = new MasteringVoice(musicDevice);
			alwaysLoudDevice = new XAudio2();
			alwaysLoudMasteringVoice = new MasteringVoice(alwaysLoudDevice);
			cutScenesDevice = new XAudio2();
			cutScenesMasteringVoice = new MasteringVoice(cutScenesDevice);

			var devices = AudioDevice.EnumerateDevices();
			var hrtfDevice = devices.FirstOrDefault(d => new AudioDevice(d).IsHrtfSupported);
			if (hrtfDevice != null)
			{
				alDevice = new AudioDevice(hrtfDevice);
				var attributes = new AudioContextAttributes().Hrtf(true).ToArray();
				alContext = new AudioContext(alDevice, attributes);
			}
			else
			{
				alDevice = new AudioDevice();
				alContext = new AudioContext(alDevice);
			}

			alContext.MakeCurrent();
			AudioContext.DistanceModel = AudioDistanceModel.InverseDistanceClamped;
			//get the listener:
			setListener();
		}

		/// <summary>
		/// Loads a wave file into a SourceVoice.
		/// </summary>
		/// <param name="FileName">The path of the file to load.</param>
		/// <param name="device">The XAudio2 device to load the sound on.</param>
		/// <param name="notificationsSupport">True to enable receiving notifications on this buffer, false otherwise. A notification might include an event when this buffer starts processing data, or when the buffer has finished playing. Set this parameter to true if you wish to receive a notification when the buffer is done playing by means of the function passed to setOnEnd.</param>
		/// <returns>A populated ExtendedAudioBuffer.</returns>
		public static ExtendedAudioBuffer LoadSound(string FileName, XAudio2 device, bool notificationsSupport)
		{
			if (!File.Exists(FileName)) {
				throw (new ArgumentException("The sound " + FileName + " could not be found."));
			}
			SoundStream stream = new SoundStream(File.OpenRead(FileName));
			WaveFormat format = stream.Format; // So we don't lose reference to it when we close the stream.
			SharpDX.XAudio2.AudioBuffer buffer = new SharpDX.XAudio2.AudioBuffer { Stream = stream.ToDataStream(), AudioBytes = (int)stream.Length, Flags = SharpDX.XAudio2.BufferFlags.EndOfStream };
			// We can now safely close the stream.
			stream.Close();
			SourceVoice sv = new SourceVoice(device, format, VoiceFlags.None, 5.0f, notificationsSupport);
			return new ExtendedAudioBuffer(buffer, sv, format);
		}

		/// <summary>
		/// Loads a wave file into a SourceVoice on the main device, with notifications disabled.
		/// </summary>
		/// <param name="FileName">The path of the file to load.</param>
		/// <returns>A populated ExtendedAudioBuffer.</returns>
		public static ExtendedAudioBuffer LoadSound(string FileName)
		{
			return LoadSound(FileName, mainSoundDevice, false);
		}

		/// <summary>
		/// Loads a wave file into a SourceVoice on the main device, with the given notificationsSupport flag.
		/// </summary>
		/// <param name="FileName">The path of the file to load.</param>
		/// <param name="notificationsSupport">True to enable receiving notifications on this buffer, false otherwise. A notification might include an event when this buffer starts processing data, or when the buffer has finished playing. Set this parameter to true if you wish to receive a notification when the buffer is done playing by means of the function passed to setOnEnd.</param>
		/// <returns>A populated ExtendedAudioBuffer.</returns>
		public static ExtendedAudioBuffer LoadSound(string FileName, bool notificationsSupport)
		{
			return LoadSound(FileName, mainSoundDevice, notificationsSupport);
		}

		/// <summary>
		/// Loads a wave file into a SourceVoice on the always loud device.
		/// </summary>
		/// <param name="FileName">The path of the file to load.</param>
		/// <param name="notificationsSupport">True to enable receiving notifications on this buffer, false otherwise. A notification might include an event when this buffer starts processing data, or when the buffer has finished playing. Set this parameter to true if you wish to receive a notification when the buffer is done playing by means of the function passed to setOnEnd.</param>
		/// <returns>A populated ExtendedAudioBuffer.</returns>
		public static ExtendedAudioBuffer LoadSoundAlwaysLoud(string FileName, bool notificationsSupport = false)
		{
			return LoadSound(FileName, alwaysLoudDevice, notificationsSupport);
		}

		public static ExtendedAudioBuffer LoadTone(byte[] tone)
		{
			WaveFormat format = new WaveFormat(44100, 16, 1);
			MemoryStream ms = new MemoryStream(tone);
			SharpDX.DataStream dataStream = new SharpDX.DataStream(ms.ToArray().Length, true, true);
			dataStream.Write(ms.ToArray(), 0, ms.ToArray().Length);
			dataStream.Position = 0; // Reset position for reading
			SharpDX.XAudio2.AudioBuffer buffer = new SharpDX.XAudio2.AudioBuffer { Stream = dataStream, AudioBytes = (int)dataStream.Length, Flags = SharpDX.XAudio2.BufferFlags.EndOfStream };
			SourceVoice sv = new SourceVoice(mainSoundDevice, format, VoiceFlags.None, 5.0f, false);
			return new ExtendedAudioBuffer(buffer, sv, format);
		}

		/// <summary>
		/// Creates a new listener object with all of its values set to the default unit vectors per the documentation.
		/// </summary>
		public static void setListener()
		{
			AudioListener.Position = new AlVector3(0, 0, 0);
			AudioListener.Orientation = (new AlVector3(0, 0, -1), new AlVector3(0, 1, 0));
			AudioListener.Velocity = new AlVector3(0, 0, 0);
		}

		public static DxVector3 getListenerPosition()
		{
			var pos = AudioListener.Position;
			return new DxVector3(pos.X, pos.Y, pos.Z);
		}

		public static DxVector3 getListenerOrientFront()
		{
			var (at, _) = AudioListener.Orientation;
			return new DxVector3(at.X, at.Y, at.Z);
		}

		/// <summary>
		/// Orients the listener. The x, y and z values are the respective components of the front and top vectors of the listener. For instance, to orient the listener to its default orientation, one should call setOrientation(0,0,1,0,1,0), IE: the default orientation vectors.
		/// </summary>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="z1"></param>
		/// <param name="x2"></param>
		/// <param name="y2"></param>
		/// <param name="z2"></param>
		public static void setOrientation(float x1, float y1, float z1, float x2=0, float y2=1, float z2=0)
		{
			AudioListener.Orientation = (new AlVector3(x1, y1, z1), new AlVector3(x2, y2, z2));
		}

		/// <summary>
		/// Sets the velocity of the listener.
		/// </summary>
		/// <param name="x">The x component of the velocity vector.</param>
		/// <param name="y">The y component of the velocity vector.</param>
		/// <param name="z">The z component of the velocity vector.</param>
		public static void setVelocity(float x, float y, float z)
		{
			AudioListener.Velocity = new AlVector3(x, y, z);
		}

		/// <summary>
		/// Plays a sound.
		/// </summary>
		/// <param name="sound">The ExtendedAudioBuffer to play.</param>
		/// <param name="stop">If true, will stop the sound and return its position to 0 before playing it. Passing false will have the effect of resuming the sound from the last position it was stopped at.</param>
		/// <param name="loop">Whether or not to loop the sound.</param>
		public static void PlaySound(ExtendedAudioBuffer sound, bool stop, bool loop)
		{
			sound.Is3D = false;
			sound.play(stop, loop);
		}

		/// <summary>
		/// Positions a sound in 3-D space
		/// </summary>
		/// <param name="sound">The ExtendedAudioBuffer to play.</param>
		/// <param name="stop">If true, will stop the sound and return its position to 0 before playing it. Passing false will have the effect of resuming the sound from the last position it was stopped at.</param>
		/// <param name="loop">Whether or not to loop the sound.</param>
		/// <param name="x">The x coordinate of the source.</param>
		/// <param name="y">The y coordinate of the source (in-game Z, or depth).</param>
		/// <param name="z">The z coordinate of the source (in-game Y, or height).</param>
		/// <param name="vx">The x component of the velocity vector.</param>
		/// <param name="vy">The y component of the velocity  vector (in-game Z, or depth).</param>
		/// <param name="vz">The z component of the velocity vector (in-game Y, or height).</param>
		/// <param name="flags">Not used by the OpenAL implementation.</param>
		public static void PlaySound3d(ExtendedAudioBuffer sound, bool stop, bool loop, float x, float y, float z, float vx=0, float vy=0, float vz=0, CalculateFlags flags = CalculateFlags.Matrix | CalculateFlags.Doppler | CalculateFlags.LpfDirect | CalculateFlags.LpfReverb, float curveDistanceScaler = 1.0f)
		{
			sound.Is3D = true;
			sound.ensureAlObjects();

			if (stop)
			{
				sound.stop();
			}

			// Game uses (X, Z, Y), OpenAL uses (X, Y, -Z)
			sound.AlSource.Position = new AlVector3(x, z, -y);
			sound.AlSource.Velocity = new AlVector3(vx, vz, -vy);
			
			sound.AlSource.ReferenceDistance = 1.0f * curveDistanceScaler;
			sound.AlSource.MaxDistance = 1000.0f * curveDistanceScaler;
			sound.AlSource.RolloffFactor = 1.0f;
			sound.AlSource.Looping = loop;
			sound.AlSource.Spatialize = SpatializationMode.On;

			if (sound.AlSource.State != AudioSourceState.Playing)
			{
				sound.AlSource.Play();
			}
		}


		/// <summary>
		/// Sets the position of the listener.
		/// </summary>
		/// <param name="x">The x coordinate of the listener.</param>
		/// <param name="y">The y coordinate of the listener.</param>
		/// <param name="z">The z coordinate of the listener.</param>
		public static void SetCoordinates(float x, float y, float z)
		{
			AudioListener.Position = new AlVector3(x, y, z);
		}

		/// <summary>
		/// Used to create a playing chain. The last files will be looped indefinitely and the files before it will only play once, in order.
		/// </summary>
		/// <param name="device">The XAudio2 device to load the files on.</param>
		/// <param name="fileNames">A list of file names to play, where the last one is looped indefinitely if more than one file is provided.</param>
		/// <returns>An ogg buffer that is ready to be played.</returns>
		public static OggBuffer loadOgg(XAudio2 device, params string[] fileNames)
		{
			for (int i = 0; i < fileNames.Length; i++) {
				if (!File.Exists(fileNames[i]))
					throw (new ArgumentException("The sound " + fileNames[i] + " could not be found."));
			}
			return new OggBuffer(device, fileNames);
		}

		/// <summary>
		/// Loads a music file using the musicDevice.
		/// </summary>
		/// <param name="filenames">The file names to load. For multi-track files, these should be passed in the order in which they are to be played. The last one will be looped.</param>
		/// <returns>An OggBuffer.</returns>
		public static OggBuffer loadMusicFile(params String[] filenames)
		{
			return loadOgg(musicDevice, filenames);
		}

		/// <summary>
		/// Loads the specified Ogg files onto the cut scenes device.
		/// </summary>
		/// <param name="filenames">The list of file names to loads.</param>
		/// <returns>An OggBuffer.</returns>
		public static OggBuffer loadOgg(params String[] filenames)
		{
			return loadOgg(cutScenesDevice, filenames);
		}

		/// <summary>
		/// Unloads the sound from memory. The memory will be freed and the object reference will be set to NULL. The sound will also be stopped if it is playing.
		/// </summary>
		/// <param name="sound">The sound to unload.</param>
		[MethodImplAttribute(MethodImplOptions.Synchronized)]
		public static void unloadSound(ref ExtendedAudioBuffer sound)
		{
			if (sound == null) {
				return;
			}
			sound.stop();
			sound.Dispose();
			sound = null;
		}

		/// <summary>
		///  Checks to see if a sound is playing.
		/// </summary>
		/// <param name="s">The sound to check</param>
		/// <returns>True if the sound is playing, false otherwise</returns>
		public static bool isPlaying(ExtendedAudioBuffer s)
		{
			return s.state == ExtendedAudioBuffer.State.playing;
		}

		/// <summary>
		///  Loads and plays the specified wave file, and disposes it after it is done playing.
		/// </summary>
		/// <param name="fn">The name of the file to play.</param>
		public static void playAndWait(String fn)
		{
			ExtendedAudioBuffer s = LoadSound(fn);
			PlaySound(s, true, false);
			while (isPlaying(s))
				Thread.Sleep(100);
			s.Dispose();
			s = null;
		}

		/// <summary>
		/// Gets rid of audio objects.
		/// </summary>
		public static void cleanUp()
		{
			musicMasteringVoice.Dispose();
			musicDevice.Dispose();
			mainMasteringVoice.Dispose();
			mainSoundDevice.Dispose();
			alContext?.Dispose();
			alDevice?.Dispose();
		}

		/// <summary>
		/// Sets the root directory for sounds.
		/// </summary>
		/// <param name="root">The path of the root directory.</param>
		public static void setRootDirectory(String root)
		{
			rootDir = root;
		}

		/// <summary>
		/// Pans a sound.
		/// This method was written using the guide at https://docs.microsoft.com/en-us/windows/win32/xaudio2/how-to--pan-a-sound
		/// </summary>
		/// <param name="sound">The sound to pan.</param>
		/// <param name="pan">The value by which to pan the sound. -1.0f is completely left, and 1.0f is completely right. 0.0f is center.</param>
		public static void setPan(ExtendedAudioBuffer sound, float pan)
		{
			SpeakerConfiguration mask = (SpeakerConfiguration)mainMasteringVoice.ChannelMask;
			float[] outputMatrix = new float[8];
			float left = 0.5f - pan / 2;
			float right = 0.5f + pan / 2;
			switch(mask) {
				case SpeakerConfiguration.mono:
					outputMatrix[0] = 1.0f;
					break;
				case SpeakerConfiguration.stereo:
				case SpeakerConfiguration.twoPointOne:
				case SpeakerConfiguration.surround:
					outputMatrix[0] = left;
					outputMatrix[1] = right;
					break;
				case SpeakerConfiguration.quad:
					outputMatrix[0] = outputMatrix[2] = left;
					outputMatrix[1] = outputMatrix[3] = right;
					break;
				case SpeakerConfiguration.fourPointOne:
					outputMatrix[0] = outputMatrix[3] = left;
					outputMatrix[1] = outputMatrix[4] = right;
					break;
				case SpeakerConfiguration.fivePointOne:
				case SpeakerConfiguration.sevenPointOne:
				case SpeakerConfiguration.fivePointOneSurround:
					outputMatrix[0] = outputMatrix[4] = left;
					outputMatrix[1] = outputMatrix[5] = right;
					break;
				case SpeakerConfiguration.sevenPointOneSurround:
					outputMatrix[0] = outputMatrix[4] = outputMatrix[6] = left;
					outputMatrix[1] = outputMatrix[5] = outputMatrix[7] = right;
					break;
			}
			VoiceDetails soundDetails = sound.getVoiceDetails();
			VoiceDetails masteringDetails = mainMasteringVoice.VoiceDetails;
			sound.setOutputMatrix(soundDetails.InputChannelCount, masteringDetails.InputChannelCount, outputMatrix);
		}

		/// <summary>
		/// Sets the volume of the background music. This method will clamp the volume between the allowable range.
		/// </summary>
		/// <param name="v">The volume to set the music to.</param>
		public static void setVolumeOfMusic(float v)
		{
			if (v < 0.0f)
				v = 0.0f;
			else if (v > 1.0f)
				v = 1.0f;
			musicMasteringVoice.SetVolume(v);
		}

		/// <summary>
		/// Gets the volume of the music.
		/// </summary>
		/// <returns>The volume.</returns>
		public static float getVolumeOfMusic()
		{
			musicMasteringVoice.GetVolume(out float v);
			return v;
		}

		/// <summary>
		/// Sets the volume of the sounds excluding music. This method will clamp the volume between the allowable range.
		/// </summary>
		/// <param name="v">The volume to set the sounds to.</param>
		public static void setVolumeOfSounds(float v)
		{
			mainMasteringVoice.SetVolume(v);
		}
	}
}
