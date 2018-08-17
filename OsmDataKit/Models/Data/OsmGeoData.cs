using Newtonsoft.Json;
using System.Collections.Generic;

namespace OsmDataKit.Models
{
    [JsonObject]
    internal abstract class OsmGeoData
    {
        [JsonProperty("i")]
        public long Id { get; set; }

        [JsonProperty("g")]
        public Dictionary<string, string> Tags { get; set; }
    }
}
