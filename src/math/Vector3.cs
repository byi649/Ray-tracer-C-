using System;

namespace RayTracer
{
    /// <summary>
    /// Immutable structure to represent a three-dimensional vector.
    /// </summary>
    public readonly struct Vector3
    {
        private readonly double x, y, z;

        /// <summary>
        /// Construct a three-dimensional vector.
        /// </summary>
        /// <param name="x">X component</param>
        /// <param name="y">Y component</param>
        /// <param name="z">Z component</param>
        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Convert vector to a readable string.
        /// </summary>
        /// <returns>Vector as string in form (x, y, z)</returns>
        public override string ToString()
        {
            return "(" + this.x + "," + this.y + "," + this.z + ")";
        }

        /// <summary>
        /// Compute the length of the vector squared.
        /// This should be used if there is a way to perform a vector
        /// computation without needing the actual length, since
        /// a square root operation is expensive.
        /// </summary>
        /// <returns>Length of the vector squared</returns>
        public double LengthSq()
        {
            double lengthsq = Math.Pow(this.x, 2)
                            + Math.Pow(this.y, 2)
                            + Math.Pow(this.z, 2);
            return lengthsq;
        }

        public static void LengthSqTest() {
            Vector3 v1 = new Vector3(0, 0, 0);
            Vector3 v2 = new Vector3(1, 2, 3);
            Vector3 v3 = new Vector3(-1, 2, -3);
            Console.WriteLine("LengthSq");
            Console.WriteLine("(0, 0, 0) == " + v1.LengthSq() + (v1.LengthSq() == 0 ? " PASS" : " FAIL"));
            Console.WriteLine("(1, 2, 3) == " + v2.LengthSq() + (v2.LengthSq() == 14 ? " PASS" : " FAIL"));
            Console.WriteLine("(-1, 2, -3) == " + v2.LengthSq() + (v3.LengthSq() == 14 ? " PASS" : " FAIL"));
        }

        /// <summary>
        /// Compute the length of the vector.
        /// </summary>
        /// <returns>Length of the vector</returns>
        public double Length()
        {
            return Math.Sqrt(this.LengthSq());
        }

        public static void LengthTest() {
            Vector3 v1 = new Vector3(0, 0, 0);
            Vector3 v2 = new Vector3(1, 2, 3);
            Vector3 v3 = new Vector3(-1, 2, -3);
            Console.WriteLine("Length");
            Console.WriteLine("(0, 0, 0) == " + v1.Length() + (v1.Length() == 0 ? " PASS" : " FAIL"));
            Console.WriteLine("(1, 2, 3) == " + v2.Length() + (v2.Length() == Math.Sqrt(14) ? " PASS" : " FAIL"));
            Console.WriteLine("(-1, 2, -3) == " + v2.Length() + (v3.Length() == Math.Sqrt(14) ? " PASS" : " FAIL"));
        }

        /// <summary>
        /// Compute a length 1 vector in the same direction.
        /// </summary>
        /// <returns>Normalized vector</returns>
        public Vector3 Normalized()
        {
            double length = this.Length();
            if (length == 0) {
                return new Vector3(0, 0, 0);
            }
            double newX = this.x / length;
            double newY = this.y / length;
            double newZ = this.z / length;
            return new Vector3(newX, newY, newZ);
        }

        public static void NormalizedTest() {
            Vector3 v1 = new Vector3(0, 0, 0);
            Vector3 v2 = new Vector3(1, 2, 3);
            Vector3 v3 = new Vector3(-1, 2, -3);
            Console.WriteLine("Normalized");
            Console.WriteLine("(0, 0, 0) == " + v1.Normalized()
                             + (( v1.Normalized().X == 0
                                & v1.Normalized().Y == 0
                                & v1.Normalized().Z == 0
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(1, 2, 3) == " + v2.Normalized()
                             + (( v2.Normalized().X == 1 / Math.Sqrt(14)
                                & v2.Normalized().Y == 2 / Math.Sqrt(14)
                                & v2.Normalized().Z == 3 / Math.Sqrt(14)
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(-1, 2, -3) == " + v3.Normalized()
                             + (( v3.Normalized().X == -1 / Math.Sqrt(14)
                                & v3.Normalized().Y == 2 / Math.Sqrt(14)
                                & v3.Normalized().Z == -3 / Math.Sqrt(14)
                                ) ? " PASS" : " FAIL"));
            }

        /// <summary>
        /// Compute the dot product with another vector.
        /// </summary>
        /// <param name="with">Vector to dot product with</param>
        /// <returns>Dot product result</returns>
        public double Dot(Vector3 with)
        {
            double dot = this.x * with.X
                       + this.y * with.Y
                       + this.z * with.Z;
            return dot;
        }

        public static void DotTest() {
            Vector3 v1 = new Vector3(0, 0, 0);
            Vector3 v2 = new Vector3(1, 2, 3);
            Vector3 v3 = new Vector3(-1, 2, -3);
            Console.WriteLine("Dot");
            Console.WriteLine("(0, 0, 0) . (1, 2, 3) == " + v1.Dot(v2) + (v1.Dot(v2) == 0 ? " PASS" : " FAIL"));
            Console.WriteLine("(1, 2, 3) . (-1, 2, -3) == " + v2.Dot(v3) + (v2.Dot(v3) == -6 ? " PASS" : " FAIL"));
            Console.WriteLine("(-1, 2, -3) . (1, 2, 3) == " + v3.Dot(v2) + (v3.Dot(v2) == -6 ? " PASS" : " FAIL"));
        }

        /// <summary>
        /// Compute the cross product with another vector.
        /// </summary>
        /// <param name="with">Vector to cross product with</param>
        /// <returns>Cross product result</returns>
        public Vector3 Cross(Vector3 with)
        {
            double crossX = this.y * with.Z - this.z * with.Y;
            double crossY = this.z * with.X - this.x * with.Z;
            double crossZ = this.x * with.Y - this.y * with.X;
            return new Vector3(crossX, crossY, crossZ);
        }

        public static void CrossTest() {
            Vector3 v1 = new Vector3(0, 0, 0);
            Vector3 v2 = new Vector3(1, 2, 3);
            Vector3 v3 = new Vector3(-1, 2, -3);
            Console.WriteLine("Cross");
            Console.WriteLine("(0, 0, 0) x (1, 2, 3) == " + v1.Cross(v2)
                             + (( v1.Cross(v2).X == 0
                                & v1.Cross(v2).Y == 0
                                & v1.Cross(v2).Z == 0
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(1, 2, 3) x (-1, 2, -3) == " + v2.Cross(v3)
                             + (( v2.Cross(v3).X == -12
                                & v2.Cross(v3).Y == 0
                                & v2.Cross(v3).Z == 4
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(-1, 2, -3) x (1, 2, 3) == " + v3.Cross(v2)
                             + (( v3.Cross(v2).X == 12
                                & v3.Cross(v2).Y == 0
                                & v3.Cross(v2).Z == -4
                                ) ? " PASS" : " FAIL"));
        }

        /// <summary>
        /// Sum two vectors together (using + operator).
        /// </summary>
        /// <param name="a">First vector</param>
        /// <param name="b">Second vector</param>
        /// <returns>Summed vector</returns>
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            double newX = a.X + b.X;
            double newY = a.Y + b.Y;
            double newZ = a.Z + b.Z;
            return new Vector3(newX, newY, newZ);
        }

        public static void AddTest() {
            Vector3 v1 = new Vector3(0, 0, 0);
            Vector3 v2 = new Vector3(1, 2, 3);
            Vector3 v3 = new Vector3(-1, 2, -3);
            Console.WriteLine("Add");
            Console.WriteLine("(0, 0, 0) + (1, 2, 3) == " + (v1 + v2)
                             + (( (v1 + v2).X == 1
                                & (v1 + v2).Y == 2
                                & (v1 + v2).Z == 3
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(1, 2, 3) + (-1, 2, -3) == " + (v2 + v3)
                             + (( (v2 + v3).X == 0
                                & (v2 + v3).Y == 4
                                & (v2 + v3).Z == 0
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(-1, 2, -3) + (1, 2, 3) == " + (v3 + v2)
                             + (( (v3 + v2).X == 0
                                & (v3 + v2).Y == 4
                                & (v3 + v2).Z == 0
                                ) ? " PASS" : " FAIL"));
        }

        /// <summary>
        /// Negate a vector (using - operator)
        /// </summary>
        /// <param name="a">Vector to negate</param>
        /// <returns>Negated vector</returns>
        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a.X, -a.Y, -a.Z);
        }

        public static void NegTest() {
            Vector3 v1 = new Vector3(0, 0, 0);
            Vector3 v2 = new Vector3(1, 2, 3);
            Vector3 v3 = new Vector3(-1, 2, -3);
            Console.WriteLine("Neg");
            Console.WriteLine("-(0, 0, 0) == " + (-v1)
                             + (( (-v1).X == 0
                                & (-v1).Y == 0
                                & (-v1).Z == 0
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("-(1, 2, 3) == " + (-v2)
                             + (( (-v2).X == -1
                                & (-v2).Y == -2
                                & (-v2).Z == -3
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("-(-1, 2, -3) == " + (-v3)
                             + (( (-v3).X == 1
                                & (-v3).Y == -2
                                & (-v3).Z == 3
                                ) ? " PASS" : " FAIL"));
        }

        /// <summary>
        /// Subtract one vector from another.
        /// </summary>
        /// <param name="a">Original vector</param>
        /// <param name="b">Vector to subtract</param>
        /// <returns>Subtracted vector</returns>
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            double newX = a.X - b.X;
            double newY = a.Y - b.Y;
            double newZ = a.Z - b.Z;
            return new Vector3(newX, newY, newZ);
        }

        
        public static void SubTest() {
            Vector3 v1 = new Vector3(0, 0, 0);
            Vector3 v2 = new Vector3(1, 2, 3);
            Vector3 v3 = new Vector3(-1, 2, -3);
            Console.WriteLine("Sub");
            Console.WriteLine("(0, 0, 0) - (1, 2, 3) == " + (v1 - v2)
                             + (( (v1 - v2).X == -1
                                & (v1 - v2).Y == -2
                                & (v1 - v2).Z == -3
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(1, 2, 3) - (-1, 2, -3) == " + (v2 - v3)
                             + (( (v2 - v3).X == 2
                                & (v2 - v3).Y == 0
                                & (v2 - v3).Z == 6
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(-1, 2, -3) - (1, 2, 3) == " + (v3 - v2)
                             + (( (v3 - v2).X == -2
                                & (v3 - v2).Y == 0
                                & (v3 - v2).Z == -6
                                ) ? " PASS" : " FAIL"));
        }

        /// <summary>
        /// Multiply a vector by a scalar value.
        /// </summary>
        /// <param name="a">Original vector</param>
        /// <param name="b">Scalar multiplier</param>
        /// <returns>Multiplied vector</returns>
        public static Vector3 operator *(Vector3 a, double b)
        {
            double newX = a.X * b;
            double newY = a.Y * b;
            double newZ = a.Z * b;
            return new Vector3(newX, newY, newZ);
        }

        public static void MulTest() {
            Vector3 v1 = new Vector3(0, 0, 0);
            Vector3 v2 = new Vector3(1, 2, 3);
            Vector3 v3 = new Vector3(-1, 2, -3);
            Console.WriteLine("Mul");
            Console.WriteLine("(0, 0, 0) * 5 == " + (v1 * 5)
                             + (( (v1 * 5).X == 0
                                & (v1 * 5).Y == 0
                                & (v1 * 5).Z == 0
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(1, 2, 3) * 5 == " + (v2 * 5)
                             + (( (v2 * 5).X == 5
                                & (v2 * 5).Y == 10
                                & (v2 * 5).Z == 15
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(-1, 2, -3) * -2.5 == " + (v3 * -2.5)
                             + (( (v3 * -2.5).X == 2.5
                                & (v3 * -2.5).Y == -5
                                & (v3 * -2.5).Z == 7.5
                                ) ? " PASS" : " FAIL"));
        }

        /// <summary>
        /// Multiply a vector by a scalar value (opposite operands).
        /// </summary>
        /// <param name="b">Scalar multiplier</param>
        /// <param name="a">Original vector</param>
        /// <returns>Multiplied vector</returns>
        public static Vector3 operator *(double b, Vector3 a)
        {
            double newX = a.X * b;
            double newY = a.Y * b;
            double newZ = a.Z * b;
            return new Vector3(newX, newY, newZ);
        }

        /// <summary>
        /// Divide a vector by a scalar value.
        /// </summary>
        /// <param name="a">Original vector</param>
        /// <param name="b">Scalar divisor</param>
        /// <returns>Divided vector</returns>
        public static Vector3 operator /(Vector3 a, double b)
        {
            if (b == 0) {
                return new Vector3(double.NaN, double.NaN, double.NaN);
            }
            double newX = a.X / b;
            double newY = a.Y / b;
            double newZ = a.Z / b;
            return new Vector3(newX, newY, newZ);
        }

        public static void DivTest() {
            Vector3 v1 = new Vector3(0, 0, 0);
            Vector3 v2 = new Vector3(1, 2, 3);
            Vector3 v3 = new Vector3(-1, 2, -3);
            Console.WriteLine("Div");
            Console.WriteLine("(0, 0, 0) / 0 == " + (v1 / 0.0)
                             + (( double.IsNaN((v1 / 0).X)
                                & double.IsNaN((v1 / 0).Y)
                                & double.IsNaN((v1 / 0).Z)
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(1, 2, 3) / 5 == " + (v2 / 5.0)
                             + (( (v2 / 5).X == 1 / 5.0
                                & (v2 / 5).Y == 2 / 5.0
                                & (v2 / 5).Z == 3 / 5.0
                                ) ? " PASS" : " FAIL"));
            Console.WriteLine("(-1, 2, -3) / -2.5 == " + (v3 / -2.5)
                             + (( (v3 / -2.5).X == -1 / -2.5
                                & (v3 / -2.5).Y == 2 / -2.5
                                & (v3 / -2.5).Z == -3 / -2.5
                                ) ? " PASS" : " FAIL"));
        }

        /// <summary>
        /// X component of the vector.
        /// </summary>
        public double X { get { return this.x; } }

        /// <summary>
        /// Y component of the vector.
        /// </summary>
        public double Y { get { return this.y; } }

        /// <summary>
        /// Z component of the vector.
        /// </summary>
        public double Z { get { return this.z; } }
    }
}
