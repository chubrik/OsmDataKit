using System.Collections.Generic;

namespace OsmDataKit
{
    public sealed class OsmObjectResponse
    {
        public IReadOnlyList<NodeObject> Nodes { get; internal set; }
        public IReadOnlyList<WayObject> Ways { get; internal set; }
        public IReadOnlyList<RelationObject> Relations { get; internal set; }
        public IReadOnlyList<WayObject> BrokenWays { get; internal set; }
        public IReadOnlyList<RelationObject> BrokenRelations { get; internal set; }

        internal OsmObjectResponse() { }
    }
}
