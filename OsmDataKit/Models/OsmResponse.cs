using OsmSharp;
using System.Collections.Generic;

namespace OsmDataKit
{
    public class OsmResponse
    {
        public Dictionary<long, Node> Nodes { get; internal set; }
        public Dictionary<long, Way> Ways { get; internal set; }
        public Dictionary<long, Relation> Relations { get; internal set; }
        public List<long> MissedNodeIds { get; internal set; }
        public List<long> MissedWayIds { get; internal set; }
        public List<long> MissedRelationIds { get; internal set; }
    }
}
