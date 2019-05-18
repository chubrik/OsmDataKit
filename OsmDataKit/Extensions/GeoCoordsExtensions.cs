using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OsmDataKit
{
    public static class GeoPointExtensions
    {
        public static GeoPoint CenterPoint(this IEnumerable<IGeoPoint> points)
        {
            Debug.Assert(points != null);

            if (points == null)
                throw new ArgumentNullException(nameof(points));

            var minLat = float.NaN;
            var maxLat = float.NaN;
            var minLng = float.NaN;
            var maxLng = float.NaN;

            foreach (var point in points)
            {
                if (float.IsNaN(minLat))
                {
                    minLat = maxLat = point.Latitude;
                    minLng = maxLng = point.Longitude;
                    continue;
                }

                if (minLat > point.Latitude)
                    minLat = point.Latitude;
                else
                if (maxLat < point.Latitude)
                    maxLat = point.Latitude;

                if (minLng > point.Longitude)
                    minLng = point.Longitude;
                else
                if (maxLng < point.Longitude)
                    maxLng = point.Longitude;
            }

            if (float.IsNaN(minLat))
                throw new ArgumentException(nameof(points));

            return new GeoPoint((minLat + maxLat) / 2, (minLng + maxLng) / 2);
        }
    }
}
