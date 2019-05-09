using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OsmDataKit
{
    public class RelationObject : GeoObject
    {
        public override OsmGeoType Type => OsmGeoType.Relation;

        public IReadOnlyList<RelationMemberObject> Members { get; internal set; }

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

        private bool? _isBroken;
        public bool IsBroken => _isBroken ?? (_isBroken = this.GetIsBroken()).Value;

        public void SetMembers(IReadOnlyList<RelationMemberObject> members)
        {
            Debug.Assert(members?.Count > 0);

            Members = members ?? throw new ArgumentNullException(nameof(members));
            _isBroken = null;
        }
    }
}
