using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    internal static class GeoExtensions
    {
        public static OsmGeoType ToOsmType(this GeoType type)
        {
            switch (type)
            {
                case GeoType.Point:
                    return OsmGeoType.Node;

                case GeoType.Line:
                    return OsmGeoType.Way;

                case GeoType.Group:
                    return OsmGeoType.Relation;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public static GeoCoords AverageCoords(this IReadOnlyCollection<GeoPoint> points) =>
            new GeoCoords(
                points.Sum(i => i.Latitude) / points.Count,
                points.Sum(i => i.Longitude) / points.Count
            );

        private static readonly List<string> _tagNames =
            new List<string> { "name:en", "int_name", "name" };

        private static readonly List<string> _tagNamesCyr =
            new List<string> { "name:ru", "name" };

        public static string TryGetTitle(this GeoObject geo)
        {
            Debug.Assert(geo != null);

            if (geo == null)
                throw new ArgumentNullException(nameof(geo));

            if (geo.Tags.Count == 0)
                return null;

            string title;

            foreach (var tagName in _tagNames)
                if (geo.Tags.TryGetValue(tagName, out title) && TextHelper.HasOnlyLatinLetters(title))
                    return TextHelper.FixApostrophe(title);

            foreach (var tagName in _tagNamesCyr)
                if (geo.Tags.TryGetValue(tagName, out title) && TextHelper.HasOnlyCyrillicLetters(title))
                    return TextHelper.CyrillicToLatin(title);

            return null;
        }
    }
}
