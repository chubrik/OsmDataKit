using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public static class RelationObjectExtensions
    {
        public static IEnumerable<NodeObject> DeepNodes(this RelationObject relation) =>
            relation.Members.Nodes().Concat(relation.Members.Ways().SelectMany(i => i.Nodes))
                                    .Concat(relation.Members.Relations().SelectMany(DeepNodes))
                                    .Distinct();

        public static IEnumerable<WayObject> DeepWays(this RelationObject relation) =>
            relation.Members.Ways().Concat(relation.Members.Relations().SelectMany(DeepWays))
                                   .Distinct();

        public static IEnumerable<RelationObject> DeepRelations(this RelationObject relation)
        {
            var relations = relation.Members.Relations().ToList();
            return relations.Concat(relations.SelectMany(DeepRelations))
                            .Distinct();
        }

        public static bool IsCompleted(this RelationObject relation) =>
            (relation.MissedMembers == null || relation.MissedMembers.Count == 0) &&
            relation.Members.All(i => i.Geo.IsCompleted());
    }
}
