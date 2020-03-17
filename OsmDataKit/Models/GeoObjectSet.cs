using OsmDataKit.Internal;
using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public sealed class GeoObjectSet
    {
        public IReadOnlyList<NodeObject> Nodes { get; internal set; }
        public IReadOnlyList<WayObject> Ways { get; internal set; }
        public IReadOnlyList<RelationObject> Relations { get; internal set; }
        public IReadOnlyList<long> MissedNodeIds { get; internal set; }
        public IReadOnlyList<long> MissedWayIds { get; internal set; }
        public IReadOnlyList<long> MissedRelationIds { get; internal set; }

        internal GeoObjectSet(GeoContext context)
        {
            Nodes = context.Nodes.Values.ToList();
            Ways = context.Ways.Values.ToList();
            Relations = context.Relations.Values.ToList();
            MissedNodeIds = context.MissedNodeIds;
            MissedWayIds = context.MissedWayIds;
            MissedRelationIds = context.MissedRelationIds;
        }
    }
}
