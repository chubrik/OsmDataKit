using Newtonsoft.Json;
using OsmDataKit.Internal;
using OsmSharp;
using System.Collections.Generic;

namespace OsmDataKit
{
    [JsonConverter(typeof(NodeObjectConverter))]
    public class NodeObject : GeoObject
    {
        public override OsmGeoType Type => OsmGeoType.Node;

        public Location Location { get; }

        public float Latitude => Location.Latitude;

        public float Longitude => Location.Longitude;

        public NodeObject(
            long id, double latitude, double longitude, Dictionary<string, string> tags = null)
            : base(id, tags)
        {
            Location = new Location(latitude, longitude);
        }

        public NodeObject(
            long id, float latitude, float longitude, Dictionary<string, string> tags = null)
            : base(id, tags)
        {
            Location = new Location(latitude, longitude);
        }

        public NodeObject(
            long id, Location location, Dictionary<string, string> tags = null)
            : base(id, tags)
        {
            Location = location;
        }

        public NodeObject(Node node) : base(node)
        {
            Location = new Location(node.Latitude.Value, node.Longitude.Value);
        }
    }
}
