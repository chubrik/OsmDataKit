using System.Collections.Generic;

namespace OsmDataKit
{
    public sealed class CompleteGeoObjects
    {
        public IReadOnlyList<NodeObject> RootNodes { get; internal set; }
        public IReadOnlyList<WayObject> RootWays { get; internal set; }
        public IReadOnlyList<RelationObject> RootRelations { get; internal set; }
        public IReadOnlyList<long> MissedNodeIds { get; internal set; }
        public IReadOnlyList<long> MissedWayIds { get; internal set; }
        public IReadOnlyList<long> MissedRelationIds { get; internal set; }

        internal CompleteGeoObjects() { }
    }
}
