using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit.Internal
{
    [JsonObject]
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

        [JsonProperty("Nodes")]
        public NodeObject[] _nodes
        {
            get => Nodes.Values.ToArray();
            set => Nodes = value.ToDictionary(i => i.Id);
        }

        [JsonProperty("Ways")]
        public WayObject[] _ways
        {
            get => Ways.Values.ToArray();
            set => Ways = value.ToDictionary(i => i.Id);
        }

        [JsonProperty("Relations")]
        public RelationObject[] _relations
        {
            get => Relations.Values.ToArray();
            set => Relations = value.ToDictionary(i => i.Id);
        }

        internal Dictionary<long, RelationObject> AllRelations { get; set; }
    }
}
