using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kit.Osm
{
    public class GeoCoords
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public GeoCoords(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public GeoCoords(IReadOnlyList<double> coords)
        {
            Debug.Assert(coords.Count == 2);

            if (coords.Count != 2)
                throw new ArgumentException();

            Latitude = coords[0];
            Longitude = coords[1];
        }
    }
}
