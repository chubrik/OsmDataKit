using System;
using System.Diagnostics;

namespace OsmDataKit
{
    public class GeoPoint : IGeoPoint
    {
        public float Latitude { get; }
        public float Longitude { get; }

        public GeoPoint(double latitude, double longitude) 
            : this((float)latitude, (float)longitude) { }

        public GeoPoint(float latitude, float longitude)
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