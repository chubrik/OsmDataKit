using System.Collections.Generic;

namespace OsmDataKit
{
    public class OsmObjectResponse
    {
        public IReadOnlyList<NodeObject> Nodes { get; set; }
        public IReadOnlyList<WayObject> Ways { get; set; }
        public IReadOnlyList<RelationObject> Relations { get; set; }
        public IReadOnlyList<WayObject> BrokenWays { get; set; }
        public IReadOnlyList<RelationObject> BrokenRelations { get; set; }
        public IReadOnlyDictionary<long, NodeObject> AllNodesDict { get; set; }
        public IReadOnlyDictionary<long, WayObject> AllWaysDict { get; set; }
        public IReadOnlyDictionary<long, RelationObject> AllRelationsDict { get; set; }
    }
}
