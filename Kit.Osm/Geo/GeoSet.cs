using System.Collections.Generic;

namespace Kit.Osm
{
    public class GeoSet
    {
        public IReadOnlyList<GeoPoint> Points { get; set; }
        public IReadOnlyList<GeoLine> Lines { get; set; }
        public IReadOnlyList<GeoGroup> Groups { get; set; }
        public IReadOnlyList<GeoLine> BrokenLines { get; set; }
        public IReadOnlyList<GeoGroup> BrokenGroups { get; set; }
        public IReadOnlyDictionary<long, GeoPoint> AllPointsDict { get; set; }
        public IReadOnlyDictionary<long, GeoLine> AllLinesDict { get; set; }
        public IReadOnlyDictionary<long, GeoGroup> AllGroupsDict { get; set; }
    }
}
