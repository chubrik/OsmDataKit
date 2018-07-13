using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    public class OsmRelation : OsmObject
    {
        public IReadOnlyList<long> MemberIds { get; private set; }
        public IReadOnlyList<OsmMember> Members { get; internal set; }

        #region Extensions

        public IEnumerable<OsmNode> Nodes =>
            Members.Where(i => i.Geo.Type == OsmGeoType.Node)
                   .Select(i => (OsmNode)i.Geo);

        public IEnumerable<OsmWay> Ways =>
            Members.Where(i => i.Geo.Type == OsmGeoType.Way)
                   .Select(i => (OsmWay)i.Geo);

        public IEnumerable<OsmRelation> Relations =>
            Members.Where(i => i.Geo.Type == OsmGeoType.Relation)
                   .Select(i => (OsmRelation)i.Geo);

        #endregion

        #region Overrides

        public override OsmGeoType Type => OsmGeoType.Relation;

        private bool? _isBroken;

        public override bool IsBroken =>
            _isBroken ?? (_isBroken = MemberIds.Count != Members.Count || Members.Any(i => i.Geo.IsBroken)).Value;

        private GeoCoords _averageCoords;

        public override IGeoCoords AverageCoords =>
            _averageCoords ?? (_averageCoords = GetAllNodes(this).AverageCoords());

        private List<OsmNode> GetAllNodes(OsmRelation relation)
        {
            var nodes = relation.Nodes.Concat(relation.Ways.SelectMany(i => i.Nodes)).ToList();

            foreach (var rel in relation.Relations)
                nodes.AddRange(GetAllNodes(rel));

            return nodes;
        }

        #endregion

        public void SetMembers(IReadOnlyList<OsmMember> members)
        {
            Debug.Assert(members != null);

            if (members == null)
                throw new ArgumentNullException(nameof(members));

            MemberIds = members.Select(i => i.Geo.Id).ToList();
            Members = members;
            _isBroken = null;
            _averageCoords = null;
        }

        internal OsmRelation(RelationData data) : base(data)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            MemberIds = data.Members.Select(i => i.Id).ToList();
        }

        public OsmRelation(
            long id,
            IReadOnlyDictionary<string, string> tags,
            IReadOnlyList<OsmMember> members,
            IReadOnlyDictionary<string, string> data = null)
            : base(id, tags, data)
        {
            SetMembers(members);
        }
    }
}
