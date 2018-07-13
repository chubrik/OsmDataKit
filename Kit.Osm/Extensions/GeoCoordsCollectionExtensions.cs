using System.Collections.Generic;

namespace Kit.Osm
{
    public static class GeoCoordsCollectionExtensions
    {
        public static GeoCoords AverageCoords(this IReadOnlyCollection<IGeoCoords> coordsColletion)
        {
            var latitudeSum = 0d;
            var longitudeSum = 0d;

            foreach (var coords in coordsColletion)
            {
                latitudeSum += coords.Latitude;
                longitudeSum += coords.Longitude;
            }

            return new GeoCoords(latitudeSum / coordsColletion.Count, longitudeSum / coordsColletion.Count);
        }
    }
}
