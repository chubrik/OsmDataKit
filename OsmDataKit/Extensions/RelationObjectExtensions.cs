using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public static class RelationObjectExtensions
    {
        public static IEnumerable<NodeObject> AllNodes(this RelationObject relation) =>
            relation.Members.Nodes().Concat(relation.Members.Ways().SelectMany(i => i.Nodes))
                                    .Concat(relation.Members.Relations().SelectMany(AllNodes)).Distinct();

        public static IEnumerable<WayObject> AllWays(this RelationObject relation) =>
            relation.Members.Ways().Concat(relation.Members.Relations().SelectMany(AllWays)).Distinct();

        public static IEnumerable<RelationObject> AllRelations(this RelationObject relation)
        {
            var relations = relation.Members.Relations().ToList();
            return relations.Concat(relations.SelectMany(AllRelations)).Distinct();
        }

        public static bool IsCompleted(this RelationObject relation) =>
            (relation.MissedMembers == null || relation.MissedMembers.Count == 0) &&
            relation.Members.All(i => i.Geo.IsCompleted());
    }
}
