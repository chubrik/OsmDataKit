using Newtonsoft.Json;
using OsmSharp;
using System;
using System.Diagnostics;

namespace Kit.Osm
{
    [JsonObject]
    internal class OsmNodeData : OsmObjectData
    {
        [JsonProperty("c")]
        public float[] Coords { get; private set; }

        public OsmNodeData() { }

        public OsmNodeData(Node node) : base(node)
        {
            Debug.Assert(node.Latitude.HasValue && node.Longitude.HasValue);

            if (!node.Latitude.HasValue || !node.Longitude.HasValue)
                throw new ArgumentException(nameof(node));

            Coords = new[] { (float)node.Latitude.Value, (float)node.Longitude.Value };
        }
    }
}
