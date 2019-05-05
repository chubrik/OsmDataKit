using Newtonsoft.Json;
using System.Collections.Generic;

namespace OsmDataKit.Internal
{
    [JsonObject]
    internal class RelationData : OsmGeoData
    {
        [JsonProperty("m")]
        public List<RelationMemberData> Members { get; set; }
    }
}
