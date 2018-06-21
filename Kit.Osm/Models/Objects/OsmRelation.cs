using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    public class OsmRelation : OsmObject
    {
        public IReadOnlyList<long> MemberIds { get; }
        public IReadOnlyList<OsmMember> Members { get; internal set; }

        internal OsmRelation(RelationData data) : base(data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Type = OsmGeoType.Relation;
            MemberIds = data.Members.Select(i => i.Id).ToList();
        }

        public override bool IsBroken() =>
            MemberIds.Count != Members.Count || Members.Any(i => i.Geo.IsBroken());

        public override GeoCoords AverageCoords()
        {
            var relWays = Relations().SelectMany(i => i.Ways());
            var allWays = relWays.Concat(Ways());
            var allNodes = allWays.SelectMany(i => i.Nodes).ToList();
            return OsmHelper.AverageCoords(allNodes);
        }

        public IEnumerable<OsmNode> Nodes() =>
            Members.Where(i => i.Geo.Type == OsmGeoType.Node)
                   .Select(i => (OsmNode)i.Geo);

        public IEnumerable<OsmWay> Ways() =>
            Members.Where(i => i.Geo.Type == OsmGeoType.Way)
                   .Select(i => (OsmWay)i.Geo);

        public IEnumerable<OsmRelation> Relations() =>
            Members.Where(i => i.Geo.Type == OsmGeoType.Relation)
                   .Select(i => (OsmRelation)i.Geo);
    }
}
