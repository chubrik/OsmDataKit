using Newtonsoft.Json;
using OsmSharp;
using System.Diagnostics;

namespace OsmDataKit
{
    [JsonObject]
    public class NodeObject : GeoObject, IGeoCoords
    {
        public override OsmGeoType Type => OsmGeoType.Node;

        [JsonIgnore]
        public float Latitude { get; set; }

        [JsonIgnore]
        public float Longitude { get; set; }

        public NodeObject() { }

        internal NodeObject(Node node) : base(node)
        {
            Latitude = (float)node.Latitude.GetValueOrDefault();
            Longitude = (float)node.Longitude.GetValueOrDefault();
        }

        [JsonProperty("c")]
        public float[] _coords
        {
            get => new[] { Latitude, Longitude };
            set
            {
                Debug.Assert(value?.Length == 2);
                Latitude = value[0];
                Longitude = value[1];
            }
        }
    }
}
