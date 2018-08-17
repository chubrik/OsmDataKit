using Newtonsoft.Json;
using OsmSharp;

namespace OsmDataKit.Models
{
    [JsonObject]
    internal class RelationMemberData
    {
        [JsonProperty("i")]
        public long Id { get; set; }

        [JsonProperty("t")]
        public OsmGeoType Type { get; set; }

        [JsonProperty("r")]
        public string Role { get; set; }
    }
}
