using Newtonsoft.Json;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OsmDataKit
{
    [JsonObject]
    public class RelationObject : GeoObject
    {
        public override OsmGeoType Type => OsmGeoType.Relation;

        [JsonIgnore]
        public IReadOnlyList<RelationMemberObject> Members { get; set; }

        [JsonProperty("m")]
        public IReadOnlyList<RelationMemberInfo> MissedMembers { get; set; }

        public RelationObject() { }

        public RelationObject(
            long id, IReadOnlyList<RelationMemberObject> members, Dictionary<string, string> tags = null)
            : base(id, tags)
        {
            Debug.Assert(members != null);
            Debug.Assert(members?.Count > 0);

            if (members == null)
                throw new ArgumentNullException(nameof(members));

            if (members.Count == 0)
                throw new ArgumentException(nameof(members));

            Members = members;
        }

        internal RelationObject(Relation relation) : base(relation)
        {
            MissedMembers = relation.Members.Select(i =>
                new RelationMemberInfo { Role = i.Role, Type = i.Type, Id = i.Id })
                .ToList();
        }

        public void ReplaceMembers(IReadOnlyList<RelationMemberObject> members)
        {
            Debug.Assert(members != null);
            Debug.Assert(members?.Count > 0);

            Members = members ?? throw new ArgumentNullException(nameof(members));
            MissedMembers = null;
        }

        internal void FillMembers(IReadOnlyList<RelationMemberObject> members)
        {
            Debug.Assert(Members == null);
            Debug.Assert(members != null);
            Debug.Assert(members?.Count > 0);

            Members = members ?? throw new ArgumentNullException(nameof(members));

            var missedDict = new Dictionary<(string, OsmGeoType, long), RelationMemberInfo>(members.Count);

            foreach (var missedMember in MissedMembers)
                if (!missedDict.ContainsKey((missedMember.Role, missedMember.Type, missedMember.Id)))
                    missedDict.Add((missedMember.Role, missedMember.Type, missedMember.Id), missedMember);

            foreach (var member in members)
                missedDict.Remove((member.Role, member.Type, member.Id));

            MissedMembers = missedDict.Count > 0 ? missedDict.Values.ToList() : null;
        }
    }
}
