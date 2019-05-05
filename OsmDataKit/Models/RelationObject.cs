using OsmDataKit.Internal;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmDataKit
{
    public class RelationObject : GeoObject
    {
        public IReadOnlyList<RelationMemberObject> Members { get; internal set; }

        #region Extensions

        public IEnumerable<NodeObject> Nodes =>
            Members.Where(i => i.Geo.Type == OsmGeoType.Node)
                   .Select(i => (NodeObject)i.Geo);

        public IEnumerable<WayObject> Ways =>
            Members.Where(i => i.Geo.Type == OsmGeoType.Way)
                   .Select(i => (WayObject)i.Geo);

        public IEnumerable<RelationObject> Relations =>
            Members.Where(i => i.Geo.Type == OsmGeoType.Relation)
                   .Select(i => (RelationObject)i.Geo);

        public IEnumerable<NodeObject> AllNodes =>
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

        public void SetMembers(IReadOnlyList<RelationMemberObject> members)
        {
            Debug.Assert(members != null);
            Members = members ?? throw new ArgumentNullException(nameof(members));
            _isBroken = null;
            _averageCoords = null;
        }

        internal RelationObject(RelationData data) : base(data) { }

        public RelationObject(
            long id,
            IReadOnlyDictionary<string, string> tags,
            IReadOnlyList<RelationMemberObject> members,
            IReadOnlyDictionary<string, string> data = null)
            : base(id, tags, data)
        {
            SetMembers(members);
        }
    }
}
