﻿namespace OsmDataKit;

using OsmDataKit.Internal;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

[JsonConverter(typeof(RelationObjectConverter))]
public class RelationObject : GeoObject
{
    public override OsmGeoType Type => OsmGeoType.Relation;

    public IReadOnlyList<RelationMemberObject>? Members { get; private set; }

    public IReadOnlyList<RelationMemberInfo>? MissedMembers { get; private set; }

    public RelationObject(
        long id, IReadOnlyList<RelationMemberObject> members, Dictionary<string, string>? tags = null)
        : base(id, tags)
    {
        if (members == null)
            throw new ArgumentNullException(nameof(members));

        if (members.Count == 0)
            throw new ArgumentException(nameof(members));

        Members = members;
    }

    public RelationObject(
        long id, IReadOnlyList<RelationMemberInfo> memberInfos, Dictionary<string, string>? tags = null)
        : base(id, tags)
    {
        if (memberInfos == null)
            throw new ArgumentNullException(nameof(memberInfos));

        if (memberInfos.Count == 0)
            throw new ArgumentException(nameof(memberInfos));

        MissedMembers = memberInfos;
    }

    public RelationObject(Relation relation) : base(relation)
    {
        MissedMembers = relation.Members.Select(i => new RelationMemberInfo(i)).ToList();
    }

    internal void FillMembers(IReadOnlyList<RelationMemberObject> members)
    {
        Debug.Assert(Members == null);
        Debug.Assert(members.Count > 0);

        Members = members ?? throw new ArgumentNullException(nameof(members));

        var missedDict = new Dictionary<(string? role, OsmGeoType type, long id), RelationMemberInfo>(members.Count);

        foreach (var missedMember in MissedMembers!)
            if (!missedDict.ContainsKey((missedMember.Role, missedMember.Type, missedMember.Id)))
                missedDict.Add((missedMember.Role, missedMember.Type, missedMember.Id), missedMember);

        foreach (var member in members)
            missedDict.Remove((member.Role, member.Type, member.Id));

        MissedMembers = missedDict.Count > 0 ? missedDict.Values.ToList() : null;
    }
}
