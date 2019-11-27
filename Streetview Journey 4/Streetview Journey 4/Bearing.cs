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

        public Bearing() : this(0) { }

        public double Value;

        public override string ToString() => Value.ToString();

        public static implicit operator string(Bearing brng) => brng.ToString();

        public static Bearing operator -(Bearing left, Bearing right) =>
            new Bearing(Math.Abs((left.Value - right.Value + 540) % 360 - 180));

        public static Bearing operator +(Bearing left, Bearing right) =>
            new Bearing((left.Value + right.Value + 360) % 360);

        public static Bearing Average(Bearing[] bearings) =>
            new Bearing((bearings.Sum(b => b.Value) % 360) / bearings.Length);

        public static Bearing Average(Point[] points) =>
            new Bearing((points.Sum(b => b.Bearing.Value) % 360) / points.Length);

        public Bearing CalculateOffset(Bearing desired) =>
            new Bearing((desired.Value + 360 - Value) % 360);
    }
}
