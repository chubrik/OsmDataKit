using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmDataKit
{
    public class RelationObject : GeoObject
    {
        public override OsmGeoType Type => OsmGeoType.Relation;

        public IReadOnlyList<RelationMemberObject> Members { get; private set; }

        public IReadOnlyList<RelationMember> MissedMembersInfo { get; private set; }

        internal RelationObject(Relation relation) : base(relation)
        {
            MissedMembersInfo = relation.Members;
        }

        ////todo internat ctor
        //internal RelationObject(
        //    long id,
        //    IReadOnlyDictionary<string, string> tags)
        //    : base(id, tags, data: null) { }

        //public RelationObject(
        //    long id,
        //    IReadOnlyList<RelationMemberObject> members,
        //    IReadOnlyDictionary<string, string> tags = null,
        //    Dictionary<string, string> data = null)
        //    : base(id, tags, data)
        //{
        //    SetMembers(members);
        //}

        //private bool? _isBroken;
        //public bool IsBroken => _isBroken ?? (_isBroken = this.GetIsBroken()).Value;

        internal void SetMembers(IReadOnlyList<RelationMemberObject> members)
        {
            Debug.Assert(members?.Count > 0);
            Members = members ?? throw new ArgumentNullException(nameof(members));

            var missedDict = MissedMembersInfo.ToDictionary(i => i.Id);

            foreach (var member in members)
                if (!missedDict.Remove(member.Id))
                    throw new InvalidOperationException();

            MissedMembersInfo = missedDict.Count > 0 ? missedDict.Values.ToList() : null;
        }
    }
}
