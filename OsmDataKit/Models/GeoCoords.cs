using System;
using System.Diagnostics;

namespace OsmDataKit
{
    public class GeoCoords : IGeoCoords
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public GeoCoords(double latitude, double longitude)
        {
            Debug.Assert(latitude >= -90 && latitude <= 90);
            Debug.Assert(longitude >= -180 && longitude <= 180);

            if (latitude < -90 || latitude > 90)
                throw new ArgumentOutOfRangeException(nameof(latitude));

            if (longitude < -180 || longitude > 180)
                throw new ArgumentOutOfRangeException(nameof(longitude));

            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
