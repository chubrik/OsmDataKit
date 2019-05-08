using Newtonsoft.Json;
using System.Collections.Generic;

namespace OsmDataKit.Data
{
    [JsonObject]
    internal class WayData : OsmGeoData
    {
        [JsonProperty("n")]
        public List<long> NodeIds { get; set; }
    }
}
