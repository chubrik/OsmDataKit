using Newtonsoft.Json;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    [JsonObject]
    internal class OsmWayData : OsmObjectData
    {
        [JsonProperty("n")]
        public List<long> NodeIds { get; private set; }

        public OsmWayData() { }

        public OsmWayData(Way way) : base(way)
        {
            Debug.Assert(way != null);

            if (way.Nodes == null)
                throw new ArgumentException(nameof(way));

            NodeIds = way.Nodes.ToList();
        }
    }
}
