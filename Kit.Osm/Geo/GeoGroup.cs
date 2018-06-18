using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    public class GeoGroup : GeoObject
    {
        public IReadOnlyList<long> MemberIds { get; }
        public IReadOnlyList<GeoMember> Members { get; private set; }

        internal GeoGroup(OsmRelationData data) : base(data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Type = GeoType.Group;
            MemberIds = data.Members.Select(i => i.Id).ToList();
        }

        internal void SetMembers(IReadOnlyList<GeoMember> members)
        {
            Debug.Assert(Members == null);

            if (Members != null)
                throw new InvalidOperationException();

            Members = members ?? throw new ArgumentNullException(nameof(members));
        }

        public override bool IsBroken() =>
            MemberIds.Count != Members.Count || Members.Any(i => i.Geo.IsBroken());

        public override GeoCoords AverageCoords()
        {
            var groupLines = Groups().SelectMany(i => i.Lines());
            var allLines = groupLines.Concat(Lines());
            var allPoints = allLines.SelectMany(i => i.Points).ToList();
            return allPoints.AverageCoords();
        }

        public IEnumerable<GeoPoint> Points() =>
            Members.Where(i => i.Geo.Type == GeoType.Point)
                   .Select(i => (GeoPoint)i.Geo);

        public IEnumerable<GeoLine> Lines() =>
            Members.Where(i => i.Geo.Type == GeoType.Line)
                   .Select(i => (GeoLine)i.Geo);

        public IEnumerable<GeoGroup> Groups() =>
            Members.Where(i => i.Geo.Type == GeoType.Group)
                   .Select(i => (GeoGroup)i.Geo);
    }
}
