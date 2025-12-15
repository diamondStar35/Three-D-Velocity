using System;
using OpenALSoft.Net.Native;

namespace OpenALSoft.Net.Core
{
    /// <summary>
    /// Represents the OpenAL listener.
    /// </summary>
    public static class AudioListener
    {
        /// <summary>
        /// Gets or sets the position of the listener in 3D space.
        /// </summary>
        public static Vector3 Position
        {
            get
            {
                AL.GetListener3f(AL.POSITION, out float x, out float y, out float z);
                return new Vector3(x, y, z);
            }
            set
            {
                AL.Listener3f(AL.POSITION, value.X, value.Y, value.Z);
            }
        }

        /// <summary>
        /// Gets or sets the velocity of the listener in 3D space.
        /// </summary>
        public static Vector3 Velocity
        {
            get
            {
                AL.GetListener3f(AL.VELOCITY, out float x, out float y, out float z);
                return new Vector3(x, y, z);
            }
            set
            {
                AL.Listener3f(AL.VELOCITY, value.X, value.Y, value.Z);
            }
        }

        /// <summary>
        /// Gets or sets the orientation of the listener.
        /// The first vector is the "at" vector (forward), and the second is the "up" vector.
        /// </summary>
        public static (Vector3 At, Vector3 Up) Orientation
        {
            get
            {
                float[] values = new float[6];
                AL.GetListenerfv(AL.ORIENTATION, values);
                return (new Vector3(values[0], values[1], values[2]), new Vector3(values[3], values[4], values[5]));
            }
            set
            {
                float[] values = new float[] 
                { 
                    value.At.X, value.At.Y, value.At.Z, 
                    value.Up.X, value.Up.Y, value.Up.Z 
                };
                AL.Listenerfv(AL.ORIENTATION, values);
            }
        }

        /// <summary>
        /// Gets or sets the master gain (volume) of the listener.
        /// </summary>
        public static float Gain
        {
            get
            {
                AL.GetListenerf(AL.GAIN, out float value);
                return value;
            }
            set
            {
                AL.Listenerf(AL.GAIN, value);
            }
        }
    }
}
