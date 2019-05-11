using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    internal static class RelationObjectExtensions
    {
        public static IEnumerable<NodeObject> GetAllNodes(this RelationObject relation) =>
            relation.Members.Nodes().Concat(relation.Members.Ways().SelectMany(i => i.Nodes))
                                    .Concat(relation.Members.Relations().SelectMany(GetAllNodes));

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
