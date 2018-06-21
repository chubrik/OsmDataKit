using Newtonsoft.Json;
using System.Collections.Generic;

namespace Kit.Osm
{
    [JsonObject]
    internal class OsmResponseData
    {
        [JsonProperty]
        public List<NodeData> Nodes { get; set; }

        [JsonProperty]
        public List<WayData> Ways { get; set; }

        [JsonProperty]
        public List<RelationData> Relations { get; set; }

        [JsonProperty]
        public List<long> MissedNodeIds { get; set; }

        [JsonProperty]
        public List<long> MissedWayIds { get; set; }

        [JsonProperty]
        public List<long> MissedRelationIds { get; set; }
    }
}
