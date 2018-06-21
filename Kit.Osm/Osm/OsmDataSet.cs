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
        public List<long> MissedNodesIds { get; set; }

        [JsonProperty]
        public List<long> MissedWaysIds { get; set; }

        [JsonProperty]
        public List<long> MissedRelationIds { get; set; }
    }
}
