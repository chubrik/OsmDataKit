using System;

namespace OsmDataKit
{
    public static class GeoObjectExtensions
    {
        public static IGeoCoords GetCenterCoords(this GeoObject geo)
        {
            switch (geo)
            {
                case NodeObject node:
                    return node;

                case WayObject way:
                    return way.Nodes.AverageCoords();

                case RelationObject relation:
                    return relation.GetAllNestedNodes().AverageCoords();

                default:
                    throw new ArgumentOutOfRangeException(nameof(geo));
            }
        }
    }
}
