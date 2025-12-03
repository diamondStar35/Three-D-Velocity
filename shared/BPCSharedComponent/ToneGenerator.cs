/* This source is provided under the GNU AGPLv3  license. You are free to modify and distribute this source and any containing work (such as sound files) provided that:
* - You make available complete source code of modifications, even if the modifications are part of a larger project, and make the modified work available under the same license (GNU AGPLv3).
* - You include all copyright and license notices on the modified source.
* - You state which parts of this source were changed in your work
* Note that containing works (such as SharpDX) may be available under a different license.
* Copyright (C) Munawar Bijani
*/
using System;

namespace BPCSharedComponent.ExtendedAudio
{
    public static class ToneGenerator
    {
        public static byte[] GenerateTriangleWave(float frequency, double duration, int sampleRate = 44100, short amplitude = 5000)
        {
            int numSamples = (int)(sampleRate * duration);
            int numBytes = numSamples * 2; // 16-bit samples
            byte[] wave = new byte[numBytes];

            double period = sampleRate / frequency;
            double halfPeriod = period / 2.0;
            //double angle = 0; // Removed unused variable

            for (int i = 0; i < numSamples; i++)
            {
                double t = i % period;
                short sample;

                if (t < halfPeriod)
                {
                    sample = (short)(amplitude * (t / halfPeriod));
                }
                else
                {
                    sample = (short)(amplitude * (1.0 - ((t - halfPeriod) / halfPeriod)));
                }

                byte[] sampleBytes = BitConverter.GetBytes(sample);
                wave[i * 2] = sampleBytes[0];
                wave[i * 2 + 1] = sampleBytes[1];
            }

            return wave;
        }
    }
}
