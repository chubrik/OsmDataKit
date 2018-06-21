using System.Collections.Generic;

namespace Kit.Osm
{
    public class OsmObjectResponse
    {
        public IReadOnlyList<OsmNode> Nodes { get; set; }
        public IReadOnlyList<OsmWay> Ways { get; set; }
        public IReadOnlyList<OsmRelation> Relations { get; set; }
        public IReadOnlyList<OsmWay> BrokenWays { get; set; }
        public IReadOnlyList<OsmRelation> BrokenRelations { get; set; }
        public IReadOnlyDictionary<long, OsmNode> AllNodesDict { get; set; }
        public IReadOnlyDictionary<long, OsmWay> AllWaysDict { get; set; }
        public IReadOnlyDictionary<long, OsmRelation> AllRelationsDict { get; set; }
    }
}
