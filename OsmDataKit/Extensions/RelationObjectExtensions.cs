namespace OsmDataKit;

using System;
using System.Collections.Generic;
using System.Linq;

public static class RelationObjectExtensions
{
    public static IEnumerable<NodeObject> AllChildNodes(this RelationObject relation)
    {
        var members = relation.Members ?? throw new InvalidOperationException();

        return members.Nodes().Concat(members.Ways().SelectMany(i => i.Nodes))
                              .Concat(members.Relations().SelectMany(AllChildNodes))
                              .Distinct();
    }

    public static IEnumerable<WayObject> AllChildWays(this RelationObject relation)
    {
        var members = relation.Members ?? throw new InvalidOperationException();

        return members.Ways().Concat(members.Relations().SelectMany(AllChildWays))
                             .Distinct();
    }

    public static IEnumerable<RelationObject> AllChildRelations(this RelationObject relation)
    {
        var memberRelations =
            relation.Members?.Relations().ToList() ?? throw new InvalidOperationException();

        return memberRelations.Concat(memberRelations.SelectMany(AllChildRelations))
                              .Distinct();
    }

    public static bool IsComplete(this RelationObject relation) =>
        relation.MissedMembers == null && relation.Members.All(i => i.Geo.IsComplete());
}
