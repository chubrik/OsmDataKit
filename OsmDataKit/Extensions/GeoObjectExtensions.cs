using OsmSharp;
using System;

namespace OsmDataKit
{
    public static class GeoObjectExtensions
    {
        public static IGeoPoint CenterPoint(this GeoObject geo)
        {
            switch (geo)
            {
                case NodeObject node:
                    return node;

                case WayObject way:
                    return way.Nodes.CenterPoint();

                case RelationObject relation:
                    return relation.DeepNodes().CenterPoint();

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
