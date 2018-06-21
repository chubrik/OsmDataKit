using Newtonsoft.Json;

namespace Kit.Osm
{
    [JsonObject]
    internal class NodeData : OsmGeoData
    {
        [JsonProperty("c")]
        public double[] Coords { get; set; }
    }
}
