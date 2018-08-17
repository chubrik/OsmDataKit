using Newtonsoft.Json;

namespace OsmDataKit.Models
{
    [JsonObject]
    internal class NodeData : OsmGeoData
    {
        [JsonProperty("c")]
        public double[] Coords { get; set; }
    }
}
