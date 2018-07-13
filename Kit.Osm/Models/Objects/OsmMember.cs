using System;
using System.Diagnostics;

namespace Kit.Osm
{
    [DebuggerDisplay("{" + nameof(DebugInfo) + "(),nq}")]
    public class OsmMember
    {
        public string Role { get; }
        public OsmObject Geo { get; }

        internal OsmMember(RelationMemberData data, OsmObject geo)
        {
            Debug.Assert(data != null);

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Debug.Assert(geo?.Type == data.Type);

            if (geo.Type != data.Type)
                throw new ArgumentException(nameof(geo));

            Role = data.Role;
            Geo = geo;
        }

        public OsmMember(string role, OsmObject geo)
        {
            Debug.Assert(!role.IsNullOrEmpty());
            Debug.Assert(geo != null);

            if (role.IsNullOrEmpty())
                throw new ArgumentException(nameof(role));

            Role = role;
            Geo = geo ?? throw new ArgumentNullException(nameof(geo));
        }

#if DEBUG
        private string DebugInfo() =>
            Geo.Type.ToString()[0] + Geo.Id.ToString() + " - " + Role + " - " + Geo.Title;
#endif
    }
}
