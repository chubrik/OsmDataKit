using System.Collections.Generic;
using System.Linq;

namespace OsmDataKit
{
    public static class GeoCoordsCollectionExtensions
    {
        public static GeoCoords AverageCoords(this IEnumerable<IGeoCoords> coordsColletion)
        {
            //todo AverageCoords
            var coordsList = coordsColletion.ToList();
            var minLat2 = coordsList.Min(i => i.Latitude);
            var maxLat2 = coordsList.Max(i => i.Latitude);
            var minLong = coordsList.Min(i => i.Longitude);
            var maxLong = coordsList.Max(i => i.Longitude);
            return new GeoCoords((minLat2 + maxLat2) / 2, (minLong + maxLong) / 2);
        }
    }
}
