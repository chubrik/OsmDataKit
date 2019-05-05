using Newtonsoft.Json;

namespace OsmDataKit.Internal
{
    [JsonObject]
    internal class NodeData : OsmGeoData
    {
        [JsonProperty("c")]
        public double[] Coords { get; set; }
    }
}
