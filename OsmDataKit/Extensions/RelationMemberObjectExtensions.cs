using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public static class RelationMemberObjectExtensions
    {
        public static IEnumerable<NodeObject> GetNodes(this IEnumerable<RelationMemberObject> members) =>
            members.Where(i => i.Geo is NodeObject).Select(i => i.Geo as NodeObject);

        public static IEnumerable<WayObject> GetWays(this IEnumerable<RelationMemberObject> members) =>
            members.Where(i => i.Geo is WayObject).Select(i => i.Geo as WayObject);

        public static IEnumerable<RelationObject> GetRelations(this IEnumerable<RelationMemberObject> members) =>
            members.Where(i => i.Geo is RelationObject).Select(i => i.Geo as RelationObject);
    }
}
