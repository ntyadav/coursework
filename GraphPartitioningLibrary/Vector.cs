using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphPartitioningLibrary
{
    /// <summary>
    /// Структура данных, позволяющая хранить вектор и производить с ним вычисления
    /// </summary>
    public class Vector
    {
        public double X, Y;
        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Vector a, Vector b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Vector a, Vector b) => a.X != b.X || a.Y != b.Y;
        public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y);
        public static Vector operator -(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y);
        public static Vector operator *(double k, Vector a) => new Vector(k * a.X, k * a.Y);
        public static Vector operator *(Vector a, double k) => new Vector(k * a.X, k * a.Y);
        public static Vector operator /(Vector a, double k) => new Vector(a.X / k, a.Y / k);
        public static Vector operator -(Vector a) => new Vector(-a.X, -a.Y);
        public double Abs() => Math.Sqrt(X * X + Y * Y);
    }
}
