﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetview_Journey_3
{
    class Smart
    {
        public enum Type { Drive, Hike }
        public static void ToSVJ(string path, Type type, int searchRadius = 50)
        {
            var locData = Import.Auto(path);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            Export.ToSVJ(locData, path.Replace(".gpx", ".svj"));
        }

        public static void ToSVJ(string path, Type type, int trimTo, bool maintainSpeed, bool keepDupes, int searchRadius = 50)
        {
            var locData = Import.Auto(path);
            double distance = type == Type.Drive ? 5 : 1;
            if (Get.AverageDistance(locData) > distance)
                locData = Modify.Interpolate(locData, distance, searchRadius);
            if (maintainSpeed || trimTo > locData.Length)
                locData = Modify.SmoothTrim(locData, trimTo);
            else
                locData = Modify.Trim(locData, trimTo);
            if (keepDupes == false)
                locData = Remove.Dupes(locData);
            Export.ToSVJ(locData, path.Replace(".gpx", ".svj"));
        }
    }
}