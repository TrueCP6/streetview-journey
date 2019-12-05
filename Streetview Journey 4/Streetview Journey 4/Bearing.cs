using System;
using System.Linq;

namespace StreetviewJourney
{
    public class Bearing
    {
        public Bearing(double value)
        {
            Value = (value + 360) % 360;
        }

        /// <summary>
        /// Creates a bearing with a value of 0
        /// </summary>
        public Bearing() : this(0) { }

        /// <summary>
        /// The bearing value ranging from 0 to 360
        /// </summary>
        public double Value;

        public override string ToString() => Value.ToString();

        public static implicit operator string(Bearing brng) => brng.ToString();

        /// <summary>
        /// Calculates the shortest bearing difference between two bearings
        /// </summary>
        public static Bearing operator -(Bearing left, Bearing right) =>
            new Bearing(Math.Abs((left.Value - right.Value + 540) % 360 - 180));

        /// <summary>
        /// Adds two bearings that results in a bearing
        /// </summary>
        public static Bearing operator +(Bearing left, Bearing right) =>
            new Bearing((left.Value + right.Value + 360) % 360);

        /// <summary>
        /// Gets the average bearing from a number of bearings
        /// </summary>
        /// <param name="bearings">The array of bearings</param>
        /// <returns>The average bearing</returns>
        public static Bearing Average(Bearing[] bearings) =>
            new Bearing((bearings.Sum(b => b.Value) % 360) / bearings.Length);

        /// <summary>
        /// Gets the average bearing from a number of bearings
        /// </summary>
        /// <param name="points">The array of points each containing bearings</param>
        /// <returns>The average bearing</returns>
        public static Bearing Average(Point[] points) =>
            new Bearing((points.Sum(b => b.Bearing.Value) % 360) / points.Length);

        public Bearing CalculateOffset(Bearing desired) =>
            new Bearing((desired.Value + 360 - Value) % 360);
    }
}
