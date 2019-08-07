using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetview_Journey_3
{
    class Calculate
    {
        public const double radius = 6371000;

        /// <summary>
        /// Calculates an intermediate point a fraction along a great circle path between 2 points.
        /// </summary>
        /// <param name="StartPoint">A latitude-longitude point.</param>
        /// <param name="EndPoint">A latitude-longitude point.</param>
        /// <param name="fraction">A number from 0 to 1 as a fraction of the distance along the way to interpolate.</param>
        /// <returns>A latitude-longitude point.</returns>
        public static (double Lat, double Lon) IntermediatePoint((double Lat, double Lon) StartPoint, (double Lat, double Lon) EndPoint, double fraction)
        {
            if (fraction < 0 || fraction > 1)
                throw new ArgumentOutOfRangeException();
            double angDist = Distance(StartPoint, EndPoint) / radius;
            double lat1 = StartPoint.Lat * (Math.PI / 180);
            double lon1 = StartPoint.Lon * (Math.PI / 180);
            double lat2 = EndPoint.Lat * (Math.PI / 180);
            double lon2 = EndPoint.Lon * (Math.PI / 180);
            double a = Math.Sin((1 - fraction) * angDist) / Math.Sin(angDist);
            double b = Math.Sin(fraction * angDist) / Math.Sin(angDist);
            double x = a * Math.Cos(lat1) * Math.Cos(lon1) + b * Math.Cos(lat2) * Math.Cos(lon2);
            double y = a * Math.Cos(lat1) * Math.Sin(lon1) + b * Math.Cos(lat2) * Math.Sin(lon2);
            double z = a * Math.Sin(lat1) + b * Math.Sin(lat2);
            double lat3 = Math.Atan2(z, Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)));
            double lon3 = Math.Atan2(y, x);
            return (lat3 * (180 / Math.PI), lon3 * (180 / Math.PI));
        }

        /// <summary>
        /// Calculates a destination point given a start point, distance and bearing.
        /// </summary>
        /// <param name="startPoint">A latitude-longitude point.</param>
        /// <param name="distance">A distance in meters.</param>
        /// <param name="bearing">A bearing value from 0 to 360.</param>
        /// <returns>A latitude-longitude point.</returns>
        public static (double Lat, double Lon) Destination((double Lat, double Lon) startPoint, double distance, double bearing)
        {
            double φ1 = startPoint.Lat * (Math.PI / 180);
            double λ1 = startPoint.Lon * (Math.PI / 180);
            double brng = bearing * (Math.PI / 180);
            double φ2 = Math.Asin(Math.Sin(φ1) * Math.Cos(distance / radius) + Math.Cos(φ1) * Math.Sin(distance / radius) * Math.Cos(brng));
            double λ2 = λ1 + Math.Atan2(Math.Sin(brng) * Math.Sin(distance / radius) * Math.Cos(φ1), Math.Cos(distance / radius) - Math.Sin(φ1) * Math.Sin(φ2));
            return (φ2 * (180 / Math.PI), λ2 * (180 / Math.PI));
        }

        /// <summary>
        /// Calculates the closest difference between 2 bearing values.
        /// </summary>
        /// <param name="initialBearing">A bearing value from 0 to 360.</param>
        /// <param name="finalBearing">A bearing value from 0 to 360.</param>
        /// <returns>The closest difference between 2 bearing values.</returns>
        public static double BearingDifference(double initialBearing, double finalBearing)
        {
            return Math.Abs((finalBearing - initialBearing + 540) % 360 - 180);
        }

        /// <summary>
        /// Calculates the offset angle for <c>Bearing.OffsetPanorama</c>.
        /// </summary>
        /// <param name="initialBearing">A bearing value from 0 to 360.</param>
        /// <param name="desiredBearing">A bearing value from 0 to 360.</param>
        /// <returns>The calculated offset angle for use with <c>Bearing.OffsetPanorama</c>.</returns>
        public static double Offset(double initialBearing, double desiredBearing)
        {
            return ((desiredBearing + 360.0) - initialBearing) % 360.0;
        }

        /// <summary>
        /// Converts the Static Streetview API zoom to field of view.
        /// </summary>
        /// <param name="zoom">Zoom level.</param>
        /// <returns>Field of view.</returns>
        public static double ZoomToFOV(double zoom)
        {
            return 180.0 / Math.Pow(2, zoom);
        }

        /// <summary>
        /// Converts field of view to the Static Streetview API zoom.
        /// </summary>
        /// <param name="fov">Field of view.</param>
        /// <returns>Streetview API zoom level.</returns>
        public static double FOVToZoom(double fov)
        {
            return Math.Log(180.0 / fov) / Math.Log(2);
        }

        /// <summary>
        /// Correctly calculates the average bearing of an array of bearings from 0 to 360.
        /// </summary>
        /// <param name="bearings">An array of bearing values from 0 to 360.</param>
        /// <returns>The average of an array of bearings.</returns>
        public static double AverageBearing(double[] bearings)
        {
            return (bearings.Sum() % 360) / Convert.ToDouble(bearings.Length);
        }

        /// <summary>
        /// Calculates the distance between 2 points in meters
        /// </summary>
        /// <param name="point1">A latitude-longitude point.</param>
        /// <param name="point2">A latitude-longitude point.</param>
        /// <returns>The distance between 2 points in meters.</returns>
        public static double Distance((double Lat, double Lon) point1, (double Lat, double Lon) point2)
        {
            double φ1 = point1.Lat * (Math.PI / 180.0);
            double φ2 = point2.Lat * (Math.PI / 180.0);
            double Δφ = (point2.Lat - point1.Lat) * (Math.PI / 180.0);
            double Δλ = (point2.Lon - point1.Lon) * (Math.PI / 180.0);
            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) + Math.Cos(φ1) * Math.Cos(φ2) * Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return radius * c;
        }

        /// <summary>
        /// Calculates the bearing from one point to another.
        /// </summary>
        /// <param name="point1">A latitude-longitude point.</param>
        /// <param name="point2">A latitude-longitude point.</param>
        /// <returns>A bearing value from 0 to 360.</returns>
        public static double Bearing((double Lat, double Lon) point1, (double Lat, double Lon) point2)
        {
            double lat1 = point1.Lat * (Math.PI / 180.0);
            double lon1 = point1.Lon * (Math.PI / 180.0);
            double lat2 = point2.Lat * (Math.PI / 180.0);
            double lon2 = point2.Lon * (Math.PI / 180.0);

            return (Math.Atan2(Math.Sin(lon2 - lon1) * Math.Cos(lat2), Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1)) * (180 / Math.PI) + 360) % 360;
        }

        /// <summary>
        /// Calculates the midpoint between 2 points.
        /// </summary>
        /// <param name="point1">A latitude-longitude point.</param>
        /// <param name="point2">A latitude-longitude point.</param>
        /// <returns>The midpoint between 2 points.</returns>
        public static (double Lat, double Lon) Midpoint((double Lat, double Lon) point1, (double Lat, double Lon) point2)
        {
            double lat1 = point1.Lat * (Math.PI / 180.0);
            double lon1 = point1.Lon * (Math.PI / 180.0);
            double lat2 = point2.Lat * (Math.PI / 180.0);
            double lon2 = point2.Lon * (Math.PI / 180.0);
            double Bx = Math.Cos(lat2) * Math.Cos(lon2 - lon1);
            double By = Math.Cos(lat2) * Math.Sin(lon2 - lon1);
            double lat3 = Math.Atan2(Math.Sin(lat1) + Math.Sin(lat2), Math.Sqrt((Math.Cos(lat1) + Bx) * (Math.Cos(lat1) + Bx) + By * By));
            double lon3 = lon1 + Math.Atan2(By, Math.Cos(lat1) + Bx);
            return (lat3 * (180.0 / Math.PI), lon3 * (180.0 / Math.PI));
        }

        /// <summary>
        /// Adds 2 bearing values together with wrap around.
        /// </summary>
        /// <param name="initialBearing">A bearing value from 0 to 360.</param>
        /// <param name="valueToAdd">A bearing value from 0 to 360.</param>
        /// <returns>A bearing value from 0 to 360.</returns>
        public static double AddBearing(double initialBearing, double valueToAdd) { return (initialBearing + valueToAdd + 360) % 360; }
    }
}
