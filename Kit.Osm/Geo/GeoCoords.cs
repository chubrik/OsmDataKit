using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kit.Osm
{
    public class GeoCoords
    {
        public float Latitude { get; protected set; }
        public float Longitude { get; protected set; }

        public GeoCoords(float latitude, float longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public GeoCoords(IReadOnlyList<float> coords)
        {
            Debug.Assert(coords.Count == 2);

            if (coords.Count != 2)
                throw new ArgumentException();

            Latitude = coords[0];
            Longitude = coords[1];
        }
    }
}
