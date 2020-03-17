using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public static class RelationObjectExtensions
    {
        public static IEnumerable<NodeObject> AllChildNodes(this RelationObject relation) =>
            relation.Members.Nodes().Concat(relation.Members.Ways().SelectMany(i => i.Nodes))
                                    .Concat(relation.Members.Relations().SelectMany(AllChildNodes))
                                    .Distinct();

        public static IEnumerable<WayObject> AllChildWays(this RelationObject relation) =>
            relation.Members.Ways().Concat(relation.Members.Relations().SelectMany(AllChildWays))
                                   .Distinct();

        public static IEnumerable<RelationObject> AllChildRelations(this RelationObject relation)
        {
            var memberRelations = relation.Members.Relations().ToList();

            return memberRelations.Concat(memberRelations.SelectMany(AllChildRelations))
                                  .Distinct();
        }

        public static bool IsComplete(this RelationObject relation) =>
            relation.MissedMembers == null && relation.Members.All(i => i.Geo.IsComplete());
    }
}
