using Newtonsoft.Json;

namespace OsmDataKit.Data
{
    [JsonObject]
    internal class NodeData : OsmGeoData
    {
        [JsonProperty("c")]
        public double[] Coords { get; set; }
    }
}
