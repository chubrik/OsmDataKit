using OsmSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kit.Osm
{
    internal static class OsmExtensions
    {
        public static GeoType ToGeoType(this OsmGeoType type)
        {
            switch (type)
            {
                case OsmGeoType.Node:
                    return GeoType.Point;

                case OsmGeoType.Way:
                    return GeoType.Line;

                case OsmGeoType.Relation:
                    return GeoType.Group;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public static IDictionary<string, string> TagsDictionary(this OsmGeo geo) =>
            geo.Tags.ToDictionary(i => i.Key, i => i.Value);
    }
}
