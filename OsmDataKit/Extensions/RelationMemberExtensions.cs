using OsmDataKit.Models;
using OsmSharp;

namespace OsmDataKit.Extensions
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
