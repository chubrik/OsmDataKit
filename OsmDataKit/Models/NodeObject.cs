using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OsmDataKit
{
    public class NodeObject : GeoObject, IGeoCoords
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public override OsmGeoType Type => OsmGeoType.Node;
        public override bool IsBroken => false;
        public override IGeoCoords AverageCoords => this;

        public NodeObject(
            long id, double latitude, double longitude,
            IReadOnlyDictionary<string, string> tags = null,
            Dictionary<string, string> data = null)
            : base(id, tags, data)
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
