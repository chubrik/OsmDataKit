using System;
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

        public static bool HasMissedParts(this RelationObject relation)
        {
            if (relation.MissedMembers?.Count > 0)
                return true;

            foreach (var member in relation.Members)
                switch (member.Geo)
                {
                    case NodeObject node:
                        break;

                    case WayObject way:

                        if (way.HasMissedNodes)
                            return true;

                        break;

                    case RelationObject rel:

                        if (rel.HasMissedParts())
                            return true;

                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(member));
                }

            return false;
        }
    }
}
