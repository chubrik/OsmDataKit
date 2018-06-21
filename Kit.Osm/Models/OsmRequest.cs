using System.Collections.Generic;

namespace Kit.Osm
{
    public class OsmRequest
    {
        public IEnumerable<long> NodeIds { get; set; }
        public IEnumerable<long> WayIds { get; set; }
        public IEnumerable<long> RelationIds { get; set; }
    }
}
