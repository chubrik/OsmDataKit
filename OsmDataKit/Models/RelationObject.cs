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

        internal RelationObject(Relation relation) : base(relation)
        {
            MissedMembers = relation.Members.Select(i =>
                new RelationMemberInfo { Role = i.Role, Type = i.Type, Id = i.Id })
                .ToList();
        }

        internal void SetMembers(IReadOnlyList<RelationMemberObject> members)
        {
            Debug.Assert(Members == null);
            Debug.Assert(members?.Count > 0);

            Members = members ?? throw new ArgumentNullException(nameof(members));

            var missedDict = MissedMembers.ToDictionary(i => i.Id);

            foreach (var member in members)
                if (!missedDict.Remove(member.Id))
                    throw new InvalidOperationException();

            MissedMembers = missedDict.Count > 0 ? missedDict.Values.ToList() : null;
        }
    }
}
