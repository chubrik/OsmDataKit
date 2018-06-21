using Newtonsoft.Json;
using System.Collections.Generic;

namespace Kit.Osm
{
    [JsonObject]
    internal class WayData : OsmGeoData
    {
        [JsonProperty("n")]
        public List<long> NodeIds { get; set; }
    }
}
