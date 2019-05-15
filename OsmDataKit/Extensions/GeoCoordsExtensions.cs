using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OsmDataKit
{
    public static class GeoCoordsExtensions
    {
        public static GeoCoords AverageCoords(this IEnumerable<IGeoCoords> coordsColletion)
        {
            Debug.Assert(coordsColletion != null);

            if (coordsColletion == null)
                throw new ArgumentNullException(nameof(coordsColletion));

            var minLat = float.NaN;
            var maxLat = float.NaN;
            var minLng = float.NaN;
            var maxLng = float.NaN;

            foreach (var coords in coordsColletion)
            {
                if (float.IsNaN(minLat))
                {
                    minLat = maxLat = coords.Latitude;
                    minLng = maxLng = coords.Longitude;
                    continue;
                }

                if (minLat > coords.Latitude)
                    minLat = coords.Latitude;
                else
                if (maxLat < coords.Latitude)
                    maxLat = coords.Latitude;

                if (minLng > coords.Longitude)
                    minLng = coords.Longitude;
                else
                if (maxLng < coords.Longitude)
                    maxLng = coords.Longitude;
            }

            if (float.IsNaN(minLat))
                throw new ArgumentException(nameof(coordsColletion));

            return new GeoCoords((minLat + maxLat) / 2, (minLng + maxLng) / 2);
        }
    }
}
