using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Kit.Osm
{
    public class OsmRelation : OsmObject
    {
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

        public IEnumerable<OsmNode> AllNodes =>
            Nodes.Concat(Ways.SelectMany(i => i.Nodes))
                 .Concat(Relations.SelectMany(i => i.AllNodes));

        #endregion

        #region Overrides

        public override OsmGeoType Type => OsmGeoType.Relation;

        private bool? _isBroken;

        public override bool IsBroken =>
            _isBroken ?? (_isBroken = Members.Any(i => i.Geo.IsBroken)).Value;

        private GeoCoords _averageCoords;

        public override IGeoCoords AverageCoords =>
            _averageCoords ?? (_averageCoords = AllNodes.ToList().AverageCoords());

        #endregion

        public void SetMembers(IReadOnlyList<OsmMember> members)
        {
            Debug.Assert(members != null);
            Members = members ?? throw new ArgumentNullException(nameof(members));
            _isBroken = null;
            _averageCoords = null;
        }

        internal OsmRelation(RelationData data) : base(data) { }

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
