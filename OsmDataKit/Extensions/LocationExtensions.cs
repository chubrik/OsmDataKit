namespace OsmDataKit;

using System;
using System.Collections.Generic;

public static class LocationExtensions
{
    public static Location CenterLocation(this IEnumerable<Location> locations)
    {
        if (locations == null)
            throw new ArgumentNullException(nameof(locations));

        var minLat = float.NaN;
        var maxLat = float.NaN;
        var minLng = float.NaN;
        var maxLng = float.NaN;

        foreach (var location in locations)
        {
            if (float.IsNaN(minLat))
            {
                minLat = maxLat = location.Latitude;
                minLng = maxLng = location.Longitude;
                continue;
            }

            if (minLat > location.Latitude)
                minLat = location.Latitude;
            else
            if (maxLat < location.Latitude)
                maxLat = location.Latitude;

            if (minLng > location.Longitude)
                minLng = location.Longitude;
            else
            if (maxLng < location.Longitude)
                maxLng = location.Longitude;
        }

        if (float.IsNaN(minLat))
            throw new ArgumentException(nameof(locations));

        return new Location((minLat + maxLat) / 2, (minLng + maxLng) / 2);
    }
}
