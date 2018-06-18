using System;
using System.Diagnostics;

namespace Kit.Osm
{
    public class GeoPoint : GeoObject
    {
        public float Latitude { get; }
        public float Longitude { get; }

        internal GeoPoint(OsmNodeData data) : base(data)
        {
            Debug.Assert(data.Coords.Length == 2);

            if (data.Coords.Length != 2)
                throw new ArgumentNullException(nameof(data));

            Type = GeoType.Point;
            Latitude = data.Coords[0];
            Longitude = data.Coords[1];
        }

        public override bool IsBroken() => false;

        public override GeoCoords AverageCoords() => new GeoCoords(Latitude, Longitude);
    }
}
