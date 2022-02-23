using System;
using System.Runtime.Serialization;

namespace SeemoPredictor.SeemoGeo
{
    [DataContract]
    public struct SmoPoint3
    {
        [DataMember]
        public float X { get; set; }


        [DataMember]
        public float Y { get; set; }

        [DataMember]
        public float Z { get; set; }

        public float Length
        {
            get { return (float)Math.Sqrt(X*X + Y*Y + Z*Z); }
        }

        public float SqrMagnitude
        {
            get { return X*X + Y*Y + Z* Z; }    
        }

        /// Gets the vector with a magnitude of 1.
        public SmoPoint3 Normalized
        {
            get
            {
                SmoPoint3 copy = this;
                copy.Normalize();
                return copy;
            }
        }

        /// Creates a new vector with given coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public SmoPoint3(float x, float y, float z = 0f)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public SmoPoint3(double x, double y, double z = 0f)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
        }

        /// Normalizes the vector with a magnitude of 1.
        public void Normalize()
        {
            float num = Length;
            if(num > 1E-05f)
            {
                this /= num;
            }
            else
            {
                this = SmoPoint3.Zero;
            }
        }


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2;
        }

        /// <summary>
        /// Determines whether the specified object as a <see cref="SmoPoint3" /> is exactly equal to this instance.
        /// </summary>
        /// <remarks>
        /// Due to floating point inaccuracies, this might return false for vectors which are essentially (but not exactly) equal. Use the <see cref="op_Equality"/> to test two points for approximate equality.
        /// </remarks>
        /// <param name="other">The <see cref="SmoPoint3" /> object to compare with this instance.</param>
        /// <returns><c>true</c> if the specified point is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object other)
        {
            bool result;
            if (!(other is SmoPoint3))
            {
                result = false;
            }
            else
            {
                SmoPoint3 vector = (SmoPoint3)other;
                result = (X.Equals(vector.X) && Y.Equals(vector.Y) && Z.Equals(vector.Z));
            }
            return result;
        }

        /// <summary>
        /// Returns a nicely formatted string for this vector.
        /// </summary>
        public override string ToString()
        {
            return String.Format("({0:F1}, {1:F1}, {2:F1})", X, Y, Z);
        }

        /// <summary>
        /// Returns a nicely formatted string for this vector.
        /// </summary>
        /// <param name="format">The format for each coordinate.</param>
        public string ToString(string format)
        {
            return String.Format("({0}, {1}, {2})", X.ToString(format), Y.ToString(format), Z.ToString(format));
        }

        /// <summary>
        /// Shorthand for writing Point(0, 0, 0).
        /// </summary>
        public static SmoPoint3 Zero = new SmoPoint3(0f, 0f, 0f);

        /// <summary>
        /// Shorthand for writing Point(1, 1, 1).
        /// </summary>
        public static SmoPoint3 One = new SmoPoint3(1f, 1f, 1f);

        /// <summary>
        /// Shorthand for writing Point(0, 0, 1).
        /// </summary>
        public static SmoPoint3 Forward = new SmoPoint3(0f, 0f, 1f);

        /// <summary>
        /// Shorthand for writing Point(0, 0, -1).
        /// </summary>
        public static SmoPoint3 Back = new SmoPoint3(0f, 0f, -1f);

        /// <summary>
        /// Shorthand for writing Point(0, 1, 0).
        /// </summary>
        public static SmoPoint3 Up = new SmoPoint3(0f, 1f, 0f);

        /// <summary>
        /// Shorthand for writing Point(0, -1, 0).
        /// </summary>
        public static SmoPoint3 Down = new SmoPoint3(0f, -1f, 0f);

        /// <summary>
        /// Shorthand for writing Point(-1, 0, 0).
        /// </summary>
        public static SmoPoint3 Left = new SmoPoint3(-1f, 0f, 0f);

        /// <summary>
        /// Shorthand for writing Point(1, 0, 0).
        /// </summary>
        public static SmoPoint3 Right = new SmoPoint3(1f, 0f, 0f);



        public static SmoPoint3 XAxis = new SmoPoint3(1f, 0f, 0f);
        public static SmoPoint3 YAxis = new SmoPoint3(0f, 1f, 0f);
        public static SmoPoint3 ZAxis = new SmoPoint3(0f, 0f, 1f);


        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The distance.</returns>
        public static float Distance(SmoPoint3 a, SmoPoint3 b)
        {
            SmoPoint3 vector = new SmoPoint3(a.X - b.X, a.Y -b.Y, a.Z - b.Z);
            return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }

        /// <summary>
        /// Multiplies two vectors component-wise.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The scaled up vector.</returns>
        public static SmoPoint3 Scale(SmoPoint3 a, SmoPoint3 b)
        {
            return new SmoPoint3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        /// <summary>
        /// Cross-product of two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The cross product vector.</returns>
        public static SmoPoint3 Cross(SmoPoint3 a, SmoPoint3 b)
        {
            return new SmoPoint3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }


        /// <summary>
        /// Dot-product of two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The dot product vector.</returns>
        public static float Dot(SmoPoint3 a, SmoPoint3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }


        /// <summary>
        /// Returns a point that is made from the smallest components of two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The minimal coordinates.</returns>
        public static SmoPoint3 Min(SmoPoint3 a, SmoPoint3 b)
        {
            return new SmoPoint3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
        }

        /// <summary>
        /// Returns a point that is made from the largest components of two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The maximal coordinates.</returns>
        public static SmoPoint3 Max(SmoPoint3 a, SmoPoint3 b)
        {
            return new SmoPoint3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static SmoPoint3 operator +(SmoPoint3 a, SmoPoint3 b)
        {
            return new SmoPoint3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static SmoPoint3 operator -(SmoPoint3 a, SmoPoint3 b)
        {
            return new SmoPoint3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        /// <param name="a">The vector.</param>
        public static SmoPoint3 operator -(SmoPoint3 a)
        {
            return new SmoPoint3(-a.X, -a.Y, -a.Z);
        }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The number.</param>
        public static SmoPoint3 operator *(SmoPoint3 a, float d)
        {
            return new SmoPoint3(a.X * d, a.Y * d, a.Z * d);
        }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="d">The number.</param>
        /// <param name="a">The vector.</param>
        public static SmoPoint3 operator *(float d, SmoPoint3 a)
        {
            return new SmoPoint3(a.X * d, a.Y * d, a.Z * d);
        }

        /// <summary>
        /// Divides a vector by a number.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The number.</param>
        public static SmoPoint3 operator /(SmoPoint3 a, float d)
        {
            return new SmoPoint3(a.X / d, a.Y / d, a.Z / d);
        }

        /// <summary>
        /// Determines whether two points are approximately equal.
        /// </summary>
        /// <remarks>
        /// To allow for floating point inaccuracies, the two vectors are considered equal if the magnitude of their difference is less than 1e-5..
        /// </remarks>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        public static bool operator ==(SmoPoint3 a, SmoPoint3 b)
        {
            return (a - b).SqrMagnitude < 9.99999944E-11f;
        }

        /// <summary>
        /// Determines whether two points are different.
        /// </summary>
        /// <remarks>
        /// To allow for floating point inaccuracies, the two vectors are considered equal if the magnitude of their difference is less than 1e-5.
        /// </remarks>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        public static bool operator !=(SmoPoint3 a, SmoPoint3 b)
        {
            return !(a == b);
        }


        /// <summary>
        /// Rotates vector around axis using Rodrigues’ rotation formula
        /// </summary>
        /// <param name="v">vector to rotate</param>
        /// <param name="k">rotation axis</param>
        /// <param name="theta">theta rotation angle[rad]</param>
        /// <returns>rotated vector</returns>
        public static SmoPoint3 Rotate(SmoPoint3 v, SmoPoint3 k, float theta)
        {
            float ct = (float)Math.Cos(theta);
            float st = (float)Math.Sin(theta);
            var khat = k;
            khat.Normalize();
            var vrot = v * ct + SmoPoint3.Cross(khat, v) * st + khat * SmoPoint3.Dot(khat, v) * (1.0f - ct);
            return vrot;
        }


    }
}
