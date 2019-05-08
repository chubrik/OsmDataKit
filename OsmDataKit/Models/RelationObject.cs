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

        public override OsmGeoType Type => OsmGeoType.Relation;

        private bool? _isBroken;

        public override bool IsBroken =>
            _isBroken ?? (_isBroken = Members.Any(i => i.Geo.IsBroken)).Value;

        private GeoCoords _averageCoords;

        public override IGeoCoords AverageCoords =>
            _averageCoords ?? (_averageCoords = this.GetAllNodes().ToList().AverageCoords());

        public void SetMembers(IReadOnlyList<RelationMemberObject> members)
        {
            Debug.Assert(members != null);
            Members = members ?? throw new ArgumentNullException(nameof(members));
            _isBroken = null;
            _averageCoords = null;
        }

        //todo internat ctor
        internal RelationObject(
            long id,
            IReadOnlyDictionary<string, string> tags)
            : base(id, tags, data: null) { }

        public RelationObject(
            long id,
            IReadOnlyList<RelationMemberObject> members,
            IReadOnlyDictionary<string, string> tags = null,
            Dictionary<string, string> data = null)
            : base(id, tags, data)
        {
            SetMembers(members);
        }
    }
}
