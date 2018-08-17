using System.Collections.Generic;

namespace OsmDataKit.Models
{
    public class OsmRequest
    {
        public IEnumerable<long> NodeIds { get; set; }
        public IEnumerable<long> WayIds { get; set; }
        public IEnumerable<long> RelationIds { get; set; }
    }
}
