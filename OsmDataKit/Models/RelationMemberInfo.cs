using OsmSharp;
using System;

namespace OsmDataKit
{
    public class RelationMemberInfo
    {
        public OsmGeoType Type { get; }

        public long Id { get; }

        public string? Role { get; }

        public string Url => $"https://www.openstreetmap.org/{Type.ToString().ToLower()}/{Id}";

        public RelationMemberInfo(OsmGeoType type, long id, string? role)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            Type = type;
            Id = id;

            if (!string.IsNullOrWhiteSpace(role))
                Role = role;
        }

        public RelationMemberInfo(RelationMember member)
        {
            Type = member.Type;
            Id = member.Id;

            if (!string.IsNullOrWhiteSpace(member.Role))
                Role = member.Role;
        }

        public static Func<RelationMemberInfo, string> StringFormatter =
            member => member.Role + " - " + member.Type.ToString()[0] + member.Id.ToString();

        public override string ToString() => StringFormatter(this);
    }
}
