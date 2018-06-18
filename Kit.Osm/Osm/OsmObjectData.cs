using Newtonsoft.Json;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kit.Osm
{
    [JsonObject]
    internal abstract class OsmObjectData
    {
        [JsonProperty("i")]
        public long Id { get; private set; }

        [JsonProperty("g")]
        public IDictionary<string, string> Tags { get; private set; }

        public OsmObjectData() { }

        protected OsmObjectData(OsmGeo osmGeo)
        {
            Debug.Assert(osmGeo.Id.HasValue);

            if (!osmGeo.Id.HasValue)
                throw new ArgumentException(nameof(osmGeo));

            Id = osmGeo.Id.Value;
            Tags = osmGeo.TagsDictionary();
        }
    }
}
