using System;
using System.Runtime.Serialization;

namespace SeemoPredictor.Geometry
{
    
    /// <summary>
    /// Representation of 3D points and vectors.
    /// </summary>
    /// <remarks>
    /// This class was inspired by the Vector3 type of the Unity Engine and 
    /// designed with the exact same interface to provide maximum compatibility.
    /// </remarks>
[DataContract]
    public struct Point3
    {
        /// <summary>
        /// Gets or sets the X coordinate.
        /// </summary>
        [DataMember]
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate.
        /// </summary>
        [DataMember]
        public float Y { get; set; }

        /// <summary>
        /// Gets or sets the Z coordinate.
        /// </summary>
        [DataMember]
        public float Z { get; set; }

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public float Length
        {
            get { return (float)Math.Sqrt(X*X + Y*Y + Z*Z); }
        }

        /// <summary>
        /// Gets the squared length of the vector.
        /// </summary>
        public float SqrMagnitude
        {
            get { return X*X + Y*Y + Z* Z; }    
        }

        /// <summary>
        /// Gets the vector with a magnitude of 1.
        /// </summary>
        public Point3 Normalized
        {
            get
            {
                Point3 copy = this;
                copy.Normalize();
                return copy;
            }
        }

        /// <summary>
        /// Creates a new vector with given coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public Point3(float x, float y, float z = 0f)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3(double x, double y, double z = 0f)
        {
            X = (float)x;
            Y = (float)y;
            Z = (float)z;
        }

        /// <summary>
        /// Normalizes the vector with a magnitude of 1.
        /// </summary>
        public void Normalize()
        {
            float num = Length;
            if(num > 1E-05f)
            {
                this /= num;
            }
            else
            {
                this = Point3.Zero;
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
        /// Determines whether the specified object as a <see cref="Point3" /> is exactly equal to this instance.
        /// </summary>
        /// <remarks>
        /// Due to floating point inaccuracies, this might return false for vectors which are essentially (but not exactly) equal. Use the <see cref="op_Equality"/> to test two points for approximate equality.
        /// </remarks>
        /// <param name="other">The <see cref="Point3" /> object to compare with this instance.</param>
        /// <returns><c>true</c> if the specified point is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object other)
        {
            bool result;
            if (!(other is Point3))
            {
                result = false;
            }
            else
            {
                Point3 vector = (Point3)other;
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
        public static Point3 Zero = new Point3(0f, 0f, 0f);

        /// <summary>
        /// Shorthand for writing Point(1, 1, 1).
        /// </summary>
        public static Point3 One = new Point3(1f, 1f, 1f);

        /// <summary>
        /// Shorthand for writing Point(0, 0, 1).
        /// </summary>
        public static Point3 Forward = new Point3(0f, 0f, 1f);

        /// <summary>
        /// Shorthand for writing Point(0, 0, -1).
        /// </summary>
        public static Point3 Back = new Point3(0f, 0f, -1f);

        /// <summary>
        /// Shorthand for writing Point(0, 1, 0).
        /// </summary>
        public static Point3 Up = new Point3(0f, 1f, 0f);

        /// <summary>
        /// Shorthand for writing Point(0, -1, 0).
        /// </summary>
        public static Point3 Down = new Point3(0f, -1f, 0f);

        /// <summary>
        /// Shorthand for writing Point(-1, 0, 0).
        /// </summary>
        public static Point3 Left = new Point3(-1f, 0f, 0f);

        /// <summary>
        /// Shorthand for writing Point(1, 0, 0).
        /// </summary>
        public static Point3 Right = new Point3(1f, 0f, 0f);



        public static Point3 XAxis = new Point3(1f, 0f, 0f);
        public static Point3 YAxis = new Point3(0f, 1f, 0f);
        public static Point3 ZAxis = new Point3(0f, 0f, 1f);



        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The distance.</returns>
        public static float Distance(Point3 a, Point3 b)
        {
            Point3 vector = new Point3(a.X - b.X, a.Y -b.Y, a.Z - b.Z);
            return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }

        /// <summary>
        /// Multiplies two vectors component-wise.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The scaled up vector.</returns>
        public static Point3 Scale(Point3 a, Point3 b)
        {
            return new Point3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        /// <summary>
        /// Cross-product of two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The cross product vector.</returns>
        public static Point3 Cross(Point3 a, Point3 b)
        {
            return new Point3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        /// <summary>
        /// Dot-product of two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>The dot product vector.</returns>
        public static float Dot(Point3 a, Point3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        /// <summary>
        /// Returns a point that is made from the smallest components of two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The minimal coordinates.</returns>
        public static Point3 Min(Point3 a, Point3 b)
        {
            return new Point3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
        }

        /// <summary>
        /// Returns a point that is made from the largest components of two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The maximal coordinates.</returns>
        public static Point3 Max(Point3 a, Point3 b)
        {
            return new Point3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        }

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static Point3 operator +(Point3 a, Point3 b)
        {
            return new Point3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static Point3 operator -(Point3 a, Point3 b)
        {
            return new Point3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        /// <param name="a">The vector.</param>
        public static Point3 operator -(Point3 a)
        {
            return new Point3(-a.X, -a.Y, -a.Z);
        }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The number.</param>
        public static Point3 operator *(Point3 a, float d)
        {
            return new Point3(a.X * d, a.Y * d, a.Z * d);
        }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="d">The number.</param>
        /// <param name="a">The vector.</param>
        public static Point3 operator *(float d, Point3 a)
        {
            return new Point3(a.X * d, a.Y * d, a.Z * d);
        }

        /// <summary>
        /// Divides a vector by a number.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The number.</param>
        public static Point3 operator /(Point3 a, float d)
        {
            return new Point3(a.X / d, a.Y / d, a.Z / d);
        }

        /// <summary>
        /// Determines whether two points are approximately equal.
        /// </summary>
        /// <remarks>
        /// To allow for floating point inaccuracies, the two vectors are considered equal if the magnitude of their difference is less than 1e-5..
        /// </remarks>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        public static bool operator ==(Point3 a, Point3 b)
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
        public static bool operator !=(Point3 a, Point3 b)
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
        public static Point3 Rotate(Point3 v, Point3 k, float theta)
        {
            float ct = (float)Math.Cos(theta);
            float st = (float)Math.Sin(theta);
            var khat = k;
            khat.Normalize();
            var vrot = v * ct + Point3.Cross(khat, v) * st + khat * Point3.Dot(khat, v) * (1.0f - ct);
            return vrot;
        }

        public static double AngleTheta(Point3 v, Point3 k)
        {

            var dot = Dot(v, k);
            var lengthV = v.Length;
            var lengthK = k.Length;

            var cos = (double)(dot / (lengthV * lengthK));
            if (cos > 1) cos = 1;
            if (cos < -1) cos = -1;

            double theta = Math.Acos(cos);

            return theta;
        }

        public static double AngleDegree(Point3 v, Point3 k)
        {
            
            double theta = AngleTheta(v, k);
            double degree = (theta * 180 / (Math.PI));

            return degree;
        }


        // added functions from GPU tracer

        public static Point3 Reflect(Point3 normal, Point3 incomming)
        {
            Point3 reflect = (incomming - normal * 2f * Dot(incomming, normal));
            return reflect.Normalized;
        }
    }
}
