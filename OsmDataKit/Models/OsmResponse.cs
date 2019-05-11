using System.Collections.Generic;

namespace OsmDataKit
{
    public sealed class OsmResponse
    {
        public Dictionary<long, NodeObject> Nodes { get; internal set; }
        public Dictionary<long, WayObject> Ways { get; internal set; }
        public Dictionary<long, RelationObject> Relations { get; internal set; }
        public List<long> MissedNodeIds { get; internal set; }
        public List<long> MissedWayIds { get; internal set; }
        public List<long> MissedRelationIds { get; internal set; }

        internal OsmResponse() { }
    }
}
