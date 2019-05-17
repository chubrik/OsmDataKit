using OsmSharp;
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
                    return relation.DeepNodes().AverageCoords();

                default:
                    throw new ArgumentOutOfRangeException(nameof(OsmGeoType));
            }
        }

        public static bool IsCompleted(this GeoObject geo)
        {
            switch (geo)
            {
                case NodeObject _:
                    return true;

                case WayObject way:
                    return way.IsCompleted;

                case RelationObject rel:
                    return rel.IsCompleted();

                default:
                    throw new ArgumentOutOfRangeException(nameof(OsmGeoType));
            }
        }
    }
}
