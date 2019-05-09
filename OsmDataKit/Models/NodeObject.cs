using OsmSharp;
using System.Collections.Generic;
using System.Diagnostics;

namespace OsmDataKit
{
    public class NodeObject : GeoObject, IGeoCoords
    {
        public override OsmGeoType Type => OsmGeoType.Node;

        public double Latitude { get; }
        public double Longitude { get; }

        public NodeObject(
            long id, double latitude, double longitude,
            IReadOnlyDictionary<string, string> tags = null,
            Dictionary<string, string> data = null)
            : base(id, tags, data)
        {
            Debug.Assert(latitude >= -90 && latitude <= 90);
            Debug.Assert(longitude >= -180 && longitude <= 180);

            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
