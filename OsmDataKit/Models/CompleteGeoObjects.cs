namespace OsmDataKit;

using System.Collections.Generic;

public sealed class CompleteGeoObjects
{
    public IReadOnlyList<NodeObject> RootNodes { get; internal set; }
    public IReadOnlyList<WayObject> RootWays { get; internal set; }
    public IReadOnlyList<RelationObject> RootRelations { get; internal set; }
    public IReadOnlyList<long> MissedNodeIds { get; internal set; }
    public IReadOnlyList<long> MissedWayIds { get; internal set; }
    public IReadOnlyList<long> MissedRelationIds { get; internal set; }

    internal CompleteGeoObjects(
        IReadOnlyList<NodeObject> rootNodes,
        IReadOnlyList<WayObject> rootWays,
        IReadOnlyList<RelationObject> rootRelations,
        IReadOnlyList<long> missedNodeIds,
        IReadOnlyList<long> missedWayIds,
        IReadOnlyList<long> missedRelationIds)
    {
        RootNodes = rootNodes;
        RootWays = rootWays;
        RootRelations = rootRelations;
        MissedNodeIds = missedNodeIds;
        MissedWayIds = missedWayIds;
        MissedRelationIds = missedRelationIds;
    }
}
