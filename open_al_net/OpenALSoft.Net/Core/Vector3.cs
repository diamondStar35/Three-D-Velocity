using System;

namespace OpenALSoft.Net.Core
{
    /// <summary>
    /// Represents a vector with 3 floating-point values.
    /// </summary>
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"<{X}, {Y}, {Z}>";
        }
    }
}
