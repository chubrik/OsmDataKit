using System;

namespace OsmDataKit
{
    public static class GeoObjectExtensions
    {
        public static IGeoCoords CenterCoords(this GeoObject geo)
        {
            switch (geo)
            {
                case NodeObject node:
                    return node;

                case WayObject way:
                    return way.Nodes.AverageCoords();

                case RelationObject relation:
                    return relation.AllNodes().AverageCoords();

                default:
                    throw new ArgumentOutOfRangeException(nameof(geo));
            }
        }
    }
}
