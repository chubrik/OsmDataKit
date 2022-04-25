using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OsmDataKit.Internal
{
    internal sealed class GeoContext
    {
        [JsonIgnore]
        public Dictionary<long, NodeObject> Nodes { get; set; } = new Dictionary<long, NodeObject>(0);

        [JsonIgnore]
        public Dictionary<long, WayObject> Ways { get; set; } = new Dictionary<long, WayObject>(0);

        [JsonIgnore]
        public Dictionary<long, RelationObject> Relations { get; set; } = new Dictionary<long, RelationObject>(0);

        public List<long> MissedNodeIds { get; set; } = new List<long>(0);
        public List<long> MissedWayIds { get; set; } = new List<long>(0);
        public List<long> MissedRelationIds { get; set; } = new List<long>(0);

        [JsonPropertyName("Nodes")]
        public NodeObject[] JsonNodes
        {
            get => Nodes.Values.ToArray();
            set => Nodes = value.ToDictionary(i => i.Id);
        }

        [JsonPropertyName("Ways")]
        public WayObject[] JsonWays
        {
            get => Ways.Values.ToArray();
            set => Ways = value.ToDictionary(i => i.Id);
        }

        [JsonPropertyName("Relations")]
        public RelationObject[] JsonRelations
        {
            get => Relations.Values.ToArray();
            set => Relations = value.ToDictionary(i => i.Id);
        }
    }
}
