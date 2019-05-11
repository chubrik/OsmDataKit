using OsmSharp;
using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public static class RelationMemberObjectExtensions
    {
        public static IEnumerable<NodeObject> Nodes(this IEnumerable<RelationMemberObject> members) =>
            members.Where(i => i.Type == OsmGeoType.Node).Select(i => i.Geo as NodeObject);

        public static IEnumerable<WayObject> Ways(this IEnumerable<RelationMemberObject> members) =>
            members.Where(i => i.Type == OsmGeoType.Way).Select(i => i.Geo as WayObject);

        public static IEnumerable<RelationObject> Relations(this IEnumerable<RelationMemberObject> members) =>
            members.Where(i => i.Type == OsmGeoType.Relation).Select(i => i.Geo as RelationObject);
    }
}
