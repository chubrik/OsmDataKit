using System.Collections.Generic;

namespace OsmDataKit
{
    public sealed class OsmObjectResponse
    {
        public IReadOnlyDictionary<long, NodeObject> RootNodes { get; set; }
        public IReadOnlyDictionary<long, WayObject> RootWays { get; set; }
        public IReadOnlyDictionary<long, RelationObject> RootRelations { get; set; }

        public List<long> MissedNodeIds { get; set; }
        public List<long> MissedWayIds { get; set; }
        public List<long> MissedRelationIds { get; set; }
    }
}
