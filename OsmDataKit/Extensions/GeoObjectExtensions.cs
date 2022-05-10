namespace OsmDataKit;

using System;
using System.Linq;

public static class GeoObjectExtensions
{
    public static Location CenterLocation(this GeoObject geo) =>
        geo switch
        {
            NodeObject node => node.Location,
            WayObject way => way.Nodes.Select(i => i.Location).CenterLocation(),
            RelationObject relation => relation.AllChildNodes().Select(i => i.Location).CenterLocation(),
            _ => throw new InvalidOperationException(),
        };

    public static bool IsComplete(this GeoObject geo) =>
        geo switch
        {
            NodeObject _ => true,
            WayObject way => way.IsComplete,
            RelationObject relation => relation.IsComplete(),
            _ => throw new InvalidOperationException(),
        };
}
