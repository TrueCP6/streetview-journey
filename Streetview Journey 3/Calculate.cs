using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetview_Journey_3
{
    class Calculate
    {
        public static double Offset(double initialBearing, double desiredBearing)
        {
            return ((desiredBearing + 360.0) - initialBearing) % 360.0;
        }

        public static double ZoomToFOV(double zoom)
        {
            return 180.0 / Math.Pow(2, zoom);
        }

        public static double FOVToZoom(double fov)
        {
            return Math.Log(180.0 / fov) / Math.Log(2);
        }

        public static double AverageBearing(double[] bearings)
        {
            return (bearings.Sum() % 360) / Convert.ToDouble(bearings.Length);
        }

        public static double Distance((double Lat, double Lon) point1, (double Lat, double Lon) point2)
        {
            double R = 6371000; //radius of earth in metres
            double φ1 = point1.Lat * (Math.PI / 180.0);
            double φ2 = point2.Lat * (Math.PI / 180.0);
            double Δφ = (point2.Lat - point1.Lat) * (Math.PI / 180.0);
            double Δλ = (point2.Lon - point1.Lon) * (Math.PI / 180.0);
            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) + Math.Cos(φ1) * Math.Cos(φ2) * Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        public static double Bearing((double Lat, double Lon) point1, (double Lat, double Lon) point2)
        {
            double lat1 = point1.Lat * (Math.PI / 180.0);
            double lon1 = point1.Lon * (Math.PI / 180.0);
            double lat2 = point2.Lat * (Math.PI / 180.0);
            double lon2 = point2.Lon * (Math.PI / 180.0);

            return (Math.Atan2(Math.Sin(lon2 - lon1) * Math.Cos(lat2), Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1)) * (180 / Math.PI) + 360) % 360;
        }

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

        public static double AddBearing(double initialBearing, double valueToAdd) { return (initialBearing + valueToAdd + 360) % 360; }
    }
}
