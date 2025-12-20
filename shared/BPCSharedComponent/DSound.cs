/* This source is provided under the GNU AGPLv3  license. You are free to modify and distribute this source and any containing work (such as sound files) provided that:
* - You make available complete source code of modifications, even if the modifications are part of a larger project, and make the modified work available under the same license (GNU AGPLv3).
* - You include all copyright and license notices on the modified source.
* - You state which parts of this source were changed in your work
* Note that containing works (such as SharpDX) may be available under a different license.
* Copyright (C) Munawar Bijani
*/
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using TdvAudio = TDV.Audio;

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
                private static X3DAudio x3DAudio;
                private static Listener listener;
                //used to hold sounds path
                public static string SoundPath;
                //used to hold narratives
                public static string NSoundPath;
                //used to hold numbers
                public static string NumPath;
                private static readonly Dictionary<ExtendedAudioBuffer, string> soundFileMap = new Dictionary<ExtendedAudioBuffer, string>();
                private static readonly Dictionary<ExtendedAudioBuffer, TdvAudio.ExtendedAudioBuffer> spatialSoundMap = new Dictionary<ExtendedAudioBuffer, TdvAudio.ExtendedAudioBuffer>();
                private static readonly object spatialLock = new object();
                private static Thread spatialUpdateThread;
                private static volatile bool spatialUpdateRunning;

		/// <summary>
		/// Initializes the sound library for playback.
		/// </summary>
		/// <param name="root">The root directory of the sounds.</param>
		public static void initialize(String root)
                {
                        initialize(root, false);
                }

                public static void initialize(String root, bool hrtfEnabled)
                {
                        StopSpatialUpdateThread();
                        DisposeAllSpatialBuffers();
                        if (TdvAudio.DSound.IsInitialized)
                                TdvAudio.DSound.cleanUp();

                        setRootDirectory(root);
                        SoundPath = "s";
                        NSoundPath = SoundPath + "\\n";
                        NumPath = NSoundPath + "\\ns";
                        mainSoundDevice = new XAudio2();
                        mainMasteringVoice = new MasteringVoice(mainSoundDevice);
                        x3DAudio = new X3DAudio((Speakers)mainMasteringVoice.ChannelMask);
                        musicDevice = new XAudio2();
                        musicMasteringVoice = new MasteringVoice(musicDevice);  
                        alwaysLoudDevice = new XAudio2();
                        alwaysLoudMasteringVoice = new MasteringVoice(alwaysLoudDevice);
                        cutScenesDevice = new XAudio2();
                        cutScenesMasteringVoice = new MasteringVoice(cutScenesDevice);
                        InitializeSpatialAudio(root, hrtfEnabled);
                        //get the listener:
                        setListener();
                        StartSpatialUpdateThread();
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
                        AudioBuffer buffer = new AudioBuffer { Stream = stream.ToDataStream(), AudioBytes = (int)stream.Length, Flags = SharpDX.XAudio2.BufferFlags.EndOfStream };
                        // We can now safely close the stream.
                        stream.Close();
                        SourceVoice sv = new SourceVoice(device, format, VoiceFlags.None, 5.0f, notificationsSupport);
                        ExtendedAudioBuffer sound = new ExtendedAudioBuffer(buffer, sv);
                        RegisterSoundFile(sound, FileName);
                        return sound;
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
                        AudioBuffer buffer = new AudioBuffer { Stream = dataStream, AudioBytes = (int)dataStream.Length, Flags = SharpDX.XAudio2.BufferFlags.EndOfStream };
                        SourceVoice sv = new SourceVoice(mainSoundDevice, format, VoiceFlags.None, 5.0f, false);
                        ExtendedAudioBuffer sound = new ExtendedAudioBuffer(buffer, sv);
                        RegisterTone(sound, tone);
                        return sound;
                }

                private static void InitializeSpatialAudio(String root, bool hrtfEnabled)
                {
                        TdvAudio.AudioSystemConfig config = TdvAudio.DSound.Config;
                        config.UseHrtf = hrtfEnabled;
                        config.HrtfMode = TdvAudio.HrtfMode.Mono;
                        config.HrtfDownmixMode = TdvAudio.HrtfDownmixMode.Average;
                        config.DistanceModel = TdvAudio.DistanceModel.Inverse;
                        config.UseCurveDistanceScaler = true;
                        config.CurveDistanceScaler = 1.0f;
                        config.MinDistance = 1.0f;
                        config.MaxDistance = 1000000000.0f;
                        config.RollOff = 1.0f;
                        config.SpeedOfSound = 343.5f;
                        config.DopplerFactor = 1.0f;
                        TdvAudio.DSound.initialize(root);
                }

                private static void StartSpatialUpdateThread()
                {
                        if (spatialUpdateRunning)
                                return;
                        spatialUpdateRunning = true;
                        spatialUpdateThread = new Thread(() =>
                        {
                                while (spatialUpdateRunning)
                                {
                                        if (TdvAudio.DSound.IsInitialized)
                                                TdvAudio.DSound.update();
                                        Thread.Sleep(10);
                                }
                        });
                        spatialUpdateThread.IsBackground = true;
                        spatialUpdateThread.Start();
                }

                private static void StopSpatialUpdateThread()
                {
                        spatialUpdateRunning = false;
                        if (spatialUpdateThread == null)
                                return;
                        spatialUpdateThread.Join(100);
                        spatialUpdateThread = null;
                }

                private static void RegisterSoundFile(ExtendedAudioBuffer sound, String fileName)
                {
                        if (sound == null || String.IsNullOrEmpty(fileName))
                                return;
                        lock (spatialLock)
                        {
                                soundFileMap[sound] = fileName;
                        }
                        if (sound.ExternalDispose == null)
                                sound.ExternalDispose = () => RemoveSoundFile(sound);
                }

                private static void RemoveSoundFile(ExtendedAudioBuffer sound)
                {
                        lock (spatialLock)
                        {
                                soundFileMap.Remove(sound);
                        }
                }

                private static void RegisterTone(ExtendedAudioBuffer sound, byte[] tone)
                {
                        if (sound == null || tone == null || tone.Length == 0)
                                return;
                        if (!TdvAudio.DSound.IsInitialized)
                                return;

                        TdvAudio.ExtendedAudioBuffer spatial = TdvAudio.DSound.LoadTone(tone);
                        lock (spatialLock)
                        {
                                spatialSoundMap[sound] = spatial;
                        }
                        AttachSpatialHandlers(sound, spatial);
                }

                private static TdvAudio.ExtendedAudioBuffer GetSpatialSound(ExtendedAudioBuffer sound)
                {
                        if (sound == null || !TdvAudio.DSound.IsInitialized)
                                return null;
                        lock (spatialLock)
                        {
                                if (spatialSoundMap.TryGetValue(sound, out TdvAudio.ExtendedAudioBuffer spatial))
                                        return spatial;
                                if (!soundFileMap.TryGetValue(sound, out String fileName))
                                        return null;
                                if (String.IsNullOrEmpty(fileName))
                                        return null;
                                spatial = TdvAudio.DSound.LoadSound(fileName);
                                spatialSoundMap[sound] = spatial;
                                AttachSpatialHandlers(sound, spatial);
                                return spatial;
                        }
                }

                private static void AttachSpatialHandlers(ExtendedAudioBuffer sound, TdvAudio.ExtendedAudioBuffer spatial)
                {
                        sound.ExternalIsPlaying = () => spatial.state == TdvAudio.ExtendedAudioBuffer.State.playing;
                        sound.ExternalStop = () => spatial.stop();
                        sound.ExternalSetVolume = v => spatial.setVolume(v);
                        sound.ExternalSetFrequency = f => spatial.setFrequency(f);
                        sound.ExternalDispose = () => DetachSpatialSound(sound);
                }

                private static void DetachSpatialSound(ExtendedAudioBuffer sound)
                {
                        TdvAudio.ExtendedAudioBuffer spatial = null;
                        lock (spatialLock)
                        {
                                if (spatialSoundMap.TryGetValue(sound, out spatial))
                                        spatialSoundMap.Remove(sound);
                                soundFileMap.Remove(sound);
                                ClearSpatialHandlers(sound);
                        }
                        if (spatial != null)
                                spatial.Dispose();
                }

                private static void ClearSpatialHandlers(ExtendedAudioBuffer sound)
                {
                        sound.ExternalIsPlaying = null;
                        sound.ExternalStop = null;
                        sound.ExternalSetVolume = null;
                        sound.ExternalSetFrequency = null;
                        sound.ExternalDispose = null;
                }

                private static void DisposeAllSpatialBuffers()
                {
                        List<TdvAudio.ExtendedAudioBuffer> spatialToDispose = new List<TdvAudio.ExtendedAudioBuffer>();
                        List<ExtendedAudioBuffer> owners = new List<ExtendedAudioBuffer>();
                        lock (spatialLock)
                        {
                                foreach (KeyValuePair<ExtendedAudioBuffer, TdvAudio.ExtendedAudioBuffer> kvp in spatialSoundMap)
                                {
                                        owners.Add(kvp.Key);
                                        spatialToDispose.Add(kvp.Value);
                                }
                                spatialSoundMap.Clear();
                                soundFileMap.Clear();
                        }
                        foreach (ExtendedAudioBuffer owner in owners)
                                ClearSpatialHandlers(owner);
                        foreach (TdvAudio.ExtendedAudioBuffer spatial in spatialToDispose)
                                spatial.Dispose();
                }

                private static TdvAudio.CalculateFlags ConvertFlags(CalculateFlags flags)
                {
                        TdvAudio.CalculateFlags converted = 0;
                        if ((flags & CalculateFlags.Matrix) == CalculateFlags.Matrix)
                                converted |= TdvAudio.CalculateFlags.Matrix;
                        if ((flags & CalculateFlags.Doppler) == CalculateFlags.Doppler)
                                converted |= TdvAudio.CalculateFlags.Doppler;
                        if ((flags & CalculateFlags.LpfDirect) == CalculateFlags.LpfDirect)
                                converted |= TdvAudio.CalculateFlags.LpfDirect;
                        if ((flags & CalculateFlags.LpfReverb) == CalculateFlags.LpfReverb)
                                converted |= TdvAudio.CalculateFlags.LpfReverb;
                        return converted;
                }


		/// <summary>
		/// Creates a new listener object with all of its values set to the default unit vectors per the documentation.
		/// </summary>
                public static void setListener()
                {
                        listener = new Listener
                        {
                                OrientFront = new Vector3(0, 0, 1),
                                OrientTop = new Vector3(0, 1, 0),
                                Position = new Vector3(0, 0, 0),
                                Velocity = new Vector3(0, 0, 0)
                        };
                        if (TdvAudio.DSound.IsInitialized)
                                TdvAudio.DSound.setListener();
                }

		public static Vector3 getListenerPosition()
		{
			return listener.Position;
		}

		public static Vector3 getListenerOrientFront()
		{
			return listener.OrientFront;
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
                        Vector3 front = new Vector3(x1, y1, z1);
                        Vector3 top = new Vector3(x2, y2, z2);
                        listener.OrientFront = front;
                        listener.OrientTop = top;
                        if (TdvAudio.DSound.IsInitialized)
                                TdvAudio.DSound.setOrientation(x1, y1, z1, x2, y2, z2);
                }

		/// <summary>
		/// Sets the velocity of the listener.
		/// </summary>
		/// <param name="x">The x component of the velocity vector.</param>
		/// <param name="y">The y component of the velocity vector.</param>
		/// <param name="z">The z component of the velocity vector.</param>
                public static void setVelocity(float x, float y, float z)
                {
                        listener.Velocity = new Vector3(x, y, z);
                        if (TdvAudio.DSound.IsInitialized)
                                TdvAudio.DSound.setVelocity(x, y, z);
                }

		/// <summary>
		/// Plays a sound.
		/// </summary>
		/// <param name="sound">The ExtendedAudioBuffer to play.</param>
		/// <param name="stop">If true, will stop the sound and return its position to 0 before playing it. Passing false will have the effect of resuming the sound from the last position it was stopped at.</param>
		/// <param name="loop">Whether or not to loop the sound.</param>
		public static void PlaySound(ExtendedAudioBuffer sound, bool stop, bool loop)
		{
			sound.play(stop, loop);
		}

		/// <summary>
		/// Positions a sound in 3-D space
		/// </summary>
		/// <param name="sound">The ExtendedAudioBuffer to play.</param>
		/// <param name="stop">If true, will stop the sound and return its position to 0 before playing it. Passing false will have the effect of resuming the sound from the last position it was stopped at.</param>
		/// <param name="loop">Whether or not to loop the sound.</param>
		/// <param name="x">The x coordinate of the source.</param>
		/// <param name="y">The y coordinate of the source.</param>
		/// <param name="z">The z coordinate of the source.</param>
		/// <param name="vx">The x component of the velocity vector.</param>
		/// <param name="vy">The y component of the velocity  vector.</param>
		/// <param name="vz">The z component of the velocity vector.</param>
		/// <param name="flags">The 3D flags to calculate. The default will calculate volume and doppler shift. This parameter is useful if it is not desirable for XAudio2 to calculate doppler on sounds that modify their own frequencies as an example; in this case, the flags should omit doppler.</param>
                public static void PlaySound3d(ExtendedAudioBuffer sound, bool stop, bool loop, float x, float y, float z, float vx=0, float vy=0, float vz=0, CalculateFlags flags = CalculateFlags.Matrix | CalculateFlags.Doppler, float curveDistanceScaler = 1.0f)
                {
                        TdvAudio.ExtendedAudioBuffer spatialSound = GetSpatialSound(sound);
                        if (spatialSound != null)
                        {
                                if (curveDistanceScaler <= 0.0f)
                                        curveDistanceScaler = 1.0f;
                                spatialSound.setVolume(sound.getVolume());
                                spatialSound.setFrequency(sound.getFrequency());
                                TdvAudio.CalculateFlags tdvFlags = ConvertFlags(flags);
                                TdvAudio.DSound.PlaySound3d(spatialSound, stop, loop, x, y, z, vx, vy, vz, tdvFlags, curveDistanceScaler);
                                return;
                        }

                        Emitter emitter = new Emitter {
                                ChannelCount = 1,
                                CurveDistanceScaler = curveDistanceScaler,
                                OrientFront = new Vector3(0, 0, 1),
                                OrientTop = new Vector3(0, 1, 0),
                                Position = new Vector3(x, y, z),
                                Velocity = new Vector3(vx, vy, vz)
                        };
                        sound.play(stop, loop);
                        DspSettings dspSettings = x3DAudio.Calculate(listener, emitter, flags, sound.getVoiceDetails().InputChannelCount, mainMasteringVoice.VoiceDetails.InputChannelCount);
                        sound.apply3D(dspSettings, sound.getVoiceDetails().InputChannelCount, mainMasteringVoice.VoiceDetails.InputChannelCount, flags);        
                }


		/// <summary>
		/// Sets the position of the listener.
		/// </summary>
		/// <param name="x">The x coordinate of the listener.</param>
		/// <param name="y">The y coordinate of the listener.</param>
		/// <param name="z">The z coordinate of the listener.</param>
                public static void SetCoordinates(float x, float y, float z)
                {
                        listener.Position = new Vector3(x, y, z);
                        if (TdvAudio.DSound.IsInitialized)
                                TdvAudio.DSound.SetCoordinates(x, y, z);
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
                        lock (spatialLock)
                        {
                                soundFileMap.Remove(sound);
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
                        StopSpatialUpdateThread();
                        DisposeAllSpatialBuffers();
                        if (TdvAudio.DSound.IsInitialized)
                                TdvAudio.DSound.cleanUp();
                        musicMasteringVoice.Dispose();
                        musicDevice.Dispose();
                        mainMasteringVoice.Dispose();
                        mainSoundDevice.Dispose();
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
                        if (TdvAudio.DSound.IsInitialized)
                                TdvAudio.DSound.setVolumeOfSounds(v);
                }
	}
}
