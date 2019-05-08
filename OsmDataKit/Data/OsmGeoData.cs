using Newtonsoft.Json;
using System.Collections.Generic;

namespace OsmDataKit.Data
{
    [JsonObject]
    internal abstract class OsmGeoData
    {
        [JsonProperty("i")]
        public long Id { get; set; }

        [JsonProperty("g", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Tags { get; set; }
    }
}
