using OsmSharp;
using System;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    internal static class RelationExtensions
    {
        public static RelationData ToData(this Relation relation)
        {
            Debug.Assert(relation.Id.HasValue);

            if (!relation.Id.HasValue)
                throw new ArgumentException(nameof(relation));

            return new RelationData
            {
                Id = relation.Id.Value,
                Tags = relation.Tags.ToDictionary(i => i.Key, i => i.Value),
                Members = relation.Members.Select(i => i.ToData()).ToList()
            };
        }
    }
}
