using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public static class RelationObjectExtensions
    {
        public static IEnumerable<NodeObject> GetNodes(this RelationObject relation) =>
            relation.Members.GetNodes();

        public static IEnumerable<WayObject> GetWays(this RelationObject relation) =>
            relation.Members.GetWays();

        public static IEnumerable<RelationObject> GetRelations(this RelationObject relation) =>
            relation.Members.GetRelations();

        public static IEnumerable<NodeObject> GetAllNestedNodes(this RelationObject relation) =>
            relation.GetNodes().Concat(relation.GetWays().SelectMany(i => i.Nodes))
                               .Concat(relation.GetRelations().SelectMany(i => i.GetAllNestedNodes()));

        internal static bool GetIsBroken(this RelationObject relation)
        {
            foreach (var member in relation.Members)
            {
                switch (member.Geo)
                {
                    case NodeObject node:
                        break;

                    case WayObject way:

                        if (way.IsBroken)
                            return true;

                        break;

                    case RelationObject rel:

                        if (rel.GetIsBroken())
                            return true;

                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(member));
                }
            }

            return false;
        }
    }
}
