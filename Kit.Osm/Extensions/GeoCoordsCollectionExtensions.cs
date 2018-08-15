using System.Collections.Generic;
using System.Linq;

namespace Kit.Osm
{
    public static class GeoCoordsCollectionExtensions
    {
        public static GeoCoords AverageCoords(this IReadOnlyCollection<IGeoCoords> coordsColletion)
        {
            var minLat = coordsColletion.Min(i => i.Latitude);
            var maxLat = coordsColletion.Max(i => i.Latitude);
            var minLong = coordsColletion.Min(i => i.Longitude);
            var maxLong = coordsColletion.Max(i => i.Longitude);
            return new GeoCoords((minLat + maxLat) / 2, (minLong + maxLong) / 2);
        }
    }
}
