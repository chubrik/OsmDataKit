using System.Collections.Generic;

namespace OsmDataKit
{
    public sealed class OsmObjectResponse
    {
        public IReadOnlyDictionary<long, NodeObject> Nodes { get; set; }
        public IReadOnlyDictionary<long, WayObject> Ways { get; set; }
        public IReadOnlyDictionary<long, RelationObject> Relations { get; set; }

        public IReadOnlyDictionary<long, WayObject> BrokenWays { get; set; }
        public IReadOnlyDictionary<long, RelationObject> BrokenRelations { get; set; }

        public List<long> MissedNodeIds { get; set; }
        public List<long> MissedWayIds { get; set; }
        public List<long> MissedRelationIds { get; set; }
    }
}
