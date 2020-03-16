using System;
using System.Runtime.CompilerServices;

namespace OsmDataKit
{
    public struct Location
    {
        public float Latitude { get; }

        public float Longitude { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Location(double latitude, double longitude)
            : this((float)latitude, (float)longitude) { }

        public Location(float latitude, float longitude)
        {
            if (latitude >= -90 && latitude <= 90)
                Latitude = latitude;
            else
                throw new ArgumentOutOfRangeException(nameof(latitude));

            if (longitude >= -180 && longitude <= 180)
                Longitude = longitude;
            else
                throw new ArgumentOutOfRangeException(nameof(longitude));
        }

        #region Equals

        public override bool Equals(object obj) =>
            obj is Location objLocation &&
            Latitude == objLocation.Latitude &&
            Longitude == objLocation.Longitude;

        public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

        public static bool operator ==(Location left, Location right) => left.Equals(right);

        public static bool operator !=(Location left, Location right) => left.Equals(right);

        #endregion
    }
}
