using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading;
using TDV.Audio;

namespace TDV.Audio.TestHrtf
{
    internal static class Program
    {
        private const double CircleDurationSeconds = 25.0;
        private const float Radius = 50.0f;
        private const int UpdateIntervalMs = 20;

        private static int Main(string[] args)
        {
            string alarm5Path = @"D:\Three-D-Velocity\TDV\Three-D-Velocity-Binaries\s\alarm5.wav";
            string alarm7Path = @"D:\Three-D-Velocity\TDV\Three-D-Velocity-Binaries\s\alarm7.wav";

            if (!File.Exists(alarm5Path) || !File.Exists(alarm7Path))
            {
                Console.WriteLine("Files not found.");
                return 2;
            }

            try
            {
                var config = new AudioSystemConfig
                {
                    UseHrtf = true,
                    HrtfMode = HrtfMode.Mono,
                    HrtfDownmixMode = HrtfDownmixMode.Left,
                    HrtfSofaPath = null,
                    PeriodSizeInFrames = 256,
                    Channels = 2,
                    UseCurveDistanceScaler = true,
                    CurveDistanceScaler = 3000.0f,
                    DistanceModel = DistanceModel.Inverse,
                    MinDistance = 1.0f,
                    MaxDistance = 1000.0f,
                    RollOff = 1.0f
                };

                var system = new AudioSystem(config);
                var output = system.CreateOutput(new AudioOutputConfig { Name = "main" });

                Console.WriteLine("Initialized. HRTF active: " + system.IsHrtfActive);

                Vector3 listenerPos = Vector3.Zero;
                Vector3 listenerForward = new Vector3(0, 0, 1);
                Vector3 listenerUp = new Vector3(0, 1, 0);
                Vector3 listenerVel = Vector3.Zero;
                output.UpdateListener(listenerPos, listenerForward, listenerUp, listenerVel);

                Console.WriteLine("Loading sounds...");
                var alarm5 = output.CreateSource(alarm5Path, true);
                var alarm7 = output.CreateSource(alarm7Path, true);

                // Apply the scaler explicitly as the game does
                alarm5.ApplyCurveDistanceScaler(3000.0f);
                alarm7.ApplyCurveDistanceScaler(3000.0f);

                // Set initial position far away to test attenuation
                alarm5.SetPosition(new Vector3(5000, 0, 0));
                alarm7.SetPosition(new Vector3(5000, 0, 0)); 

                var stopwatch = Stopwatch.StartNew();
                double startTime = stopwatch.Elapsed.TotalSeconds;
                
                // Match the value found in Aircraft.cs: private const float targetSolutionFreqCoefficient = -5 / 80f;
                float targetSolutionFreqCoefficient = -5.0f / 80.0f; 

                Console.WriteLine("Starting simulation loop with coefficient: " + targetSolutionFreqCoefficient);

                while (stopwatch.Elapsed.TotalSeconds - startTime < 15.0)
                {
                    double t = stopwatch.Elapsed.TotalSeconds;
                    // Oscillate degreesDifference between 0 and 20 over time
                    float degreesDifference = (float)(10.0 + 10.0 * Math.Sin(t * 2.0));

                    if (degreesDifference > 5.0f)
                    {
                        // Moving target logic
                        if (alarm7.IsPlaying) alarm7.Stop();
                        
                        // Calculate frequency
                        float freq = targetSolutionFreqCoefficient * degreesDifference;
                        
                        // alarm5.SetFrequency(freq); // Not available on AudioSourceHandle
                        alarm5.SetPitch((float)Math.Pow(2.0, freq / 12.0));
                        alarm5.Play(true);
                        
                        Console.WriteLine($"Diff: {degreesDifference:F2} > 5. Playing Alarm5 at Dist 5000");
                    }
                    else
                    {
                        // Solid lock logic
                        if (alarm5.IsPlaying) alarm5.Stop();
                        
                        alarm7.Play(true);
                        Console.WriteLine($"Diff: {degreesDifference:F2} <= 5. Playing Alarm7 at Dist 5000");
                    }

                    system.Update();
                    Thread.Sleep(20);
                }

                alarm5.Dispose();
                alarm7.Dispose();
                system.Dispose();
                Console.WriteLine("Test finished.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.ToString());
                return 1;
            }
        }
    }
}
