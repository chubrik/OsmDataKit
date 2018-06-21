using OsmSharp;
using System;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    internal static class WayExtensions
    {
        public static WayData ToData(this Way way)
        {
            Debug.Assert(way.Id.HasValue);

            if (!way.Id.HasValue)
                throw new ArgumentException(nameof(way));

            return new WayData
            {
                Id = way.Id.Value,
                Tags = way.Tags.ToDictionary(i => i.Key, i => i.Value),
                NodeIds = way.Nodes.ToList()
            };
        }
    }
}
