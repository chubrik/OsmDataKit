using Kit;
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

            if (!role.IsNullOrWhiteSpace())
                Role = role;
        }

        public RelationMemberInfo(RelationMember member)
        {
            Type = member.Type;
            Id = member.Id;

            if (!member.Role.IsNullOrWhiteSpace())
                Role = member.Role;
        }

        public static Func<RelationMemberInfo, string> StringFormatter =
            member => member.Role + " - " + member.Type.ToString()[0] + member.Id.ToString();

        public override string ToString() => StringFormatter(this);
    }
}
