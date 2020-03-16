using System;
using System.Linq;

namespace OsmDataKit
{
    public static class GeoObjectExtensions
    {
        public static Location CenterLocation(this GeoObject geo) =>
            geo switch
            {
                NodeObject node => node.Location,
                WayObject way => way.Nodes.Select(i => i.Location).CenterLocation(),
                RelationObject relation => relation.DeepNodes().Select(i => i.Location).CenterLocation(),
                _ => throw new InvalidOperationException(),
            };

        public static bool IsCompleted(this GeoObject geo) =>
            geo switch
            {
                NodeObject _ => true,
                WayObject way => way.IsCompleted,
                RelationObject relation => relation.IsCompleted(),
                _ => throw new InvalidOperationException(),
            };
    }
}
