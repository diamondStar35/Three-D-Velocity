using System;

namespace OpenALSoft.Net.Core
{
    /// <summary>
    /// Represents a vector with 2 floating-point values.
    /// </summary>
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"<{X}, {Y}>";
        }
    }
}
