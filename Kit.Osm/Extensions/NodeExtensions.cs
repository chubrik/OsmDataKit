using OsmSharp;
using System;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    internal static class NodeExtensions
    {
        public static NodeData ToData(this Node node)
        {
            var isValid = node.Id.HasValue && node.Latitude.HasValue && node.Longitude.HasValue;
            Debug.Assert(isValid);

            if (!isValid)
                throw new ArgumentException(nameof(node));

            return new NodeData
            {
                Id = node.Id.Value,
                Tags = node.Tags.ToDictionary(i => i.Key, i => i.Value),
                Coords = new[] { node.Latitude.Value, node.Longitude.Value }
            };
        }
    }
}
