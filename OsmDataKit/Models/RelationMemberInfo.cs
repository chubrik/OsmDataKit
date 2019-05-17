using Newtonsoft.Json;
using OsmSharp;

namespace OsmDataKit
{
    [JsonObject]
    public class RelationMemberInfo
    {
        [JsonProperty("r")]
        public string Role { get; set; }

        [JsonProperty("t")]
        public OsmGeoType Type { get; set; }

        [JsonProperty("i")]
        public long Id { get; set; }

        [JsonIgnore]
        public string Url => $"https://www.openstreetmap.org/{Type.ToString().ToLower()}/{Id}";

        public override string ToString() => Role + " - " + Type.ToString()[0] + Id.ToString();
    }
}
