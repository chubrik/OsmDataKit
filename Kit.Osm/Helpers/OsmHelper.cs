using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    internal static class OsmHelper
    {
        public static GeoCoords AverageCoords(IReadOnlyCollection<OsmNode> nodes) =>
            new GeoCoords(
                nodes.Sum(i => i.Latitude) / nodes.Count,
                nodes.Sum(i => i.Longitude) / nodes.Count
            );

        private static readonly List<string> _tagNames =
            new List<string> { "name:en", "int_name", "name" };

        private static readonly List<string> _tagNamesCyr =
            new List<string> { "name:ru", "name" };

        public static string TryGetTitle(OsmObject geo)
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
