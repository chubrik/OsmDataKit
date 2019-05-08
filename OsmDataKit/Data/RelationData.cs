using Newtonsoft.Json;
using System.Collections.Generic;

namespace OsmDataKit.Data
{
    [JsonObject]
    internal class RelationData : OsmGeoData
    {
        [JsonProperty("m")]
        public List<RelationMemberData> Members { get; set; }
    }
}
