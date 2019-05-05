using OsmSharp;

namespace OsmDataKit.Internal
{
    internal static class RelationMemberExtensions
    {
        public static RelationMemberData ToData(this RelationMember member) =>
            new RelationMemberData
            {
                Id = member.Id,
                Type = member.Type,
                Role = member.Role
            };
    }
}
