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
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: TDV.Audio.TestHrtf <wavPath> [hrtfSofaPath]");
                return 2;
            }

            string wavPath = args[0];
            string hrtfPath = args.Length > 1 ? args[1] : null;

            if (!File.Exists(wavPath))
            {
                Console.WriteLine("File not found: " + wavPath);
                return 2;
            }

            try
            {
                var config = new AudioSystemConfig
                {
                    UseHrtf = true,
                    HrtfMode = HrtfMode.Mono,
                    HrtfDownmixMode = HrtfDownmixMode.Left,
                    HrtfSofaPath = hrtfPath,
                    PeriodSizeInFrames = 256,
                    Channels = 2,
                    UseCurveDistanceScaler = false,
                    DistanceModel = DistanceModel.Linear,
                    MinDistance = 1.0f,
                    MaxDistance = 1000.0f,
                    RollOff = 0.1f
                };

                var system = new AudioSystem(config);
                var output = system.CreateOutput(new AudioOutputConfig { Name = "main" });

                Console.WriteLine("Initialized. HRTF active: " + system.IsHrtfActive);

                Vector3 listenerPos = Vector3.Zero;
                Vector3 listenerForward = new Vector3(0, 0, 1);
                Vector3 listenerUp = new Vector3(0, 1, 0);
                Vector3 listenerVel = Vector3.Zero;
                output.UpdateListener(listenerPos, listenerForward, listenerUp, listenerVel);

                var source = output.CreateSource(wavPath, true);
                source.SetPosition(new Vector3(Radius, 0, 0));
                source.SetVelocity(Vector3.Zero);
                source.Play(false);

                var stopwatch = Stopwatch.StartNew();
                var last = stopwatch.Elapsed;
                Vector3 lastPos = new Vector3(Radius, 0, 0);

                while (source.IsPlaying)
                {
                    var now = stopwatch.Elapsed;
                    double totalSeconds = now.TotalSeconds;
                    double dt = (now - last).TotalSeconds;
                    if (dt <= 0)
                        dt = UpdateIntervalMs / 1000.0;

                    double phase = (totalSeconds % CircleDurationSeconds) / CircleDurationSeconds;
                    double angle = phase * Math.PI * 2.0;

                    float x = (float)(Math.Cos(angle) * Radius);
                    float z = (float)(Math.Sin(angle) * Radius);
                    Vector3 pos = new Vector3(x, 0, z);
                    Vector3 vel = (pos - lastPos) / (float)dt;

                    source.SetPosition(pos);
                    source.SetVelocity(vel);

                    system.Update();

                    last = now;
                    lastPos = pos;

                    Thread.Sleep(UpdateIntervalMs);
                }

                source.Dispose();
                system.Dispose();
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
